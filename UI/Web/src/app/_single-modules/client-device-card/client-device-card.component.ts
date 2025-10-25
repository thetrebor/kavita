import {ChangeDetectionStrategy, Component, computed, input} from '@angular/core';
import {ClientDevice} from "../../_models/client-device";
import {TranslocoDirective} from "@jsverse/transloco";
import {TranslocoDatePipe} from "@jsverse/transloco-locale";
import {AuthenticationType} from "../../_models/progress/reading-session";
import {TimeAgoPipe} from "../../_pipes/time-ago.pipe";

@Component({
  selector: 'app-client-device-card',
  imports: [
    TranslocoDirective,
    TranslocoDatePipe,
    TimeAgoPipe
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
    return `${info.browser} ${info.browserVersion}`;
  });

  screenInfo = computed(() => {
    const info = this.clientDevice().currentClientInfo;
    return `${info.screenWidth}×${info.screenHeight} (${info.orientation})`;
  });

  deviceIcon = computed(() => {
    const deviceType = this.clientDevice().currentClientInfo.deviceType.toLowerCase();
    if (deviceType.includes('mobile')) return 'fa-solid fa-mobile-screen';
    if (deviceType.includes('tablet')) return 'fa-solid fa-tablet-screen-button';
    if (deviceType.includes('laptop')) return 'fa-solid fa-laptop';
    return 'fa-solid fa-desktop';
  });

  clientTypeBadgeClass = computed(() => {
    const clientType = this.clientDevice().currentClientInfo.clientType.toLowerCase();
    if (clientType.includes('web')) return 'badge bg-primary';
    if (clientType.includes('opds') || clientType.includes('koreader')) return 'badge bg-info';
    if (clientType.includes('mihon') || clientType.includes('panels')) return 'badge bg-warning';
    return 'badge bg-secondary';
  });

  authTypeLabel = computed(() => {
    const authType = this.clientDevice().currentClientInfo.authType;
    switch (authType) {
      case AuthenticationType.ApiKey: return 'API Key';
      case AuthenticationType.OIDC: return 'OAuth';
      case AuthenticationType.JWT: return 'Web App';
      default: return 'Unknown';
    }
  });

  isRecentlyActive = computed(() => {
    const lastSeen = new Date(this.clientDevice().lastSeenUtc);
    const now = new Date();
    const hoursDiff = (now.getTime() - lastSeen.getTime()) / (1000 * 60 * 60);
    return hoursDiff < 24; // Active within last 24 hours
  });


}
