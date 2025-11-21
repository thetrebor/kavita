import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  OnInit,
  signal
} from '@angular/core';
import {Location, TitleCasePipe} from '@angular/common';
import {ReadingPaceComponent} from "../../../statistics/_components/reading-pace/reading-pace.component";
import {ActivityGraphComponent} from "../../../statistics/_components/activity-graph/activity-graph.component";
import {MemberInfo} from "../../../_models/user/member-info";
import {TranslocoDirective} from "@jsverse/transloco";
import {ImageService} from "../../../_services/image.service";
import {TimeAgoPipe} from "../../../_pipes/time-ago.pipe";
import {
  NgbNav,
  NgbNavChangeEvent,
  NgbNavContent,
  NgbNavItem,
  NgbNavLink,
  NgbNavOutlet
} from "@ng-bootstrap/ng-bootstrap";
import {ReviewService} from "../../../_services/review.service";
import {filter, fromEvent, of, tap} from "rxjs";
import {takeUntilDestroyed, toSignal} from "@angular/core/rxjs-interop";
import {ActivatedRoute} from "@angular/router";
import {ReviewListItemComponent} from "../review-list-item/review-list-item.component";
import {PreferredFormatComponent} from "../../../statistics/_components/preferred-format/preferred-format.component";
import {FictionGraphComponent} from "../../../statistics/_components/fiction-graph/fiction-graph.component";
import {StatisticsService} from "../../../_services/statistics.service";
import {StringBreakdownComponent} from "../../../statistics/_components/string-breakdown/string-breakdown.component";
import {UtcToLocaleDatePipe} from "../../../_pipes/utc-to-locale-date.pipe";
import {
  BucketSpreadChartComponent
} from "../../../statistics/_components/bucket-spread-chart/bucket-spread-chart.component";
import {LibraryService} from "../../../_services/library.service";
import {FormControl, FormGroup, ReactiveFormsModule} from "@angular/forms";
import {TypeaheadSettings} from "../../../typeahead/_models/typeahead-settings";
import {Library} from "../../../_models/library/library";
import {UtilityService} from "../../../shared/_services/utility.service";
import {SettingItemComponent} from "../../../settings/_components/setting-item/setting-item.component";
import {TypeaheadComponent} from "../../../typeahead/_components/typeahead.component";
import {
  SmartTimeRangePickerComponent,
  TimeRange
} from "../../../shared/smart-time-range-picker/smart-time-range-picker.component";
import {map} from "rxjs/operators";
import {ProfileImageComponent} from "../profile-image/profile-image.component";
import {FavouriteAuthorsComponent} from "../../../statistics/_components/favourite-authors/favourite-authors.component";
import {StatsFilter} from "../../../statistics/_models/stats-filter";

