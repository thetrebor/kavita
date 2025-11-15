import {ChangeDetectionStrategy, Component, computed, inject, input} from '@angular/core';
import {StatisticsService} from "../../../_services/statistics.service";
import {EChartsDirective, ECOption} from "../../../_directives/echarts.directive";
import {BarChartComponent} from "../bar-chart/bar-chart.component";
import {StatCount} from "../../_models/stat-count";

@Component({
  selector: 'app-preferred-genre',
  imports: [
    BarChartComponent
  ],
  templateUrl: './preferred-genre.component.html',
  styleUrl: './preferred-genre.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PreferredGenreComponent {

  private readonly statsService = inject(StatisticsService);
  protected readonly genreBreakdown = this.statsService
    .getGenreBreakDownResource(() => this.userId());

  userId = input.required<number>();
  userName = input.required<string>();

  mostReadGenre = computed(() => {
    if (!this.genreBreakdown.hasValue()) return null;

    return this.genreBreakdown.value()!.data
      .reduce((prev, cur) => prev.count > cur.count ? prev : cur,
        {count: -1, value: ""});
  });

  labels = computed(() => this.genreBreakdown.value()?.data.map(d => d.value).reverse() ?? []);
  labelsRight = computed(() => this.genreBreakdown.value()?.data
    .map(d => this.labelFormatter(d)).reverse() ?? []);

  data = computed(() => this.genreBreakdown.value()?.data.map(d => d.count).reverse() ?? []);
  totalReads = computed(() => this.genreBreakdown.value()?.total ?? 0);

  labelFormatter = (params: StatCount<string>) => {
    const percent = ((params.count / this.totalReads()) * 100).toFixed(1);
    return `${params.count} (${percent}%)`;
  }

}
