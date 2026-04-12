import { Injectable, inject } from '@angular/core';
import {TextResonse} from "../_types/text-response";
import {HttpClient, HttpParams} from "@angular/common/http";
import {environment} from "../../environments/environment";
import {DashboardStream} from "../_models/dashboard/dashboard-stream";
import {RecentlyUpdatedItem} from "../_models/dashboard/recently-updated-item";
import {UtilityService} from "../shared/_services/utility.service";

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private httpClient = inject(HttpClient);
  private utilityService = inject(UtilityService);

  baseUrl = environment.apiUrl;

  getDashboardStreams(visibleOnly = true) {
    return this.httpClient.get<Array<DashboardStream>>(this.baseUrl + 'stream/dashboard?visibleOnly=' + visibleOnly);
  }

  getRecentlyUpdatedItems(pageNum?: number, itemsPerPage?: number) {
    let params = new HttpParams();
    params = this.utilityService.addPaginationIfExists(params, pageNum, itemsPerPage);
    return this.httpClient.post<RecentlyUpdatedItem[]>(this.baseUrl + 'dashboard/recently-updated-items', {}, {params});
  }

  updateDashboardStreamPosition(streamName: string, dashboardStreamId: number, fromPosition: number, toPosition: number) {
    return this.httpClient.post(this.baseUrl + 'stream/update-dashboard-position', {streamName, id: dashboardStreamId, fromPosition, toPosition}, TextResonse);
  }

  updateDashboardStream(stream: DashboardStream) {
    return this.httpClient.post(this.baseUrl + 'stream/update-dashboard-stream', stream, TextResonse);
  }

  createDashboardStream(smartFilterId: number) {
    return this.httpClient.post<DashboardStream>(this.baseUrl + 'stream/add-dashboard-stream?smartFilterId=' + smartFilterId, {});
  }

  deleteSmartFilterStream(streamId: number) {
    return this.httpClient.delete(this.baseUrl + 'stream/smart-filter-dashboard-stream?dashboardStreamId=' + streamId, {});
  }
}
