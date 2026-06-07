import {ScrobbleProvider} from "./scrobble-provider.enum";

export type UpdateScrobbleProvider = {
  provider: ScrobbleProvider;
  userName: string;
  authenticationToken: string;
}
