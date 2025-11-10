import {ChangeDetectionStrategy, Component, effect, inject, input, model} from '@angular/core';
import {AgeRating} from "../../_models/metadata/age-rating";
import {ImageComponent} from "../../shared/image/image.component";
import {NgbTooltip} from "@ng-bootstrap/ng-bootstrap";
import {AgeRatingPipe} from "../../_pipes/age-rating.pipe";
import {FilterUtilitiesService} from "../../shared/_services/filter-utilities.service";
import {FilterComparison} from "../../_models/metadata/v2/filter-comparison";
import {FilterField} from "../../_models/metadata/v2/filter-field";

const basePath = './assets/images/ratings/';

@Component({
  selector: 'app-age-rating-image',
  imports: [
      ImageComponent,
      NgbTooltip,
      AgeRatingPipe,
  ],
  standalone: true,
  templateUrl: './age-rating-image.component.html',
  styleUrl: './age-rating-image.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
})
export class AgeRatingImageComponent {
  private readonly filterUtilityService = inject(FilterUtilitiesService);

  rating = input.required<AgeRating>();

  imageUrl = model<string>('unknown-rating.png');

  constructor() {
    effect(() => {
      this.rating();
      this.setImage();
    });
  }

  setImage() {
    switch (this.rating()) {
      case AgeRating.Unknown:
        this.imageUrl.set(basePath + 'unknown-rating.png');
        break;
      case AgeRating.RatingPending:
        this.imageUrl.set(basePath + 'rating-pending-rating.png');
        break;
      case AgeRating.EarlyChildhood:
        this.imageUrl.set(basePath + 'early-childhood-rating.png');
        break;
      case AgeRating.Everyone:
        this.imageUrl.set(basePath + 'everyone-rating.png');
        break;
      case AgeRating.G:
        this.imageUrl.set(basePath + 'g-rating.png');
        break;
      case AgeRating.Everyone10Plus:
        this.imageUrl.set(basePath + 'everyone-10+-rating.png');
        break;
      case AgeRating.PG:
        this.imageUrl.set(basePath + 'pg-rating.png');
        break;
      case AgeRating.KidsToAdults:
        this.imageUrl.set(basePath + 'kids-to-adults-rating.png');
        break;
      case AgeRating.Teen:
        this.imageUrl.set(basePath + 'teen-rating.png');
        break;
      case AgeRating.Mature15Plus:
        this.imageUrl.set(basePath + 'ma15+-rating.png');
        break;
      case AgeRating.Mature17Plus:
        this.imageUrl.set(basePath + 'mature-17+-rating.png');
        break;
      case AgeRating.Mature:
        this.imageUrl.set(basePath + 'm-rating.png');
        break;
      case AgeRating.R18Plus:
        this.imageUrl.set(basePath + 'r18+-rating.png');
        break;
      case AgeRating.AdultsOnly:
        this.imageUrl.set(basePath + 'adults-only-18+-rating.png');
        break;
      case AgeRating.X18Plus:
        this.imageUrl.set(basePath + 'x18+-rating.png');
        break;
    }
  }

  openRating() {
    this.filterUtilityService.applyFilter(['all-series'], FilterField.AgeRating, FilterComparison.Equal, `${this.rating}`).subscribe();
  }


}
