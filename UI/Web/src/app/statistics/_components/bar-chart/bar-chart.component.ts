import {ChangeDetectionStrategy, Component, computed, input, TemplateRef} from '@angular/core';
import {EChartsDirective, ECOption} from "../../../_directives/echarts.directive";
import {LabelFormatterCallback} from "echarts/types/dist/shared";
import {NgTemplateOutlet} from "@angular/common";

@Component({
  selector: 'app-bar-chart',
  imports: [EChartsDirective, NgTemplateOutlet],
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
  multiColor = input(false);

  showToolTips = input(false);
  readonly tooltipTemplate = input<TemplateRef<unknown> | null>(null);

  processedData = computed(() => {
    const data = this.data();
    const multiColor = this.multiColor();

    if (!multiColor) return data;

    return data.map((d, index) => {
      return {
        value: d,
        itemStyle: {
          color: this.getColorForIndex(index),
          borderRadius: 5
        }
      };
    })
  });

  // TODO: Update colours, move into theme service?
  private getColorForIndex(index: number): string {
    const palette = ['#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de', '#3ba272', '#fc8452'];
    return palette[index % palette.length];
  }

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
        data: this.processedData(),
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
