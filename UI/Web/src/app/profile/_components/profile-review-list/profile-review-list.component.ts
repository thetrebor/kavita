import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  model,
  Resource, ViewChild
} from '@angular/core';
import {TranslocoDirective} from "@jsverse/transloco";
import {NgxStarsComponent, NgxStarsModule} from "ngx-stars";
import {ReviewListItemComponent} from "../review-list-item/review-list-item.component";
import {VirtualScrollerModule} from "@iharbeck/ngx-virtual-scroller";
import {ThemeService} from "../../../_services/theme.service";
import {MemberInfo} from "../../../_models/user/member-info";
import {ReviewService} from "../../../_services/review.service";
import {FormControl, FormGroup, ReactiveFormsModule} from "@angular/forms";
import {UserReviewExtended} from "../../../_models/user-review";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {debounceTime, distinctUntilChanged, tap} from "rxjs";

@Component({
  selector: 'app-profile-review-list',
  imports: [
    TranslocoDirective,
    NgxStarsModule,
    ReviewListItemComponent,
    VirtualScrollerModule,
    ReactiveFormsModule
  ],
  templateUrl: './profile-review-list.component.html',
  styleUrl: './profile-review-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileReviewListComponent {
  private readonly themeService = inject(ThemeService);
  private readonly reviewService = inject(ReviewService);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild(NgxStarsComponent) starsComponent!: NgxStarsComponent;

  memberInfo = input.required<MemberInfo>();

  reviews = model<UserReviewExtended[]>([]);
  rating = model<number>(0);
  nameFilter = model<string>('');


  starColor = this.themeService.getCssVariable('--rating-star-color');
  formGroup = new FormGroup({
    query: new FormControl('', []),
    rating: new FormControl(0, []),
  });

  trackByReview = (_index: number, review: UserReviewExtended): string => {
    return `${review.id}-${review.seriesId}-${review.chapterId || review.createdUtc}`;
  };
  compareReviews = (item1: UserReviewExtended, item2: UserReviewExtended): boolean => {
    if (!item1 && !item2) return true;
    if (!item1 || !item2) return false;

    // Use the trackBy function to get the identity and compare
    return this.trackByReview(0, item1) === this.trackByReview(0, item2);
  };

  constructor() {

    // BUG: This is triggering (likely from virtual scroller) on every resize
    effect(() => {
      const userId = this.memberInfo().id;
      const query = this.nameFilter();
      const ratingFilter = this.rating();

      console.log('userId/query/rating: ', userId, '/', query, '/', ratingFilter);

      this.reviewService.getReviewsByUser(userId, query, ratingFilter).subscribe(res => {
        this.reviews.set(res);
      });
    });

    this.formGroup.get('query')?.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(val => this.nameFilter.set(val ?? '')),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  updateRating(rating: number) {
    this.rating.set(rating);
  }

  resetRating() {
    this.starsComponent.setRating(0);
  }
}
