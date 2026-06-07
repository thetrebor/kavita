import {ScrobbleProvider} from "../kavitaplus/scrobble-providers/scrobble-provider.enum";

export interface ExternalSeries {
  name: string;
  coverUrl: string;
  url: string;
  summary: string;
  aniListId?: number;
  malId?: number;
  provider: ScrobbleProvider;
}
