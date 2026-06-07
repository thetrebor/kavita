import {ScrobbleProvider} from "./scrobble-providers/scrobble-provider.enum";

export interface UserTokenInfo {
  userId: number;
  username: string;
  tokens: TokenValidityInfo[];
}

export interface TokenValidityInfo {
  provider: ScrobbleProvider;
  validUntilUtc: string;
}
