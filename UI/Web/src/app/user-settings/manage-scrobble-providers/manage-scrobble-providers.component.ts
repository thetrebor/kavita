import {ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal} from '@angular/core';
import {ScrobbleProvider, ScrobblingService} from "../../_services/scrobbling.service";
import {FormControl, FormGroup, NonNullableFormBuilder, ReactiveFormsModule} from "@angular/forms";
import {map, switchMap, tap} from "rxjs";
import {AgeRating} from "../../_models/metadata/age-rating";
import {
  ReadStatusTransitionRule,
  ReviewScrobbleTarget,
  ScrobbleProviderSettings,
  ScrobbleReadStatus,
  UserScrobbleProvider
} from "../../_models/kavitaplus/scrobble-provider-settings";
import {PublicationStatus} from "../../_models/metadata/publication-status";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {debounceTime, distinctUntilChanged} from "rxjs/operators";
import {TranslocoDirective} from "@jsverse/transloco";
import {
  NgbAccordionBody,
  NgbAccordionButton,
  NgbAccordionCollapse,
  NgbAccordionDirective,
  NgbAccordionHeader,
  NgbAccordionItem
} from "@ng-bootstrap/ng-bootstrap";
import {ScrobbleProviderNamePipe} from "../../_pipes/scrobble-provider-name.pipe";
import {ProviderImagePipe} from "../../_pipes/provider-image.pipe";
import {TagBadgeComponent} from "../../shared/tag-badge/tag-badge.component";
import {ScrobbleProviderDescriptionPipe} from "../manga-user-preferences/scrobble-provider-description.pipe";
import {UtcToLocalTimePipe} from "../../_pipes/utc-to-local-time.pipe";
import {ScrobbleEventType} from "../../_models/scrobbling/scrobble-event";
import {SettingSwitchComponent} from "../../settings/_components/setting-switch/setting-switch.component";

type ReadStatusTransitionRuleFromGroup = FormGroup<{
  enabled: FormControl<boolean>;
  days: FormControl<number>;
  transitionStatus: FormControl<ScrobbleReadStatus>;
  excludedPublicationStatus: FormControl<PublicationStatus[]>;
}>;

type ScrobbleProviderSettingsFormGroup = FormGroup<{
  progressScrobbling: FormControl<boolean>;
  wantToReadSync: FormControl<boolean>;
  ratingScrobbling: FormControl<boolean>;
  reviewScrobbling: FormControl<boolean>;
  reviewScrobbleTarget: FormControl<ReviewScrobbleTarget>;
  allLibraries: FormControl<boolean>;
  libraries: FormControl<number[]>;
  highestAgeRating:FormControl<AgeRating>;
  inactiveSeriesRule: ReadStatusTransitionRuleFromGroup;
  droppedSeriesRule: ReadStatusTransitionRuleFromGroup;
}>;

const ProviderSupportedEvents: Record<ScrobbleProvider, ScrobbleEventType[]> = {
  [ScrobbleProvider.AniList]: [ScrobbleEventType.ScoreUpdated, ScrobbleEventType.Review, ScrobbleEventType.ChapterRead, ScrobbleEventType.AddWantToRead],
  [ScrobbleProvider.Hardcover]: [ScrobbleEventType.ScoreUpdated, ScrobbleEventType.Review, ScrobbleEventType.ChapterRead, ScrobbleEventType.AddWantToRead],
  [ScrobbleProvider.Mal]: [ScrobbleEventType.AddWantToRead],
  [ScrobbleProvider.MangaBaka]: [ScrobbleEventType.ScoreUpdated, ScrobbleEventType.Review, ScrobbleEventType.ChapterRead, ScrobbleEventType.AddWantToRead],
  [ScrobbleProvider.Cbr]: [],
  [ScrobbleProvider.Kavita]: []
}

