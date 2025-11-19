import {AgeRestriction} from '../metadata/age-restriction';
import {Preferences} from '../preferences/preferences';
import {IHasCover} from "../common/i-has-cover";

// This interface is only used for login and storing/retrieving JWT from local storage
export interface User extends IHasCover {
  id: number;
  username: string;
  token: string;
  refreshToken: string;
  roles: string[];
  preferences: Preferences;
  apiKey: string;
  email: string;
  ageRestriction: AgeRestriction;
  hasRunScrobbleEventGeneration: boolean;
  scrobbleEventGenerationRan: string; // datetime
  identityProvider: IdentityProvider;

  coverImage?: string;
  primaryColor: string;
  secondaryColor: string;
}

export enum IdentityProvider {
  Kavita = 0,
  OpenIdConnect = 1,
}

export const IdentityProviders: IdentityProvider[] = [IdentityProvider.Kavita, IdentityProvider.OpenIdConnect];
