import {ChangeDetectionStrategy, Component, computed, inject, input} from '@angular/core';
import {StatisticsService} from "../../../_services/statistics.service";
import {StatBucket} from "../../_models/stats/stat-bucket";
import {BarChartComponent} from "../bar-chart/bar-chart.component";
import {TranslocoDirective} from "@jsverse/transloco";

@Component({
  selector: 'app-word-spread',
  imports: [
    BarChartComponent,
    TranslocoDirective
  ],
  templateUrl: './word-spread.component.html',
  styleUrl: './word-spread.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WordSpreadComponent {

  private readonly statService = inject(StatisticsService);

  userId = input.required<number>();
  userName = input.required<string>();

  protected readonly wordSpreadResource = this.statService.getWordSpread(() => this.userId());
  protected highestBucket = computed(() => {
    if (!this.wordSpreadResource.hasValue()) return null;

    return this.rangeFormatter(this.wordSpreadResource.value()!.buckets
      .reduce((prev, cur) => prev.count > cur.count ? prev : cur, {
        count: -1,
        rangeStart: 0,
        rangeEnd: 0,
        percentage: 0
      }));
  });

  protected data = computed(() => this.wordSpreadResource.value()?.buckets.map(d => d.count) ?? []);
  protected labels = computed(() => this.wordSpreadResource.value()?.buckets
    .map(d => this.rangeFormatter(d)) ?? []);

  rangeFormatter = (params: StatBucket) => {
    return `${params.rangeStart}-${params.rangeEnd}`;
  }
}
