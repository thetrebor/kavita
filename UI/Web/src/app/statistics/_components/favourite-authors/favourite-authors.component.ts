import {ChangeDetectionStrategy, Component, inject, input, ViewEncapsulation} from '@angular/core';
import {StatisticsService} from "../../../_services/statistics.service";
import {TranslocoDirective} from "@jsverse/transloco";
import {LoadingComponent} from "../../../shared/loading/loading.component";
import {ImageService} from "../../../_services/image.service";
import {ImageComponent} from "../../../shared/image/image.component";
import {TagBadgeComponent} from "../../../shared/tag-badge/tag-badge.component";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {NgbTooltip} from "@ng-bootstrap/ng-bootstrap";
import {StatsFilter} from "../../_models/stats-filter";

@Component({
  selector: 'app-favourite-authors',
  imports: [
    TranslocoDirective,
    LoadingComponent,
    ImageComponent,
    TagBadgeComponent,
    FormsModule,
    ReactiveFormsModule,
    NgbTooltip
  ],
  templateUrl: './favourite-authors.component.html',
  styleUrl: './favourite-authors.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class FavouriteAuthorsComponent {

  private readonly statsService = inject(StatisticsService);
  protected readonly imageService = inject(ImageService);

  userId = input.required<number>();
  userName = input.required<string>();
  statsFilter = input.required<StatsFilter>();

  protected readonly favouriteAuthors = this.statsService.getFavouriteAuthors(
    () => this.statsFilter(),
    () => this.userId(),
  );

  chapters(s: string): {ChapterId: number, Title: string}[] {
    return JSON.parse(s)
  }

}
