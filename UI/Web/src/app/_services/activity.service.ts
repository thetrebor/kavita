import {inject, Injectable} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {ReadingSession} from "../_models/progress/reading-session";
import {environment} from "../../environments/environment";
import {ClientDevice} from "../_models/client-device";

@Injectable({
  providedIn: 'root'
})
export class ActivityService {
  private readonly httpClient = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getActiveSessions() {
    return this.httpClient.get<Array<ReadingSession>>(this.baseUrl + 'activity/current');
  }

  getMyClientDevices() {
    return this.httpClient.get<Array<ClientDevice>>(this.baseUrl + 'activity/devices');
  }
}
