const fs = require('fs');
const path = require('path');
const glob = require('glob');

// ─── Helpers ────────────────────────────────────────────────────────────────

function flattenKeys(obj, prefix = '') {
  const result = new Map();
  for (const [key, value] of Object.entries(obj)) {
    const fullKey = prefix ? `${prefix}.${key}` : key;
    if (typeof value === 'object' && value !== null) {
      for (const [k, v] of flattenKeys(value, fullKey)) {
        result.set(k, v);
      }
    } else if (typeof value === 'string') {
      result.set(fullKey, value);
    }
  }
  return result;
}

// Matches cross-references like {{common.save}} but NOT runtime params like {{num}}
// Cross-refs have a dot in the identifier; params do not
const CROSS_REF_RE = /\{\{([\w-]+\.[\w-]+(?:\.[\w-]+)*)\}\}/g;

function isCrossRef(value) {
  return /\{\{[\w-]+\.[\w-]+(?:\.[\w-]+)*\}\}/.test(value);
}

function extractCrossRefs(value) {
  const refs = [];
  let match;
  const re = new RegExp(CROSS_REF_RE.source, 'g');
  while ((match = re.exec(value)) !== null) {
    refs.push(match[1]);
  }
  return refs;
}

// ─── 1. Duplicate Value Finder ──────────────────────────────────────────────

function findDuplicateValues(flatMap) {
  const valueToKeys = new Map();
  for (const [key, value] of flatMap) {
    // Skip cross-references and short strings
    if (isCrossRef(value) || value.length <= 3) continue;
    // Skip values that are pure parameter interpolation
    if (/^\{\{[^.]+\}\}$/.test(value)) continue;

    if (!valueToKeys.has(value)) {
      valueToKeys.set(value, []);
    }
    valueToKeys.get(value).push(key);
  }

  const duplicates = [];
  for (const [value, keys] of valueToKeys) {
    if (keys.length > 1) {
      // Check if any key is already in common.*
      const commonKey = keys.find(k => k.startsWith('common.'));
      duplicates.push({
        value,
        count: keys.length,
        keys,
        suggestion: commonKey
          ? `Already in common: ${commonKey} — other keys can use {{${commonKey}}}`
          : `Consider adding to common.* and referencing via {{common.xxx}}`
      });
    }
  }
  return duplicates.sort((a, b) => b.count - a.count);
}

// ─── 2. Cross-Reference Validator ───────────────────────────────────────────

function validateCrossRefs(flatMap) {
  const broken = [];
  const circular = [];

  // Build ref graph
  const graph = new Map();
  for (const [key, value] of flatMap) {
    const refs = extractCrossRefs(value);
    if (refs.length > 0) {
      graph.set(key, refs);
    }
  }

  // Check broken refs
  for (const [key, refs] of graph) {
    for (const ref of refs) {
      if (!flatMap.has(ref)) {
        broken.push({ key, ref, value: flatMap.get(key) });
      }
    }
  }

  // Check circular refs via DFS
  function hasCycle(startKey, visited = new Set(), path = new Set()) {
    if (path.has(startKey)) return true;
    if (visited.has(startKey)) return false;

    visited.add(startKey);
    path.add(startKey);

    const refs = graph.get(startKey) || [];
    for (const ref of refs) {
      if (hasCycle(ref, visited, path)) {
        circular.push({ key: startKey, cycle: [...path, ref].join(' → ') });
        return true;
      }
    }
    path.delete(startKey);
    return false;
  }

  const visited = new Set();
  for (const key of graph.keys()) {
    if (!visited.has(key)) {
      hasCycle(key, visited, new Set());
    }
  }

  return { broken, circular };
}

// ─── 3. Dead Key Finder ─────────────────────────────────────────────────────

