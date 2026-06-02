import {AgeRating} from "../../metadata/age-rating";
import {PublicationStatus} from "../../metadata/publication-status";
import {ReviewScrobbleTarget} from "./review-scrobble-target.enum";
import {ScrobbleReadStatus} from "./scrobble-read-status.enum";
import {ReadStatusTransitionRule} from "./read-status-transition-rule";

export type ScrobbleProviderSettings = {
  progressScrobbling: boolean;
  wantToReadSync: boolean;
  ratingScrobbling: boolean;
  reviewsScrobbling: boolean;
  reviewScrobbleTarget: ReviewScrobbleTarget;
  allLibraries: boolean;
  libraries: number[];
  highestAgeRating: AgeRating;
  inactiveSeriesRule: ReadStatusTransitionRule;
  droppedSeriesRule: ReadStatusTransitionRule;
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
  reviewsScrobbling: true,
  wantToReadSync: true
};
