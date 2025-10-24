import {Injectable, signal} from '@angular/core';
import {VersionService} from "./version.service";

export interface ClientInfo {
  browser: string;
  browserVersion: string;
  platform: string;
  deviceType: string;
  screenWidth: number;
  screenHeight: number;
  orientation: string;
  appVersion: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class ClientInfoService {

  private readonly clientInfo = signal<ClientInfo>(this.detectClientInfo());

  constructor() {
    // Update orientation and screen size on resize/rotation
    window.addEventListener('resize', () => {
      this.clientInfo.update(info => ({
        ...info,
        screenWidth: window.innerWidth,
        screenHeight: window.innerHeight,
        orientation: this.getOrientation()
      }));
    });
  }

  /**
   * Returns a compact header string with client info
   * Format: "web-app/1.2.3 (Chrome/120.0; Windows; Desktop; 1920x1080; landscape)"
   */
  getClientInfoHeader(): string {
    const info = this.clientInfo();
    return `web-app/${info.appVersion} (${info.browser}/${info.browserVersion}; ${info.platform}; ${info.deviceType}; ${info.screenWidth}x${info.screenHeight}; ${info.orientation})`;
  }

  /**
   * Returns full client info object (for display in UI if needed)
   */
  getClientInfo(): ClientInfo {
    return this.clientInfo();
  }

  private detectClientInfo(): ClientInfo {
    const ua = navigator.userAgent;

    return {
      browser: this.detectBrowser(ua),
      browserVersion: this.detectBrowserVersion(ua),
      platform: this.detectPlatform(ua),
      deviceType: this.detectDeviceType(ua),
      screenWidth: window.innerWidth,
      screenHeight: window.innerHeight,
      orientation: this.getOrientation(),
      appVersion: this.getAppVersion()
    };
  }

  private detectBrowser(ua: string) {
    if (ua.includes('Edg/')) return 'Edge';
    if (ua.includes('Chrome/')) return 'Chrome';
    if (ua.includes('Safari/') && !ua.includes('Chrome')) return 'Safari';
    if (ua.includes('Firefox/')) return 'Firefox';
    if (ua.includes('Opera/') || ua.includes('OPR/')) return 'Opera';
    return 'Unknown';
  }

  private detectBrowserVersion(ua: string) {
    let match: RegExpMatchArray | null = null;

    if (ua.includes('Edg/')) {
      match = ua.match(/Edg\/(\d+\.\d+)/);
    } else if (ua.includes('Chrome/')) {
      match = ua.match(/Chrome\/(\d+\.\d+)/);
    } else if (ua.includes('Safari/') && !ua.includes('Chrome')) {
      match = ua.match(/Version\/(\d+\.\d+)/);
    } else if (ua.includes('Firefox/')) {
      match = ua.match(/Firefox\/(\d+\.\d+)/);
    }

    return match ? match[1] : 'Unknown';
  }

  private detectPlatform(ua: string) {
    if (ua.includes('Win')) return 'Windows';
    if (ua.includes('Mac')) return 'macOS';
    if (ua.includes('Linux') && !ua.includes('Android')) return 'Linux';
    if (ua.includes('iPhone') || ua.includes('iPad')) return 'iOS';
    if (ua.includes('Android')) return 'Android';
    return 'Unknown';
  }

  private detectDeviceType(ua: string): string {
    // Check for tablet first (more specific)
    if (ua.includes('iPad') || (ua.includes('Android') && !ua.includes('Mobile'))) {
      return 'Tablet';
    }

    // Then check for mobile
    if (ua.includes('Mobile') || ua.includes('iPhone') || ua.includes('Android')) {
      return 'Mobile';
    }

    // Default to desktop
    return 'Desktop';
  }

  private getOrientation() {
    return window.innerWidth > window.innerHeight ? 'landscape' : 'portrait';
  }

  private getAppVersion() {
    return localStorage.getItem(VersionService.SERVER_VERSION_KEY);
  }

}
