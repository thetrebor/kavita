import {AgeRating} from "../metadata/age-rating";
import {PublicationStatus} from "../metadata/publication-status";
import {ScrobbleProvider} from "../../_services/scrobbling.service";

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

export type ScrobbleProviderSettings = {
  progressScrobbling: boolean;
  wantToReadSync: boolean;
  ratingScrobbling: boolean;
  reviewScrobbling: boolean;
  reviewScrobbleTarget: ReviewScrobbleTarget;
  allLibraries: boolean;
  libraries: number[];
  highestAgeRating: AgeRating;
  inactiveSeriesRule: ReadStatusTransitionRule;
  droppedSeriesRule: ReadStatusTransitionRule;
}

export enum ReviewScrobbleTarget {
  Private = 0,
  Friends = 1,
  Public = 2,
}

export enum ScrobbleReadStatus {
  Ignore = 0,
  WantToRead = 1,
  Read = 2,
  UnRead = 3,
  Dropped = 4,
  OnHold = 5,
}

export type ReadStatusTransitionRule = {
  enabled: boolean;
  days: number;
  transitionStatus: ScrobbleReadStatus;
  excludedPublicationStatus: PublicationStatus[];
}

export const DEFAULT_SCROBBLE_PROVIDER_SETTINGS: ScrobbleProviderSettings = {
  allLibraries: false,
  droppedSeriesRule: {
    enabled: false,
    days: 90,
    transitionStatus: ScrobbleReadStatus.Dropped,
    excludedPublicationStatus: [PublicationStatus.OnGoing]
  },
  highestAgeRating: AgeRating.NotApplicable,
  inactiveSeriesRule: {
    enabled: false,
    days: 30,
    transitionStatus: ScrobbleReadStatus.OnHold,
    excludedPublicationStatus: []
  },
  libraries: [],
  progressScrobbling: true,
  ratingScrobbling: true,
  reviewScrobbleTarget: ReviewScrobbleTarget.Private,
  reviewScrobbling: true,
  wantToReadSync: true
};
