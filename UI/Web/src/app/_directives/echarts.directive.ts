import {DestroyRef, Directive, effect, ElementRef, inject, input, NgZone, OnDestroy, OnInit} from '@angular/core';
import {EChartsInitOpts, init, EChartsType, ComposeOption} from "echarts/core";
import { BarSeriesOption, LineSeriesOption, PieSeriesOption } from 'echarts/charts';
import {
  TitleComponentOption,
  TooltipComponentOption,
  DatasetComponentOption,
  LegendComponentOption,
} from 'echarts/components';
import {ThemeService} from "../_services/theme.service";
import {asyncScheduler, Subject, Subscription, tap} from "rxjs";
import {throttleTime} from "rxjs/operators";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";

export type ECOption = ComposeOption<
  | BarSeriesOption
  | LineSeriesOption
  | PieSeriesOption
  | TitleComponentOption
  | TooltipComponentOption
  | DatasetComponentOption
  | LegendComponentOption
>;

/**
 * Some code in this directive has been (partially) copied from https://github.com/xieziyu/ngx-echarts/blob/master/projects/ngx-echarts/src/lib/ngx-echarts.directive.ts
 */
@Directive({
  selector: '[appECharts]'
})
export class EChartsDirective implements OnInit, OnDestroy {

  private readonly destroyRef = inject(DestroyRef);
  private readonly el = inject(ElementRef);
  private readonly themeService = inject(ThemeService);
  private ngZone = inject(NgZone);

  readonly options = input<ECOption | null>(null);
  readonly initOptions = input<EChartsInitOpts | undefined>(undefined);
  readonly theme = input<object | string | undefined>(undefined);

  echart?: EChartsType;

  private resizer$ = new Subject<void>();
  private resizeSub?: Subscription;
  private resizeObserver?: ResizeObserver;
  private resizeObserverHasFired = false;
  private animationFrameID?: number;

  constructor() {
    effect(() => {
      this.themeService.currentTheme(); // Call on theme updates

      const newTheme = this.createTheme();
      this.echart?.setOption(newTheme);
    });
    effect(() => {
      const options = this.options();
      if (!options || !this.echart) return;

      this.ngZone.runOutsideAngular(() => {
        this.echart!.setOption(options);
      });
    });
  }

  ngOnInit() {
    const options = this.options();
    if (!options) return;

    this.ngZone.runOutsideAngular(() => {
      this.echart = init(this.el.nativeElement, this.theme(), this.initOptions());
      this.echart.setOption(options);
    });

    this.resizeSub = this.resizer$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        throttleTime(100, asyncScheduler, { leading: false, trailing: true}),
        tap(() => {
          if (this.echart) {
            this.echart.resize();
          }
        }),
      ).subscribe();

    this.resizeObserver = this.ngZone.runOutsideAngular(
      () =>
        new window.ResizeObserver(entries => {
          for (const entry of entries) {
            if (entry.target === this.el.nativeElement) {
              if (!this.resizeObserverHasFired) {
                this.resizeObserverHasFired = true;
                return; // Ignore first fire on insertion, no resize actually happened
              }

              this.animationFrameID = window.requestAnimationFrame(() => {
                this.resizer$.next();
              });
            }
          }
        })
    );

    this.resizeObserver.observe(this.el.nativeElement);
  }

  ngOnDestroy() {
    if (this.animationFrameID) {
      window.cancelAnimationFrame(this.animationFrameID);
    }

    if (this.resizeObserver) {
      this.resizeObserver.unobserve(this.el.nativeElement);
    }
  }

  // TODO: Create a nice theme
  private createTheme() {
    return {};
  }

}
