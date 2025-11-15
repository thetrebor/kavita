import {StatBucket} from "./stat-bucket";

export interface PageSpreadStats {
  buckets: Array<StatBucket>;
  totalCount: number;
}
