import {ChangeDetectionStrategy, Component, computed, forwardRef, inject, OnInit, signal} from '@angular/core';
import {ControlValueAccessor, FormControl, FormGroup, NG_VALUE_ACCESSOR, ReactiveFormsModule} from "@angular/forms";
import { tap } from "rxjs";
import { CommonModule } from '@angular/common';
import {toSignal} from "@angular/core/rxjs-interop";
import {ServerService} from "../../_services/server.service";
import {TranslocoDirective} from "@jsverse/transloco";

export type TimeRangeFormGroup = FormGroup<{
  startDate: FormControl<Date | null>,
  endDate: FormControl<Date | null>,
}>

export type TimeRange = {
  startDate: Date | null,
  endDate: Date | null,
}

@Component({
  selector: 'app-smart-time-range-picker',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, TranslocoDirective],
  templateUrl: './smart-time-range-picker.component.html',
  styleUrl: './smart-time-range-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SmartTimeRangePickerComponent),
      multi: true,
    }
  ]
})
export class SmartTimeRangePickerComponent implements ControlValueAccessor, OnInit {

  private serverService = inject(ServerService);

  readonly formGroup: TimeRangeFormGroup = new FormGroup({
    startDate: new FormControl<Date | null>(null),
    endDate: new FormControl<Date | null>(null),
  });

  readonly isOpen = signal(false);
  readonly dropDownMode = signal<'all' | 'year' | 'date'>('all');

  readonly selectedTime = toSignal(this.formGroup.valueChanges,
    { initialValue: {startDate: null, endDate: null} });

  readonly displayText = computed(() => {
    const selectedTime = this.selectedTime();
    const start = selectedTime.startDate
    const end = selectedTime.endDate;

    if (!start && !end) {
      return 'during their entire life';
    }

    if (start && end) {
      const startDate = new Date(start);
      const endDate = new Date(end);

      if (startDate.getMonth() === 0 && startDate.getDate() === 1 &&
        endDate.getMonth() === 11 && endDate.getDate() === 31 &&
        startDate.getFullYear() === endDate.getFullYear()) {
        return `during ${startDate.getFullYear()}`;
      }

      return `from ${this.formatDate(startDate)} to ${this.formatDate(endDate)}`;
    }

    return 'select a time range';
  });
  readonly yearOptions = signal<number[]>([]);

  private _onChange: (v: TimeRange) => void = () => {};
  private _onTouch: () => void = () => {};

  constructor() {
    this.formGroup.valueChanges.pipe(
      tap(obj => {
        this._onTouch();
        this._onChange(obj as TimeRange);
      })
    ).subscribe();
  }

  ngOnInit() {
    this.serverService.getServerInfo().pipe(
      tap(info => {
        const installDate = info.firstInstallDate;
        if (installDate) {
         const installYear = new Date(installDate).getFullYear();
         const amountOfYears = new Date().getFullYear() - installYear + 1;

         this.yearOptions.set(Array.from(
           {length: amountOfYears},
           (_, i) => installYear + i,
         ));
        }
      })
    ).subscribe();
  }

  toggleDropdown() {
    this.isOpen.update(v => !v);
  }

  closeDropdown() {
    this.isOpen.set(false);
    this.dropDownMode.set('all');
  }

  selectForever() {
    this.formGroup.setValue({ startDate: null, endDate: null });
    this.closeDropdown();
  }

  setYearRange(year: number) {
    const startDate = new Date(year, 0, 1); // Jan 1
    const endDate = new Date(year, 11, 31); // Dec 31
    this.formGroup.setValue({ startDate, endDate });
    this.closeDropdown();
  }

  private formatDate(date: Date): string {
    const options: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    };
    return date.toLocaleDateString('en-US', options);
  }

  registerOnChange(fn: (v: TimeRange) => void) {
    this._onChange = fn;
  }

  registerOnTouched(fn: () => void) {
    this._onTouch = fn;
  }

  writeValue(obj: TimeRange) {
    if (obj) {
      this.formGroup.setValue(obj, { emitEvent: false });
    }
  }
}
