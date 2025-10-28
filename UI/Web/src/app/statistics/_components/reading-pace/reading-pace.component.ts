import {ChangeDetectionStrategy, Component, computed, input} from '@angular/core';

export interface ReadingStats {
  hoursRead: number;
  pagesRead: number;
  wordsRead: number;
  booksRead: number;
  comicsRead: number;
  daysInRange: number;
}

@Component({
  selector: 'app-reading-pace',
  imports: [],
  templateUrl: './reading-pace.component.html',
  styleUrl: './reading-pace.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReadingPaceComponent {
  stats = input.required<ReadingStats>();
  userName = input<string>('User');
  //dateRange = input<string>();

  // Calculate pace in days - books per day inverted
  paceInDays = computed(() => {
    const booksRead = this.stats().booksRead;
    const days = this.stats().daysInRange;

    if (booksRead === 0) return '∞';

    return (days / booksRead).toFixed(2);
  });

  // Hours calculations
  hoursPerYear = computed(() => this.projectAnnually(this.stats().hoursRead));
  hoursPerMonth = computed(() => this.projectMonthly(this.stats().hoursRead));
  hoursPerDay = computed(() => this.projectDaily(this.stats().hoursRead));

  // Pages calculations
  pagesPerYear = computed(() => this.projectAnnually(this.stats().pagesRead));
  pagesPerMonth = computed(() => this.projectMonthly(this.stats().pagesRead));
  pagesPerDay = computed(() => this.projectDaily(this.stats().pagesRead));

  // Words calculations
  wordsPerYear = computed(() => this.projectAnnually(this.stats().wordsRead));
  wordsPerMonth = computed(() => this.projectMonthly(this.stats().wordsRead));
  wordsPerDay = computed(() => this.projectDaily(this.stats().wordsRead));

  // Comics calculations
  comicsPerYear = computed(() => this.projectAnnually(this.stats().comicsRead));
  comicsPerMonth = computed(() => this.projectMonthly(this.stats().comicsRead));
  comicsPerDay = computed(() => this.projectDaily(this.stats().comicsRead));

  // Books calculations
  booksPerYear = computed(() => this.projectAnnually(this.stats().booksRead));
  booksPerMonth = computed(() => this.projectMonthly(this.stats().booksRead));
  booksPerDay = computed(() => this.projectDaily(this.stats().booksRead));

  private projectAnnually(value: number): string {
    const days = this.stats().daysInRange;
    if (days === 0) return '0';
    const projected = (value / days) * 365;
    return this.formatNumber(projected);
  }

  private projectMonthly(value: number): string {
    const days = this.stats().daysInRange;
    if (days === 0) return '0';
    const projected = (value / days) * 30;
    return this.formatNumber(projected);
  }

  private projectDaily(value: number): string {
    const days = this.stats().daysInRange;
    if (days === 0) return '0';
    const projected = value / days;
    return this.formatNumber(projected);
  }

  private formatNumber(value: number): string {
    if (value >= 1000) {
      return Math.round(value).toLocaleString();
    } else if (value >= 1) {
      return value.toFixed(1);
    } else {
      return value.toFixed(3);
    }
  }
}
