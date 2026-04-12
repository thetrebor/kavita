import {EntityKind} from './entity-kind.enum';
import {SeriesGroup} from '../series-group';
import {ReadingList} from '../reading-list/reading-list';

/**
 * Mixed dashboard stream entry. Exactly one of `series` / `readingList` is populated
 * based on `kind`. Items are ordered by `updatedUtc` across types.
 */
export interface RecentlyUpdatedItem {
  kind: EntityKind;
  updatedUtc: string;
  series?: SeriesGroup;
  readingList?: ReadingList;
}
