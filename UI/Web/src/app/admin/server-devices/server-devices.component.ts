import {ChangeDetectionStrategy, Component, inject, model} from '@angular/core';
import {DeviceService} from "../../_services/device.service";
import {ClientDevice} from "../../_models/client-device";
import {ClientDeviceCardComponent} from "../../_single-module/client-device-card/client-device-card.component";
import {LoadingComponent} from "../../shared/loading/loading.component";
import {TranslocoDirective} from "@jsverse/transloco";
import {PieChartModule} from "@swimlane/ngx-charts";
import {PieDataItem} from "../../statistics/_models/pie-data-item";
import {StatisticsService} from "../../_services/statistics.service";
import {ClientDeviceTypePipe} from "../../_pipes/client-device-type.pipe";

@Component({
  selector: 'app-server-devices',
  imports: [
    ClientDeviceCardComponent,
    LoadingComponent,
    TranslocoDirective,
    PieChartModule
  ],
  templateUrl: './server-devices.component.html',
  styleUrl: './server-devices.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerDevicesComponent {

  private readonly deviceService = inject(DeviceService);
  private readonly statsService = inject(StatisticsService);

  clientDevices = model<ClientDevice[]>([]);
  clientDeviceTypeBreakdown = model<PieDataItem[]>([]);
  mobileVsDesktop = model<PieDataItem[]>([]);

  constructor() {
    this.deviceService.getAllDevices().subscribe(devices => {
      this.clientDevices.set([...devices]);
    });

    this.statsService.getClientDeviceBreakdown().subscribe(clientDeviceBreakdown => {
      const pipe = new ClientDeviceTypePipe();
      this.clientDeviceTypeBreakdown.set(
        clientDeviceBreakdown.records.map(record => ({
          name: pipe.transform(record.value),
          value: record.count,
          extra: { clientType: record.value }
        }))
      );
    });

    this.statsService.getClientDeviceTypeCounts().subscribe(data => {
      this.mobileVsDesktop.set(
        data.map(record => ({
          name: record.value,
          value: record.count,
          extra: { clientType: record.value }
        }))
      );
    });
  }


}
