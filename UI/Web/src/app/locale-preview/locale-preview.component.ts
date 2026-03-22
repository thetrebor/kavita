import {ChangeDetectionStrategy, Component, computed, inject, OnInit, signal} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {TranslocoDirective, TranslocoService} from '@jsverse/transloco';
import {FormsModule} from '@angular/forms';
import {forkJoin} from 'rxjs';
import {KavitaLocale} from "../_models/metadata/language";
import {LocalizationService} from "../_services/localization.service";
import {DecimalPipe} from "@angular/common";

interface KeyEntry {
  key: string;
  enValue: string;
  enResolved: string;
  enChain: string[];
  localeValue: string | null;
  localeResolved: string | null;
  localeChain: string[];
  status: 'resolved' | 'missing' | 'empty' | 'broken-ref';
}

@Component({
  selector: 'app-locale-preview',
  imports: [TranslocoDirective, FormsModule, DecimalPipe],
  templateUrl: './locale-preview.component.html',
  styleUrl: './locale-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LocalePreviewComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly translocoService = inject(TranslocoService);
  private readonly localizationService = inject(LocalizationService);

  languages = signal<KavitaLocale[]>([]);
  entries = signal<KeyEntry[]>([]);
  activeLang = signal<string>('en');
  loading = signal(true);
  searchQuery = signal('');

  hideResolved = signal(false);
  showOnlyMissing = signal(false);
  showOnlyBroken = signal(false);
  showOnlyEmpty = signal(false);

  stats = computed(() => {
    const all = this.entries();
    return {
      total: all.length,
      resolved: all.filter(e => e.status === 'resolved').length,
      missing: all.filter(e => e.status === 'missing').length,
      empty: all.filter(e => e.status === 'empty').length,
      broken: all.filter(e => e.status === 'broken-ref').length,
    };
  });

  filteredEntries = computed(() => {
    let result = this.entries();
    const query = this.searchQuery().toLowerCase().trim();

    if (this.hideResolved()) {
      result = result.filter(e => e.status !== 'resolved');
    }
    if (this.showOnlyMissing()) {
      result = result.filter(e => e.status === 'missing');
    }
    if (this.showOnlyBroken()) {
      result = result.filter(e => e.status === 'broken-ref');
    }
    if (this.showOnlyEmpty()) {
      result = result.filter(e => e.status === 'empty');
    }
    if (query) {
      result = result.filter(e =>
        e.key.toLowerCase().includes(query) ||
        e.enValue.toLowerCase().includes(query) ||
        (e.localeValue?.toLowerCase().includes(query) ?? false)
      );
    }

    return result;
  });

  ngOnInit() {
    const lang = this.translocoService.getActiveLang();
    this.activeLang.set(lang);
    this.loadData(lang);

    this.localizationService.getLocales().subscribe(langs => {
      this.languages.set(langs);
    });
  }

  onLocaleChange(lang: string) {
    this.activeLang.set(lang);
    this.loading.set(true);
    this.loadData(lang);
  }

  private loadData(lang: string) {
    const en$ = this.http.get<Record<string, any>>('assets/langs/en.json');
    const locale$ = lang === 'en'
      ? this.http.get<Record<string, any>>('assets/langs/en.json')
      : this.http.get<Record<string, any>>(`assets/langs/${lang}.json`);

    forkJoin([en$, locale$]).subscribe({
      next: ([enData, localeData]) => {
        const enFlat = this.flatten(enData);
        const localeFlat = this.flatten(localeData);
        this.buildEntries(enFlat, localeFlat);
        this.loading.set(false);
      },
      error: () => {
        // If locale file doesn't exist, show English only
        const en$ = this.http.get<Record<string, any>>('assets/langs/en.json');
        en$.subscribe(enData => {
          const enFlat = this.flatten(enData);
          this.buildEntries(enFlat, new Map());
          this.loading.set(false);
        });
      }
    });
  }

  private flatten(obj: Record<string, any>, prefix = ''): Map<string, string> {
    const result = new Map<string, string>();
    for (const [key, value] of Object.entries(obj)) {
      const fullKey = prefix ? `${prefix}.${key}` : key;
      if (typeof value === 'object' && value !== null) {
        for (const [k, v] of this.flatten(value, fullKey)) {
          result.set(k, v);
        }
      } else if (typeof value === 'string') {
        result.set(fullKey, value);
      }
    }
    return result;
  }

  private resolve(value: string, flatMap: Map<string, string>, depth = 0): { resolved: string; chain: string[] } {
    const chain: string[] = [value];
    if (depth > 10) return {resolved: value, chain};

    const crossRefRe = /\{\{([\w-]+\.[\w-]+(?:\.[\w-]+)*)\}\}/g;
    let current = value;
    let match;
    let iterations = 0;

    while ((match = crossRefRe.exec(current)) !== null && iterations < 10) {
      const ref = match[1];
      const target = flatMap.get(ref);
      if (target === undefined) {
        chain.push(`{{${ref}}} [BROKEN]`);
        break;
      }
      current = current.replace(match[0], target);
      chain.push(current);
      crossRefRe.lastIndex = 0;
      iterations++;
    }

    return {resolved: current, chain};
  }

  private isCrossRef(value: string): boolean {
    return /\{\{[\w-]+\.[\w-]+(?:\.[\w-]+)*\}\}/.test(value);
  }

  private buildEntries(enFlat: Map<string, string>, localeFlat: Map<string, string>) {
    const entries: KeyEntry[] = [];

    for (const [key, enValue] of enFlat) {
      const localeValue = localeFlat.get(key) ?? null;
      const enResolution = this.resolve(enValue, enFlat);
      const localeResolution = localeValue !== null
        ? this.resolve(localeValue, localeFlat)
        : {resolved: null, chain: []};

      let status: KeyEntry['status'] = 'resolved';
      if (localeValue === null) {
        status = 'missing';
      } else if (localeValue === '' && !this.isCrossRef(enValue)) {
        status = 'empty';
      } else if (
        enResolution.chain.some(c => c.includes('[BROKEN]')) ||
        localeResolution.chain.some(c => c.includes('[BROKEN]'))
      ) {
        status = 'broken-ref';
      }

      entries.push({
        key,
        enValue,
        enResolved: enResolution.resolved,
        enChain: enResolution.chain.length > 1 ? enResolution.chain : [],
        localeValue,
        localeResolved: localeResolution.resolved,
        localeChain: localeResolution.chain.length > 1 ? localeResolution.chain : [],
        status
      });
    }

    this.entries.set(entries);
  }

  getAvailableLocales(): string[] {
    return ['ar','ca','cs','da','de','el','en','es','et','fi','fr','ga','hi','hr','hu',
      'id','it','ja','ko','lt','ms','nb_NO','nl','pl','pt','pt_BR','ru','sk','sl','sv',
      'ta','th','tr','uk','vi','zh_Hans','zh_Hant'];
  }

  getBadgeClass(status: string): string {
    switch (status) {
      case 'missing': return 'bg-warning text-dark';
      case 'empty': return 'bg-info text-dark';
      case 'broken-ref': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }
}