enum TabID {
  Overview = 'overview-tab',
  Stats = 'stats-tab',
  Reviews = 'reviews-tab',
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    ReadingPaceComponent,
    ActivityGraphComponent,
    TranslocoDirective,
    TimeAgoPipe,
    NgbNav,
    NgbNavContent,
    NgbNavLink,
    NgbNavItem,
    NgbNavOutlet,
    ReviewListItemComponent,
    PreferredFormatComponent,
    FictionGraphComponent,
    TitleCasePipe,
    StringBreakdownComponent,
    UtcToLocaleDatePipe,
    BucketSpreadChartComponent,
    ReactiveFormsModule,
    TypeaheadComponent,
    SmartTimeRangePickerComponent,
    ProfileImageComponent,
    FavouriteAuthorsComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent implements OnInit {

  private readonly location = inject(Location);
  private readonly reviewService = inject(ReviewService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly imageService = inject(ImageService);
  private readonly statsService = inject(StatisticsService);
  private readonly libraryService = inject(LibraryService);
  private readonly utilityService = inject(UtilityService);

  // Set by angular from the resolver
  memberInfo = input.required<MemberInfo>();
  userId = computed(() => this.memberInfo().id);

  protected readonly reviewsResource = this.reviewService.getReviewsByUserResource(() => this.memberInfo().id);
  protected readonly genreBreakdown = this.statsService.getGenreBreakDownResource(() => this.filter()!, () => this.userId());
  protected readonly tagsBreakdown = this.statsService.getTagBreakDownResource(() => this.filter()!, () => this.userId());
  protected readonly wordSpreadResource = this.statsService.getWordSpread(() => this.userId());
  protected readonly pageSpreadResource = this.statsService.getPageSpread(() => this.userId());

  activeTabId = TabID.Overview;

  allLibraries = signal<Library[]>([]);
  showLibraryTypeahead = signal(false);
  libraryTypeaheadSettings?: TypeaheadSettings<Library>;
  filterForm = new FormGroup({
    timeFilter: new FormGroup({
      startDate: new FormControl<Date | null>(null),
      endDate: new FormControl<Date | null>(null),
    }),
    libraries: new FormControl<number[]>([]),
  });
  filter = toSignal(this.filterForm.valueChanges.pipe(
    map(value => value as StatsFilter),
  ));

  constructor() {
    this.route.fragment.pipe(tap(frag => {
      const fragId = frag as TabID;
      if (frag !== null && this.activeTabId !== fragId) {
        this.updateUrl(fragId);
        this.activeTabId = fragId;
      }
    }), takeUntilDestroyed(this.destroyRef)).subscribe();
  }

  ngOnInit() {
    this.libraryService.getLibraries().pipe(
      tap(libs => this.allLibraries.set(libs)),
      tap(libs => this.filterForm.get('libraries')?.setValue(libs.map(l => l.id))),
      tap(libs => this.libraryTypeaheadSettings = this.setupLibrarySettings(libs, libs))
    ).subscribe();
  }

  setupLibrarySettings(
    allLibraries: Array<Library>,
    currentSelectedLibraries: Array<Library> | undefined,
  ): TypeaheadSettings<Library> {
    const settings = new TypeaheadSettings<Library>();

    settings.minCharacters = 0;
    settings.multiple = true;
    settings.id = 'libraries';
    settings.unique = true;
    settings.showLocked = false;
    settings.addIfNonExisting = false;
    settings.compareFn = (options: Library[], filter: string) => {
      return options.filter(l => this.utilityService.filter(l.name, filter));
    }
    settings.compareFnForAdd = (options: Library[], filter: string) => {
      return options.filter(l => this.utilityService.filterMatches(l.name, filter));
    }
    settings.fetchFn = (filter: string) => of(allLibraries)
      .pipe(map(items => settings.compareFn(items, filter)));

    settings.selectionCompareFn = (a: Library, b: Library) => {
      return a.id === b.id;
    }

    settings.trackByIdentityFn = (_, value) => value.id + '';

    const savedData = currentSelectedLibraries?.filter(l => allLibraries.indexOf(l) >= 0);
    if (savedData) {
      settings.savedData = savedData;
    }

    return settings;
  }

  updateSelectedLibraries(libs: Library[]) {
    this.filterForm.get('libraries')!.setValue(libs.map(l => l.id));
    this.libraryTypeaheadSettings = this.setupLibrarySettings(this.allLibraries(), libs);
  }

  updateTimeRange(tr: TimeRange) {
    this.filterForm.get('timeFilter')!.setValue(tr);
  }

  libraryName(libraryId: number): string {
    return this.allLibraries().find(l => l.id === libraryId)?.name ?? 'unknown';
  }

  onNavChange(event: NgbNavChangeEvent) {
    this.updateUrl(event.nextId);
    this.activeTabId = event.nextId;
  }

  updateUrl(activeTab: TabID) {
    const tokens = this.location.path().split('#');
    const newUrl = `${tokens[0]}#${activeTab}`;
    this.location.replaceState(newUrl) // TODO: Look into making this a directive for tabs
  }


  protected readonly TabID = TabID;

}
