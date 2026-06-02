import {ScrobbleProvider} from "../../../_services/scrobbling.service";
import {ScrobbleProviderSettings} from "./scrobble-provider-settings";

export type UserScrobbleProvider = {
  provider: ScrobbleProvider;
  userName: string;
  authenticationToken: string;
  validUntilUtc: string;
  lastSyncedUtc: string;
  hasRunScrobbleEventGeneration: boolean;
  scrobbleEventGenerationRan: string;
  settings: ScrobbleProviderSettings;
}
