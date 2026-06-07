import {ChangeDetectionStrategy, Component, inject, input, OnInit, output, signal} from '@angular/core';
import {NgOptimizedImage} from "@angular/common";
import {TranslocoDirective} from "@jsverse/transloco";
import {Series} from "../../_models/series";
import {SeriesService} from "../../_services/series.service";
import {allMetadataProviders, MetadataProvider} from "../../_models/kavitaplus/metadata-provider.enum";
import {MetadataProviderNamePipe} from "../../_pipes/metadata-provider-name.pipe";
import {MetadataProviderImagePipe} from "../../_pipes/metadata-provider-image.pipe";

/**
 * Per metadata-provider match state, derived from the series' external ids and the exclusion set.
 */
export type ProviderMatchStatus = 'matched' | 'excluded' | 'not-found';

@Component({
  selector: 'app-manage-series-exclusions',
  imports: [
    TranslocoDirective,
    NgOptimizedImage,
    MetadataProviderNamePipe,
    MetadataProviderImagePipe,
  ],
  templateUrl: './manage-series-exclusions.component.html',
  styleUrl: './manage-series-exclusions.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ManageSeriesExclusionsComponent implements OnInit {
  private readonly seriesService = inject(SeriesService);

  series = input.required<Series>();

  /** Emitted when the user wants to navigate back to the match step. */
  back = output<void>();
  /** Emitted once exclusions have been persisted. */
  saved = output<void>();

  protected readonly allMetadataProviders = allMetadataProviders;

  /** Local, editable set of excluded metadata providers. */
  protected readonly excluded = signal<MetadataProvider[]>([]);
  /** Local, editable "blacklist everywhere" toggle state. */
  protected readonly blacklisted = signal<boolean>(false);
  protected readonly isSaving = signal<boolean>(false);

  ngOnInit() {
    this.blacklisted.set(this.series().isBlacklisted);

    // TODO: load the current exclusion set for this series
    // this.seriesService.getMetadataBlacklist(this.series().id)
    //   .subscribe(providers => this.excluded.set(providers));
  }

  isExcluded(provider: MetadataProvider): boolean {
    return this.excluded().includes(provider);
  }

  toggleExclusion(provider: MetadataProvider): void {
    // TODO: confirm desired behavior (optimistic local toggle, persisted on save)
    this.excluded.update(curr => curr.includes(provider)
      ? curr.filter(p => p !== provider)
      : [...curr, provider]);
  }

  excludedCount(): number {
    return this.excluded().length;
  }

  providerStatus(provider: MetadataProvider): ProviderMatchStatus {
    if (this.isExcluded(provider)) return 'excluded';

    // TODO: return 'matched' when the series has this provider's external id set, else 'not-found'.
    // Map provider -> series id field (Hardcover -> hardcoverId, Mangabaka -> mangaBakaId, CBR -> cbrId)
    // once those ids are exposed on the Series model.
    return 'not-found';
  }

  toggleBlacklist(): void {
    // TODO: wire up blacklist persistence
    this.blacklisted.update(v => !v);
  }

  canSave(): boolean {
    // TODO: compute from dirty state
    return !this.isSaving();
  }

  save(): void {
    // TODO: persist via seriesService.updateMetadataProviderExclusions(this.series().id, this.excluded())
    // then emit saved on success.
    this.saved.emit();
  }
}
