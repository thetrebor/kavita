import {Pipe, PipeTransform} from '@angular/core';
import {ClientDeviceType} from "../_services/client-info.service";
import {translate} from "@jsverse/transloco";

@Pipe({
  name: 'clientDeviceType'
})
export class ClientDeviceTypePipe implements PipeTransform {

  transform(value: ClientDeviceType) {
    switch (value) {
      case ClientDeviceType.Unknown:
        return translate('client-device-type-pipe.unknown');
      case ClientDeviceType.WebBrowser:
        return translate('client-device-type-pipe.web-browser');
      case ClientDeviceType.WebApp:
        return translate('client-device-type-pipe.web-app');
      case ClientDeviceType.KoReader:
        return translate('client-device-type-pipe.koreader');
      case ClientDeviceType.Panels:
        return translate('client-device-type-pipe.panels');
      case ClientDeviceType.Librera:
        return translate('client-device-type-pipe.librera');
      case ClientDeviceType.OpdsClient:
        return translate('client-device-type-pipe.opds-client');

    }
  }

}
