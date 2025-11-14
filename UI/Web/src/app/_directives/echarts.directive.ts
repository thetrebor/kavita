import {Directive, effect, ElementRef, inject, input, NgZone, OnInit} from '@angular/core';
import {EChartsInitOpts, init, EChartsType, ComposeOption} from "echarts/core";
import { BarSeriesOption, LineSeriesOption, PieSeriesOption } from 'echarts/charts';
import {
  TitleComponentOption,
  TooltipComponentOption,
  DatasetComponentOption,
  LegendComponentOption,
} from 'echarts/components';
import {ThemeService} from "../_services/theme.service";

export type ECOption = ComposeOption<
  | BarSeriesOption
  | LineSeriesOption
  | PieSeriesOption
  | TitleComponentOption
  | TooltipComponentOption
  | DatasetComponentOption
  | LegendComponentOption
>;

@Directive({
  selector: '[appECharts]'
})
export class EChartsDirective implements OnInit {

  private readonly el = inject(ElementRef);
  private readonly themeService = inject(ThemeService);
  private ngZone = inject(NgZone);

  readonly options = input<ECOption | null>(null);
  readonly initOptions = input<EChartsInitOpts | undefined>(undefined);
  readonly theme = input<object | string | undefined>(undefined);

  echart?: EChartsType;

  constructor() {
    effect(() => {
      this.themeService.currentTheme(); // Call on theme updates

      const newTheme = this.createTheme();
      this.echart?.setOption(newTheme);
    });
  }

  ngOnInit() {
    const options = this.options();
    if (!options) return;

    this.ngZone.runOutsideAngular(() => {
      this.echart = init(this.el.nativeElement, this.theme(), this.initOptions());
      this.echart.setOption(options);
    });
  }

  // TODO: Create a nice theme
  private createTheme() {
    return {};
  }

}
