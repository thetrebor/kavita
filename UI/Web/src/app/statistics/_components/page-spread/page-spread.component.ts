import {ChangeDetectionStrategy, Component, computed, inject, input} from '@angular/core';
import {StatisticsService} from "../../../_services/statistics.service";
import {TranslocoDirective} from "@jsverse/transloco";
import {BarChartComponent} from "../bar-chart/bar-chart.component";
import {StatBucket} from "../../_models/stats/stat-bucket";

@Component({
  selector: 'app-page-spread',
  imports: [
    TranslocoDirective,
    BarChartComponent
  ],
  templateUrl: './page-spread.component.html',
  styleUrl: './page-spread.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PageSpreadComponent {

  private readonly statService = inject(StatisticsService);

  protected readonly pageSpreadResource = this.statService.getPageSpread(() => this.userId());

  userId = input.required<number>();
  userName = input.required<string>();

  highestBucket = computed(() => {
    if (!this.pageSpreadResource.hasValue()) return null;

    return this.rangeFormatter(this.pageSpreadResource.value()!.buckets
      .reduce((prev, cur) => prev.count > cur.count ? prev : cur, {
        count: -1,
        rangeStart: 0,
        rangeEnd: 0,
        percentage: 0
      }));
  });

  data = computed(() => this.pageSpreadResource.value()?.buckets.map(d => d.count) ?? []);
  labels = computed(() => this.pageSpreadResource.value()?.buckets
    .map(d => this.rangeFormatter(d)) ?? []);
  labelsRight = computed(() => this.pageSpreadResource.value()?.buckets
    .map(d => this.rangeFormatter(d)) ?? []);

  rangeFormatter = (params: StatBucket) => {
    return `${params.rangeStart}-${params.rangeEnd ?? '1000+'}`;
  }

}
