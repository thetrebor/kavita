import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { ControlValueAccessor, FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { tap } from "rxjs";
import { CommonModule } from '@angular/common';
import {toSignal} from "@angular/core/rxjs-interop";

export type TimeRangeFormGroup = FormGroup<{
  startDate: FormControl<Date | null>,
  endDate: FormControl<Date | null>,
}>

export type TimeRange = {
  startDate: Date | null,
  endDate: Date | null,
}

type SelectionMode = 'forever' | 'year' | 'custom';

@Component({
  selector: 'app-smart-time-range-picker',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './smart-time-range-picker.component.html',
  styleUrl: './smart-time-range-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SmartTimeRangePickerComponent implements ControlValueAccessor {

  readonly formGroup: TimeRangeFormGroup = new FormGroup({
    startDate: new FormControl<Date | null>(null),
    endDate: new FormControl<Date | null>(null),
  });

  readonly isOpen = signal(false);
  readonly dropDownMode = signal<'all' | 'year' | 'date'>('all');
  readonly customYearInput = signal<number>(new Date().getFullYear());

  readonly selectedTime = toSignal(this.formGroup.valueChanges,
    { initialValue: {startDate: null, endDate: null} })

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

  private _onChange: (v: TimeRange) => void = () => {};
  private _onTouch: () => void = () => {};

  currentYear = new Date().getFullYear();
  lastYear = this.currentYear - 1;

  constructor() {
    this.formGroup.valueChanges.pipe(
      tap(obj => {
        this._onTouch();
        this._onChange(obj as TimeRange);
      })
    ).subscribe();
  }

  toggleDropdown(): void {
    this.isOpen.update(v => !v);
  }

  closeDropdown(): void {
    this.isOpen.set(false);
    this.dropDownMode.set('all');
  }

  selectForever(): void {
    this.formGroup.setValue({ startDate: null, endDate: null });
    this.closeDropdown();
  }

  selectThisYear(): void {
    const year = new Date().getFullYear();
    this.setYearRange(year);
    this.closeDropdown();
  }

  selectLastYear(): void {
    const year = new Date().getFullYear() - 1;
    this.setYearRange(year);
    this.closeDropdown();
  }

  selectCustomYear(): void {
    const year = this.customYearInput();
    if (year && year > 1900 && year < 2200) {
      this.setYearRange(year);
      this.closeDropdown();
    }
  }

  private setYearRange(year: number): void {
    const startDate = new Date(year, 0, 1); // Jan 1
    const endDate = new Date(year, 11, 31); // Dec 31
    this.formGroup.setValue({ startDate, endDate });
  }

  private formatDate(date: Date): string {
    const options: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    };
    return date.toLocaleDateString('en-US', options);
  }

  registerOnChange(fn: (v: TimeRange) => void): void {
    this._onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this._onTouch = fn;
  }

  writeValue(obj: TimeRange): void {
    if (obj) {
      this.formGroup.setValue(obj, { emitEvent: false });
    }
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.formGroup.disable();
    } else {
      this.formGroup.enable();
    }
  }
}
