import {ChangeDetectionStrategy, Component, computed, input} from '@angular/core';
import {EChartsDirective} from "../../../_directives/echarts.directive";
import {LineSeriesOption} from "echarts/charts";

type ArrayAble<T> = T | T[];

@Component({
  selector: 'app-line-chart',
  imports: [
    EChartsDirective
  ],
  templateUrl: './line-chart.component.html',
  styleUrl: './line-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LineChartComponent {

  /**
   * Data used for the series
   */
  data = input.required<any[][] | any[]>();
  /**
   * Labels used for the valueAxis
   */
  axisLabels = input.required<string[]>();
  legendLabels = input<string[]>([]);

  /**
   * Height of the chart
   *
   * @default 300px
   */
  height = input('300px');
  /**
   * Width of the chart
   *
   * @default 100%
   */
  width = input('100%');

  private isMultiLineChart = computed(() => {
    const data = this.data();
    if (data.length === 0) return false;

    return Array.isArray(data[0]);
  });

  private seriesOption = computed<ArrayAble<LineSeriesOption>>(() => {
    const data = this.data();
    const isMultiLineChart = this.isMultiLineChart();

    if (!isMultiLineChart) {
      return {
        name: this.legendLabels()[0],
        type: 'line',
        stack: 'Total',
        data: data as any[],
      }
    }

    return data.map((dataSet, index) => ({
      name: this.legendLabels()[index],
      type: 'line',
      stack: 'Total',
      data: dataSet as any[],
    }))
  })

  protected options = computed(() => ({
    legend: {
      data: this.legendLabels(),
    },
    tooltip: {
      show: true
    },
    grid: {
      left: '10%',
      right: '5%',
      top: '5%',
      bottom: '5%'
    },
    toolbox: {
      feature: {
        saveAsImage: {}
      }
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: this.axisLabels(),
    },
    yAxis: {
      type: 'value'
    },
    series: this.seriesOption(),
  }));

}
