import {ChangeDetectionStrategy, Component, computed, inject, model} from '@angular/core';
import {StatisticsService} from "../../../_services/statistics.service";
import {AccountService} from "../../../_services/account.service";
import {TranslocoDirective} from "@jsverse/transloco";
import {MangaFormatPipe} from "../../../_pipes/manga-format.pipe";
import {PieChartModule, ScaleType} from "@swimlane/ngx-charts";
import { EChartsCoreOption } from "echarts/core";
import {EChartsDirective, ECOption} from "../../../_directives/echarts.directive";

@Component({
  selector: 'app-preferred-format',
  imports: [
    TranslocoDirective,
    PieChartModule,
    EChartsDirective
  ],
  templateUrl: './preferred-format.component.html',
  styleUrl: './preferred-format.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PreferredFormatComponent {
  private readonly statsService = inject(StatisticsService);
  private readonly accountService = inject(AccountService);

  formatsResource = this.statsService
    .getPreferredFormatResource(() => this.accountService.currentUserSignal()!.id);

  options = computed<ECOption>(() => {
    const data = this.formatsResource.value();
    const pipe = new MangaFormatPipe();

    const source = [
      ['Format', 'Count'],
      ...(data ?? []).map(record => [
        pipe.transform(record.value),
        record.count,
      ]),
    ];

    return {
      legend: {},
      dataset: { source },
      series: [{ type: 'pie' }],
    };
  });



}
