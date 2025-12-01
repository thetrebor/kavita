export interface AuthKey {
  id: number;
  key: string;
  name: string;
  createdAtUtc: string;
  expiresAtUtc: string;
  lastAccessedAtUtc: string;
  provider: AuthKeyProvider;
}

export enum AuthKeyProvider {
  User = 0,
  System = 1
}

