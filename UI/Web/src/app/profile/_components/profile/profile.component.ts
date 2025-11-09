import {ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, model} from '@angular/core';
import {Location} from '@angular/common';
import {ReadingPaceComponent} from "../../../statistics/_components/reading-pace/reading-pace.component";
import {ActivityGraphComponent} from "../../../statistics/_components/activity-graph/activity-graph.component";
import {MemberInfo} from "../../../_models/user/member-info";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {ActivatedRoute, Router} from "@angular/router";
import {TranslocoDirective} from "@jsverse/transloco";
import {ImageComponent} from "../../../shared/image/image.component";
import {ImageService} from "../../../_services/image.service";
import {UtcToLocalTimePipe} from "../../../_pipes/utc-to-local-time.pipe";
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
import {UserReview} from "../../../_models/user-review";
import {ReviewCardComponent} from "../../../_single-module/review-card/review-card.component";

enum TabID {
  Overview = 'overview-tab',
  Stats = 'stats-tab',
  Reviews = 'reviews-tab',
}

@Component({
  selector: 'app-profile',
  imports: [
    ReadingPaceComponent,
    ActivityGraphComponent,
    TranslocoDirective,
    ImageComponent,
    UtcToLocalTimePipe,
    TimeAgoPipe,
    NgbNav,
    NgbNavContent,
    NgbNavLink,
    NgbNavItem,
    NgbNavOutlet,
    ReviewCardComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent {

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly location = inject(Location);
  private readonly reviewService = inject(ReviewService);
  protected readonly imageService = inject(ImageService);

  userInfo = model<MemberInfo | null>(null);
  reviews = model<UserReview[]>([]);
  hasCoverImage = computed(() => false);
  activeTabId = model<TabID>(TabID.Overview);

  constructor() {
    this.route.data.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(data => {
      this.userInfo.set(data['memberInfo']);

      if (data['memberInfo'] == null) {
        this.router.navigateByUrl('/home');
        return;
      }
    });

    effect(() => {
      if (!this.userInfo()) return;
      this.reviewService.getReviewsFromUser(this.userInfo()!.id || 0).subscribe(reviews => {
        this.reviews.set(reviews);
      })
    })
  }

  onNavChange(event: NgbNavChangeEvent) {
    this.updateUrl(event.nextId);
    this.activeTabId.set(event.nextId);
  }

  updateUrl(activeTab: TabID) {
    const tokens = this.location.path().split('#');
    const newUrl = `${tokens[0]}#${activeTab}`;
    this.location.replaceState(newUrl) // TODO: Look into making this a directive for tabs
  }


  protected readonly TabID = TabID;

}