function findUsedKeys(srcDir) {
  const usedKeys = new Set();

  // Scan HTML files
  const htmlFiles = glob.sync('**/*.html', { cwd: srcDir, absolute: true });
  for (const file of htmlFiles) {
    const content = fs.readFileSync(file, 'utf8');

    // Find transloco prefix directives
    const prefixRe = /\*transloco\s*=\s*"[^"]*prefix\s*:\s*'([^']+)'/g;
    let prefixMatch;
    const prefixes = [];
    while ((prefixMatch = prefixRe.exec(content)) !== null) {
      prefixes.push(prefixMatch[1]);
    }

    // Find t('key') calls in template
    const tCallRe = /t\(\s*'([^']+)'\s*\)/g;
    let tMatch;
    while ((tMatch = tCallRe.exec(content)) !== null) {
      const key = tMatch[1];
      if (key.includes('.')) {
        // Fully qualified key
        usedKeys.add(key);
      } else {
        // Scoped key — combine with each prefix found in this file
        for (const prefix of prefixes) {
          usedKeys.add(`${prefix}.${key}`);
        }
      }
    }

    // Also find transloco pipe usage: 'key' | transloco
    const pipeRe = /'([^']+)'\s*\|\s*transloco/g;
    let pipeMatch;
    while ((pipeMatch = pipeRe.exec(content)) !== null) {
      usedKeys.add(pipeMatch[1]);
    }
  }

  // Scan TS files
  const tsFiles = glob.sync('**/*.ts', { cwd: srcDir, absolute: true, ignore: ['**/*.spec.ts', '**/node_modules/**'] });
  for (const file of tsFiles) {
    const content = fs.readFileSync(file, 'utf8');

    // translate('full.key') or translocoService.translate('full.key')
    const translateRe = /translate\(\s*'([^']+)'/g;
    let trMatch;
    while ((trMatch = translateRe.exec(content)) !== null) {
      usedKeys.add(trMatch[1]);
    }

    // Also: translate("full.key")
    const translateDqRe = /translate\(\s*"([^"]+)"/g;
    while ((trMatch = translateDqRe.exec(content)) !== null) {
      usedKeys.add(trMatch[1]);
    }

    // selectTranslate('full.key')
    const selectRe = /selectTranslate\(\s*'([^']+)'/g;
    while ((trMatch = selectRe.exec(content)) !== null) {
      usedKeys.add(trMatch[1]);
    }
  }

  return usedKeys;
}

function findDeadKeys(flatMap, srcDir, allowlistPath) {
  const usedKeys = findUsedKeys(srcDir);

  // Load dynamic key allowlist
  let allowlist = [];
  if (fs.existsSync(allowlistPath)) {
    allowlist = JSON.parse(fs.readFileSync(allowlistPath, 'utf8'));
  }

  // Build set of keys that are referenced via cross-refs (transitively used)
  const referencedKeys = new Set();
  for (const [, value] of flatMap) {
    for (const ref of extractCrossRefs(value)) {
      referencedKeys.add(ref);
    }
  }

  const deadKeys = [];
  for (const [key] of flatMap) {
    // Skip if directly used in code
    if (usedKeys.has(key)) continue;
    // Skip if referenced by another key via cross-ref
    if (referencedKeys.has(key)) continue;
    // Skip if covered by dynamic allowlist prefix
    if (allowlist.some(prefix => key.startsWith(prefix))) continue;

    deadKeys.push(key);
  }

  return deadKeys.sort();
}

// ─── 4. Key Sync Report ─────────────────────────────────────────────────────

function syncReport(enFlat, langDir) {
  const localeFiles = fs.readdirSync(langDir)
    .filter(f => f.endsWith('.json') && f !== 'en.json');

  const report = {};
  for (const file of localeFiles) {
    const locale = file.replace('.json', '');
    const data = JSON.parse(fs.readFileSync(path.join(langDir, file), 'utf8'));
    const localeFlat = flattenKeys(data);

    const missing = [];
    const empty = [];
    for (const [key, value] of enFlat) {
      if (!localeFlat.has(key)) {
        // Skip keys that are cross-refs (they should be identical across locales)
        if (!isCrossRef(value)) {
          missing.push(key);
        }
      } else if (localeFlat.get(key) === '' && !isCrossRef(value)) {
        empty.push(key);
      }
    }

    const extra = [];
    for (const [key] of localeFlat) {
      if (!enFlat.has(key)) {
        extra.push(key);
      }
    }

    report[locale] = {
      totalKeys: localeFlat.size,
      missing: missing.length,
      empty: empty.length,
      extra: extra.length,
      missingKeys: missing,
      emptyKeys: empty,
      extraKeys: extra
    };
  }
  return report;
}

