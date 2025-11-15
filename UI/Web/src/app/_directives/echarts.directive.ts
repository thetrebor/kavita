import {
  DestroyRef,
  Directive,
  effect,
  ElementRef, EmbeddedViewRef,
  inject,
  input,
  NgZone,
  OnDestroy,
  OnInit, TemplateRef,
  untracked, ViewContainerRef
} from '@angular/core';
import {EChartsInitOpts, init, EChartsType, ComposeOption, registerTheme} from "echarts/core";
import { BarSeriesOption, LineSeriesOption, PieSeriesOption } from 'echarts/charts';
import {
  TitleComponentOption,
  TooltipComponentOption,
  DatasetComponentOption,
  LegendComponentOption,
} from 'echarts/components';
import {ThemeService} from "../_services/theme.service";
import {asyncScheduler, Subject, tap, fromEvent, filter} from "rxjs";
import {throttleTime} from "rxjs/operators";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {SiteTheme} from "../_models/preferences/site-theme";

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
  private readonly vcr = inject(ViewContainerRef);

  readonly options = input<ECOption | null>(null);
  readonly initOptions = input<EChartsInitOpts | undefined>(undefined);
  readonly useCustomTooltips = input(false);
  readonly tooltipTemplate = input<TemplateRef<unknown> | null>(null);

  private tooltipViewRef: EmbeddedViewRef<any> | null = null;
  private tooltipContext = { $implicit: null as any };
  private lastMousePosition = { x: 0, y: 0 };
  private tooltipVisible = false;

  echart?: EChartsType;

  private resizer$ = new Subject<void>();
  private resizeObserver?: ResizeObserver;
  private resizeObserverHasFired = false;
  private animationFrameID?: number;

  constructor() {
    // Keep up to date with themes
    effect(() => {
      const kavitaTheme = this.themeService.currentTheme();
      const options = untracked(this.options);
      if (!kavitaTheme || !options) return;

      this.ngZone.runOutsideAngular(() => {
        this.initEChart(kavitaTheme, options);
      });
    });

    // Keep up to date with options
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
    const kavitaTheme = this.themeService.currentTheme();
    if (!options || !kavitaTheme) return;

    this.ngZone.runOutsideAngular(() => {
      this.initEChart(kavitaTheme, options);
    });

    this.resizer$.pipe(
      takeUntilDestroyed(this.destroyRef),
      throttleTime(100, asyncScheduler, { leading: false, trailing: true}),
      tap(() => {
        if (this.echart) {
          this.echart.resize();
        }
      })).subscribe();

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

    if (this.useCustomTooltips()) {
      fromEvent<MouseEvent>(this.el.nativeElement, 'mousemove')
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          tap((event) => {
            const rect = this.el.nativeElement.getBoundingClientRect();
            this.lastMousePosition = {
              x: event.clientX - rect.left,
              y: event.clientY - rect.top
            };
          }),
          filter(() => this.tooltipVisible),
          tap(() => this.moveTooltip(this.lastMousePosition.x, this.lastMousePosition.y)),
        ).subscribe();
    }
  }

  ngOnDestroy() {
    if (this.animationFrameID) {
      window.cancelAnimationFrame(this.animationFrameID);
    }

    if (this.resizeObserver) {
      this.resizeObserver.unobserve(this.el.nativeElement);
    }

    this.clearTooltip();
  }

  private initEChart(kavitaTheme: SiteTheme, option: ECOption) {
    registerTheme(kavitaTheme.name, this.createTheme());

    this.echart?.dispose();
    this.echart = init(this.el.nativeElement, kavitaTheme.name, untracked(this.initOptions));

    if (this.useCustomTooltips()) {
      option.tooltip = {
        ...option.tooltip,
        show: false,
        showContent: false,
        trigger: 'item',
      }
    }

    this.echart.setOption(option);

    if (this.useCustomTooltips()) {
      this.echart.on('showTip', (params: any) => {
        this.tooltipVisible = true;
        this.renderTooltip({
          dataIndex: params.dataIndex,
          params: params,
        });

        const x = params.x ?? this.lastMousePosition.x;
        const y = params.y ?? this.lastMousePosition.y;
        this.moveTooltip(x, y);
      });

      this.echart.on('updateAxisPointer', (params: any) => {
        if (this.tooltipVisible) {
          const x = params.x ?? this.lastMousePosition.x;
          const y = params.y ?? this.lastMousePosition.y;
          this.moveTooltip(x, y);
        }
      });

      this.echart.on('hideTip', () => {
        this.tooltipVisible = false;
        this.clearTooltip();
      });
    }
  }

  private createTheme() {
    const textColour = this.getCssVar('--body-text-color');

    const colorPalette = [
      this.getCssVar('--primary-color'),
      this.getCssVar('--primary-color-dark-shade'),
      this.getCssVar('--primary-color-darker-shade'),
      this.getCssVar('--primary-color-darkest-shade'),
    ];
    return {
      color: colorPalette,
      categoryAxis: {
        axisLabel: {
          color: textColour,
        }
      },
      valueAxis: {
        axisLabel: {
          color: textColour,
        }
      },
      label: {
        textBorderWidth: 0,
        textBorderColor: 'transparent',
        color: textColour,
      },
    };
  }

  private getCssVar(key: string) {
    return getComputedStyle(document.documentElement).getPropertyValue(key).trim();
  }

  private renderTooltip(ctx: unknown) {
    this.tooltipContext.$implicit = ctx;

    if (!this.tooltipViewRef && this.tooltipTemplate()) {
      this.tooltipViewRef = this.vcr.createEmbeddedView(
        this.tooltipTemplate()!,
        this.tooltipContext
      );
    }

    this.tooltipViewRef?.detectChanges();
  }

  private moveTooltip(x: number, y: number) {
    if (!this.tooltipViewRef) return;
    const native = this.tooltipViewRef.rootNodes.find(
      node => node.nodeType === Node.ELEMENT_NODE
    ) as HTMLElement;
    if (!native || !native.style) return;

    const offsetX = 0;
    const offsetY = 0;

    native.style.position = 'absolute';
    native.style.pointerEvents = 'none';
    native.style.left = `${x + offsetX}px`;
    native.style.top = `${y + offsetY}px`;
    native.style.zIndex = '9999';

    const rect = native.getBoundingClientRect();
    const containerRect = this.el.nativeElement.getBoundingClientRect();

    if (rect.right > containerRect.right) {
      native.style.left = `${x - rect.width - offsetX}px`;
    }

    if (rect.bottom > containerRect.bottom) {
      native.style.top = `${y - rect.height - offsetY}px`;
    }
  }

  private clearTooltip() {
    if (this.tooltipViewRef) {
      this.tooltipViewRef.destroy();
      this.tooltipViewRef = null;
    }
  }
}
