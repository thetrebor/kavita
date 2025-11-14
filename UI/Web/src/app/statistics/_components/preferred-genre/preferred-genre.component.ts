import {ChangeDetectionStrategy, Component, computed, inject, input} from '@angular/core';
import {StatisticsService} from "../../../_services/statistics.service";
import {EChartsDirective, ECOption} from "../../../_directives/echarts.directive";

@Component({
  selector: 'app-preferred-genre',
  imports: [
    EChartsDirective
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

  options = computed<ECOption>(() => {
    const breakdown = this.genreBreakdown.value()!;

    const labels = breakdown.data.map(d => d.value);
    const values = breakdown.data.map(d => d.count);

    return {
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' },
        formatter: (params: any) => {
          const { name, value } = params[0];
          const percent = ((value / breakdown.total) * 100).toFixed(1);
          return `${name}: ${value} (${percent}%)`;
        }
      },
      xAxis: {
        type: 'value',
        max: breakdown.total,
      },
      yAxis: {
        type: 'category',
        data: labels,
        axisLabel: { rotate: 0 },
      },
      series: [
        {
          type: 'bar',
          data: values,
          label: {
            show: true,
            position: 'right',
            formatter: (params: any) => {
              const percent = ((params.value / breakdown.total) * 100).toFixed(1);
              return `${params.value} (${percent}%)`;
            }
          }
        }
      ],
      grid: { left: '20%', right: '5%', top: '5%', bottom: '5%' },
    };
  });

}
