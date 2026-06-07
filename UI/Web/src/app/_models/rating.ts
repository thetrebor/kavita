import {ScrobbleProvider} from "./kavitaplus/scrobble-providers/scrobble-provider.enum";

export enum RatingAuthority {
  User = 0,
  Critic = 1,
}

export interface Rating {
  averageScore: number;
  meanScore: number;
  favoriteCount: number;
  provider: ScrobbleProvider;
  providerUrl: string | undefined;
  authority: RatingAuthority;
}