// ─── Main ───────────────────────────────────────────────────────────────────

function main() {
  const webDir = path.resolve(__dirname);
  const langDir = path.join(webDir, 'src', 'assets', 'langs');
  const srcDir = path.join(webDir, 'src', 'app');
  const enPath = path.join(langDir, 'en.json');
  const allowlistPath = path.join(webDir, 'i18n-dynamic-keys.json');
  const reportPath = path.join(webDir, 'i18n-audit-report.json');

  if (!fs.existsSync(enPath)) {
    console.error('en.json not found at', enPath);
    process.exit(1);
  }

  const enData = JSON.parse(fs.readFileSync(enPath, 'utf8'));
  const enFlat = flattenKeys(enData);

  console.log(`\n📊 i18n Audit Report`);
  console.log(`${'─'.repeat(60)}`);
  console.log(`Total keys in en.json: ${enFlat.size}\n`);

  // 1. Duplicates
  console.log('1️⃣  Duplicate Values');
  const duplicates = findDuplicateValues(enFlat);
  console.log(`   Found ${duplicates.length} duplicate string values`);
  const topDupes = duplicates.slice(0, 10);
  for (const d of topDupes) {
    console.log(`   "${d.value}" appears ${d.count}x — ${d.suggestion}`);
  }
  if (duplicates.length > 10) {
    console.log(`   ... and ${duplicates.length - 10} more (see report)`);
  }

  // 2. Cross-reference validation
  console.log('\n2️⃣  Cross-Reference Validation');
  const { broken, circular } = validateCrossRefs(enFlat);
  console.log(`   Broken refs: ${broken.length}`);
  for (const b of broken.slice(0, 10)) {
    console.log(`   ❌ ${b.key} references {{${b.ref}}} — target does not exist`);
  }
  if (broken.length > 10) {
    console.log(`   ... and ${broken.length - 10} more (see report)`);
  }
  console.log(`   Circular refs: ${circular.length}`);
  for (const c of circular) {
    console.log(`   🔄 ${c.cycle}`);
  }

  // 3. Dead keys
  console.log('\n3️⃣  Dead Keys (not found in source code)');
  const deadKeys = findDeadKeys(enFlat, srcDir, allowlistPath);
  console.log(`   Found ${deadKeys.length} potentially dead keys`);
  for (const k of deadKeys.slice(0, 15)) {
    console.log(`   🪦 ${k}`);
  }
  if (deadKeys.length > 15) {
    console.log(`   ... and ${deadKeys.length - 15} more (see report)`);
  }

  // 4. Sync report
  console.log('\n4️⃣  Locale Sync Report');
  const sync = syncReport(enFlat, langDir);
  const locales = Object.keys(sync).sort((a, b) => sync[b].missing - sync[a].missing);
  console.log(`   ${'Locale'.padEnd(10)} ${'Total'.padStart(6)} ${'Missing'.padStart(8)} ${'Empty'.padStart(8)} ${'Extra'.padStart(8)}`);
  console.log(`   ${'─'.repeat(42)}`);
  for (const locale of locales) {
    const s = sync[locale];
    console.log(`   ${locale.padEnd(10)} ${String(s.totalKeys).padStart(6)} ${String(s.missing).padStart(8)} ${String(s.empty).padStart(8)} ${String(s.extra).padStart(8)}`);
  }

  // Write full report
  const report = {
    generatedAt: new Date().toISOString(),
    totalKeys: enFlat.size,
    duplicates,
    crossRefs: { broken, circular },
    deadKeys,
    localeSync: sync
  };

  fs.writeFileSync(reportPath, JSON.stringify(report, null, 2));
  console.log(`\n✅ Full report written to ${path.relative(webDir, reportPath)}`);
}

main();
