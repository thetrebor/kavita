import {ChangeDetectionStrategy, Component, inject, input} from '@angular/core';
import {StatisticsService} from "../../../_services/statistics.service";
import {TranslocoDirective} from "@jsverse/transloco";

@Component({
  selector: 'app-page-spread',
  imports: [
    TranslocoDirective
  ],
  templateUrl: './page-spread.component.html',
  styleUrl: './page-spread.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PageSpreadComponent {

  private readonly statService = inject(StatisticsService);

  userId = input.required<number>();

  pageSpreadResource = this.statService.getPageSpread(() => this.userId());


}