@Component({
  selector: 'app-manage-scrobble-providers',
  imports: [
    TranslocoDirective,
    NgbAccordionDirective,
    NgbAccordionItem,
    NgbAccordionHeader,
    NgbAccordionBody,
    ScrobbleProviderNamePipe,
    ProviderImagePipe,
    ReactiveFormsModule,
    NgbAccordionCollapse,
    NgbAccordionButton,
    TagBadgeComponent,
    ScrobbleProviderDescriptionPipe,
    UtcToLocalTimePipe,
    SettingSwitchComponent
  ],
  templateUrl: './manage-scrobble-providers.component.html',
  styleUrl: './manage-scrobble-providers.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ManageScrobbleProvidersComponent implements OnInit {

  protected readonly scrobbleService = inject(ScrobblingService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef$ = inject(DestroyRef);

  formGroups = signal<Map<ScrobbleProvider, ScrobbleProviderSettingsFormGroup>>(new Map());
  userScrobbleProviders = signal<Map<ScrobbleProvider, UserScrobbleProvider>>(new Map());

  ngOnInit() {
    this.scrobbleService.getScrobbleProviders()
      .pipe(tap(userScrobbleProviders => {
        const groups: Map<ScrobbleProvider, ScrobbleProviderSettingsFormGroup> = new Map();

        for (const p of userScrobbleProviders) {
          const group = this.buildScrobbleProviderSettingsFormGroup(p.settings);

          groups.set(p.provider, group);

          this.listenToChanges(p.provider, group);
        }

        this.userScrobbleProviders.set(new Map(userScrobbleProviders.map(p => [p.provider, p])));
        this.formGroups.set(groups);
      })).subscribe();
  }

  private listenToChanges(provider: ScrobbleProvider, group: ScrobbleProviderSettingsFormGroup) {
    group.valueChanges.pipe(
      takeUntilDestroyed(this.destroyRef$),
      distinctUntilChanged(),
      debounceTime(500),
      map(() => {
        const userScrobbleProvider = this.userScrobbleProviders().get(provider)!;

        return {
          ...userScrobbleProvider,
          settings: group.getRawValue(),
        }
      }),
      switchMap(s => this.scrobbleService.saveScrobbleSettings(s))
    ).subscribe();
  }

  private buildScrobbleProviderSettingsFormGroup(scrobbleSettings: ScrobbleProviderSettings): ScrobbleProviderSettingsFormGroup {
    return this.fb.group({
      progressScrobbling: this.fb.control(scrobbleSettings.progressScrobbling),
      wantToReadSync: this.fb.control(scrobbleSettings.wantToReadSync),
      ratingScrobbling: this.fb.control(scrobbleSettings.ratingScrobbling),
      reviewScrobbling: this.fb.control(scrobbleSettings.reviewScrobbling),
      reviewScrobbleTarget: this.fb.control(scrobbleSettings.reviewScrobbleTarget),
      allLibraries: this.fb.control(scrobbleSettings.allLibraries),
      libraries: this.fb.control(scrobbleSettings.libraries),
      highestAgeRating: this.fb.control(scrobbleSettings.highestAgeRating),
      inactiveSeriesRule: this.buildReadStatusTransitionRuleFromGroup(scrobbleSettings.inactiveSeriesRule),
      droppedSeriesRule: this.buildReadStatusTransitionRuleFromGroup(scrobbleSettings.droppedSeriesRule),
    });
  }

  private buildReadStatusTransitionRuleFromGroup(rule: ReadStatusTransitionRule): ReadStatusTransitionRuleFromGroup {
    return this.fb.group({
      enabled: this.fb.control(rule.enabled),
      days: this.fb.control(rule.days),
      transitionStatus: this.fb.control(rule.transitionStatus),
      excludedPublicationStatus: this.fb.control(rule.excludedPublicationStatus),
    })
  }

  protected disconnectScrobbleProvider(provider: ScrobbleProvider) {
    const group = this.formGroups().get(provider);
    if (!group) return;

    group.get('')
  }

  protected connectScrobbleProvider(provider: ScrobbleProvider) {}

  protected readonly ProviderSupportedEvents = ProviderSupportedEvents;
  protected readonly ScrobbleEventType = ScrobbleEventType;
}
