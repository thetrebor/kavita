import {ChangeDetectionStrategy, Component, computed, input} from '@angular/core';
import {EChartsDirective, ECOption} from "../../../_directives/echarts.directive";
import {LabelFormatterCallback} from "echarts/types/dist/shared";

@Component({
  selector: 'app-bar-chart',
  imports: [EChartsDirective],
  templateUrl: './bar-chart.component.html',
  styleUrl: './bar-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BarChartComponent {
  data = input.required<any[]>();
  axisLabels = input.required<string[]>();

  axisLabelsRight = input<string[]>();
  horizontal = input(false);
  height = input('300px');
  width = input('100%');

  showLabels = input(false);
  labelPosition = input<'top' | 'bottom' | 'left' | 'right' | 'inside' | 'insideLeft' | 'insideRight' | 'insideTop' | 'insideBottom'>('right');
  seriesLabelFormatter = input<LabelFormatterCallback | undefined>(undefined);

  showToolTips = input(false);

  protected options = computed(() => {
    const isHorizontal = this.horizontal();

    const option: ECOption = {
      grid: {
        left: '10%',
        right: this.axisLabelsRight() ? '10%' : '5%',
        top: '5%',
        bottom: '5%'
      },
      xAxis: {
        type: isHorizontal ? 'value' : 'category',
        data: isHorizontal ? undefined : this.axisLabels(),
        splitLine: { show: false },
        axisLabel: { show: false },
        axisLine: { show: false }
      },
      yAxis: this.buildYAxis(),
      series: [{
        type: 'bar',
        data: this.data(),
        label: {
          show: this.showLabels(),
          position: this.labelPosition(),
          formatter: this.seriesLabelFormatter()
        },
        itemStyle: {
          borderRadius: 5
        },
        barGap: '10%',
        barMinHeight: 5
      }]
    };

    if (this.showToolTips()) {
      option.tooltip = {
        trigger: 'axis'
      };
    }

    return option;
  });

  private buildYAxis() {
    const isHorizontal = this.horizontal();

    const leftYAxis = {
      type: isHorizontal ? 'category' : 'value',
      data: isHorizontal ? this.axisLabels() : undefined,
      position: 'left' as const,
      axisLine: { show: false },
      axisLabel: { rotate: 0 }
    };

    if (!this.axisLabelsRight()) {
      return leftYAxis;
    }

    const rightYAxis = {
      type: isHorizontal ? 'category' : 'value',
      data: isHorizontal ? this.axisLabelsRight() : undefined,
      position: 'right' as const,
      axisLine: { show: false },
      axisTick: { show: false }
    };

    return [leftYAxis, rightYAxis];
  }
}
