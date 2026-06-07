import {LibraryType} from "../library/library";

import {ScrobbleEventType} from "../scrobbling/scrobble-event";
import {ScrobbleReadStatus} from "./scrobble-providers/scrobble-read-status.enum";
import {ScrobbleProvider} from "./scrobble-providers/scrobble-provider.enum";

export interface KavitaPlusScrobbleDetails {
  scrobbleEventType: ScrobbleEventType | null;
  chapterNumber: number | null;
  volumeNumber: number | null;
  percentRead: number | null;
  rating: number | null;
  provider: ScrobbleProvider;
  libraryType: LibraryType;
  readStatus: ScrobbleReadStatus | null;
}
