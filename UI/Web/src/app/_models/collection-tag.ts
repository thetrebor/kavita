import {AgeRating} from "./metadata/age-rating";
import {ScrobbleProvider} from "./kavitaplus/scrobble-providers/scrobble-provider.enum";

export interface UserCollection {
  id: number;
  title: string;
  promoted: boolean;
  /**
   * This is used as a placeholder to store the coverImage url. The backend does not use this or send it.
   */
  coverImage: string;
  coverImageLocked: boolean;
  summary: string;
  lastSyncUtc: string;
  owner: string;
  source: ScrobbleProvider;
  sourceUrl: string | null;
  totalSourceCount: number;
  /**
   * HTML anchors separated by <br/>
   */
  missingSeriesFromSource: string | null;
  ageRating: AgeRating;
  itemCount: number;
}
