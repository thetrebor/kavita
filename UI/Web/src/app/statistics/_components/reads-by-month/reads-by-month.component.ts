import {ChangeDetectionStrategy, Component, computed, input, Resource} from '@angular/core';
import {StatCount} from "../../_models/stat-count";
import {TranslocoDirective} from "@jsverse/transloco";
import {LineChartComponent} from "../line-chart/line-chart.component";

@Component({
  selector: 'app-reads-by-month',
  imports: [
    TranslocoDirective,
    LineChartComponent
  ],
  templateUrl: './reads-by-month.component.html',
  styleUrl: './reads-by-month.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReadsByMonthComponent {

  userName = input.required<string>();
  readsByMonthResource = input.required<Resource<StatCount<{year: number, month: number}>[] | undefined>>();

  legendLabels = computed(() => {
    if (!this.readsByMonthResource().hasValue()) return [];

    const years = this.readsByMonthResource().value()!.map(s => s.value.year);
    return Array.from(new Set(years)).sort((a, b) => a - b).map(y => y + '');
  });

  axisLabels = computed(() => ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12']);

  data = computed(() => {
    if (!this.readsByMonthResource().hasValue()) return [];

    const yearCount = this.legendLabels().length;
    const data = Array.from({ length: yearCount }, () => new Array(12).fill(0));

    this.readsByMonthResource().value()!.forEach(s => {
      const yearIndex = this.legendLabels().indexOf(s.value.year + '');
      if (yearIndex !== -1) {
        data[yearIndex][s.value.month] = s.count;
      }
    });

    return data;
  });

}
