import {LibraryType} from "./library/library";
import {MangaFormat} from "./manga-format";
import {IHasCover} from "./common/i-has-cover";
import {AgeRating} from "./metadata/age-rating";
import {IHasReadingTime} from "./common/i-has-reading-time";
import {IHasCast} from "./common/i-has-cast";
import {IHasDisplayTitle} from "./common/i-has-display-title";

export interface ReadingListItem extends IHasDisplayTitle{
  /**
   * Pre-computed display designation, e.g. "Chapter 5", "Issue #5"
   */
  displayNumber: string;
  /**
   * Pre-computed full display title
   */
  displayTitle: string;
  pagesRead: number;
  pagesTotal: number;
  seriesName: string;
  seriesSortName: string;
  seriesFormat: MangaFormat;
  seriesId: number;
  chapterId: number;
  order: number;
  chapterNumber: string;
  volumeNumber: string;
  libraryId: number;
  id: number;
  releaseDate: string;
  title: string;
  libraryType: LibraryType;
  libraryName: string;
  summary?: string;
}

export interface ReadingList extends IHasCover {
  id: number;
  title: string;
  summary: string;
  promoted: boolean;
  coverImageLocked: boolean;
  items: Array<ReadingListItem>;
  /**
   * If this is empty or null, the cover image isn't set. Do not use this externally.
  */
  coverImage?: string;
  primaryColor: string;
  secondaryColor: string;
  startingYear: number;
  startingMonth: number;
  endingYear: number;
  endingMonth: number;
  itemCount: number;
  ageRating: AgeRating;
}

export interface ReadingListInfo extends IHasReadingTime, IHasReadingTime {
  pages: number;
  wordCount: number;
  isAllEpub: boolean;
  minHoursToRead: number;
  maxHoursToRead: number;
  avgHoursToRead: number;
}

export interface ReadingListCast extends IHasCast {}
