import {ClientInfo} from "../_services/client-info.service";

/**
 * Represents a physical device a client is using to interact with Kavita
 */
export interface ClientDevice {
  friendlyName: string;
  currentClientInfo: ClientInfo;
  firstSeenUtc: Date;
  lastSeenUtc: Date;
  ownerUsername: string;
  ownerUserId: number;
}
