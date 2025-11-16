import {ChangeDetectionStrategy, Component, computed, input, Resource} from '@angular/core';
import {StatBucket} from "../../_models/stats/stat-bucket";
import {SpreadStats} from "../../_models/stats/spread-stats";
import {TranslocoDirective} from "@jsverse/transloco";
import {BarChartComponent} from "../bar-chart/bar-chart.component";

@Component({
  selector: 'app-bucket-spread-chart',
  imports: [
    TranslocoDirective,
    BarChartComponent
  ],
  templateUrl: './bucket-spread-chart.component.html',
  styleUrl: './bucket-spread-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BucketSpreadChartComponent {

  userId = input.required<number>();
  userName = input.required<string>();
  translationKey = input.required<string>();
  bucketSpreadResource = input.required<Resource<SpreadStats | undefined>>();
  endRangeFallback = input<string>('');

  protected highestBucket = computed(() => {
    if (!this.bucketSpreadResource().hasValue()) return null;

    return this.rangeFormatter(this.bucketSpreadResource().value()!.buckets
      .reduce((prev, cur) => prev.count > cur.count ? prev : cur, {
        count: -1,
        rangeStart: 0,
        rangeEnd: 0,
        percentage: 0
      }));
  });

  protected data = computed(() => this.bucketSpreadResource().value()?.buckets.map(d => d.count) ?? []);
  protected labels = computed(() => this.bucketSpreadResource().value()?.buckets
    .map(d => this.rangeFormatter(d)) ?? []);

  rangeFormatter = (params: StatBucket) => {
    return `${params.rangeStart}-${params.rangeEnd ?? this.endRangeFallback()}`;
  }

}
