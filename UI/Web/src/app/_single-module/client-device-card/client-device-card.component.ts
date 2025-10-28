import {ChangeDetectionStrategy, Component, computed, input} from '@angular/core';
import {ClientDevice} from "../../_models/client-device";
import {TranslocoDirective} from "@jsverse/transloco";
import {TimeAgoPipe} from "../../_pipes/time-ago.pipe";
import {ClientDeviceType} from "../../_services/client-info.service";
import {ClientDeviceAuthTypePipe} from "../../_pipes/client-device-authtype.pipe";
import {DefaultValuePipe} from "../../_pipes/default-value.pipe";
import {ClientDevicePlatformPipe} from "../../_pipes/client-device-platform.pipe";
import {ClientDeviceTypePipe} from "../../_pipes/client-device-type.pipe";
import {DateTime, Duration, Interval} from "luxon";
import {UtcToLocaleDatePipe} from "../../_pipes/utc-to-locale-date.pipe";
import {DefaultDatePipe} from "../../_pipes/default-date.pipe";
import {UtcToLocalTimePipe} from "../../_pipes/utc-to-local-time.pipe";

@Component({
  selector: 'app-client-device-card',
  imports: [
    TranslocoDirective,
    TimeAgoPipe,
    ClientDeviceAuthTypePipe,
    DefaultValuePipe,
    ClientDevicePlatformPipe,
    ClientDeviceTypePipe,
    UtcToLocaleDatePipe,
    DefaultDatePipe,
    UtcToLocalTimePipe
  ],
  templateUrl: './client-device-card.component.html',
  styleUrl: './client-device-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientDeviceCardComponent {
  clientDevice = input.required<ClientDevice>();
  showTechnicalDetails = input<boolean>(false);

  browserInfo = computed(() => {
    const info = this.clientDevice().currentClientInfo;
    if (!info.browser && !info.browserVersion) return '';

    return `${info.browser} ${info.browserVersion}`;
  });

  screenInfo = computed(() => {
    const info = this.clientDevice().currentClientInfo;
    if (!info.screenWidth && !info.screenHeight) return '';
    return `${info.screenWidth}×${info.screenHeight} (${info.orientation})`;
  });

  deviceIcon = computed(() => {
    if (this.clientDevice().currentClientInfo?.deviceType != null) {
      const deviceType = this.clientDevice().currentClientInfo.deviceType.toLowerCase();
      if (deviceType.includes('mobile')) return 'fa-solid fa-mobile-screen';
      if (deviceType.includes('tablet')) return 'fa-solid fa-tablet-screen-button';
      if (deviceType.includes('laptop')) return 'fa-solid fa-laptop';
    }

    return 'fa-solid fa-desktop';
  });

  clientTypeBadgeClass = computed(() => {
    const clientType = this.clientDevice().currentClientInfo.clientType;
    if (clientType == ClientDeviceType.WebApp || clientType === ClientDeviceType.WebBrowser) return 'badge bg-primary';
    if ([ClientDeviceType.OpdsClient, ClientDeviceType.Librera].includes(clientType)) return 'badge bg-info';
    return 'badge bg-secondary';
  });

  isRecentlyActive = computed(() => {
    const lastSeen = DateTime.fromISO(this.clientDevice().lastSeenUtc, { zone: "utc" }); //new Date(this.clientDevice().lastSeenUtc);
    const now = DateTime.now().toUTC();
    const twentyFourHours = Duration.fromObject({ hours: 24 });

    const interval = Interval.fromDateTimes(lastSeen, now);

    return interval.toDuration().valueOf() < twentyFourHours.valueOf();
  });




}
