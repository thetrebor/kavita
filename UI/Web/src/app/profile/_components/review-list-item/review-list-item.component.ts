import {ChangeDetectionStrategy, Component, computed, inject, input} from '@angular/core';
import {TranslocoDirective} from "@jsverse/transloco";
import {DatePipe} from "@angular/common";
import {ImageComponent} from "../../../shared/image/image.component";
import {NgbProgressbar} from "@ng-bootstrap/ng-bootstrap";
import {ReadMoreComponent} from "../../../shared/read-more/read-more.component";
import {SeriesFormatComponent} from "../../../shared/series-format/series-format.component";
import {UserReview, UserReviewExtended} from "../../../_models/user-review";
import {ImageService} from "../../../_services/image.service";
import {CardActionablesComponent} from "../../../_single-module/card-actionables/card-actionables.component";

@Component({
  selector: 'app-review-list-item',
  imports: [
    TranslocoDirective,
    DatePipe,
    ImageComponent,
    ReadMoreComponent,
    CardActionablesComponent
  ],
  templateUrl: './review-list-item.component.html',
  styleUrl: './review-list-item.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReviewListItemComponent {

  protected readonly imageService = inject(ImageService);

  review = input.required<UserReviewExtended>();

  imageUrl = computed(() => {
    const item = this.review();
    if (item.chapterId) {
      return this.imageService.getChapterCoverImage(item.chapterId);
    }

    return this.imageService.getSeriesCoverImage(item.seriesId);
  });

  title = computed(() => {
    const item = this.review();
    if (item.chapterId) {
      return item.chapter?.title || item.series.name;
    }

    return item.series.name;
  });

}
