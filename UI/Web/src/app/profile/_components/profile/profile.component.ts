import {
  ChangeDetectionStrategy,
  Component,
  computed, DestroyRef,
  effect,
  inject,
  input,
  model
} from '@angular/core';
import {Location} from '@angular/common';
import {ReadingPaceComponent} from "../../../statistics/_components/reading-pace/reading-pace.component";
import {ActivityGraphComponent} from "../../../statistics/_components/activity-graph/activity-graph.component";
import {MemberInfo} from "../../../_models/user/member-info";
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
import {tap} from "rxjs";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {ActivatedRoute} from "@angular/router";
import {ReviewListItemComponent} from "../review-list-item/review-list-item.component";
import {PreferredFormatComponent} from "../../../statistics/_components/preferred-format/preferred-format.component";

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
    ReviewListItemComponent,
    PreferredFormatComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent {

  private readonly location = inject(Location);
  private readonly reviewService = inject(ReviewService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly imageService = inject(ImageService);

  // Set by angular from the resolver
  memberInfo = input.required<MemberInfo>();
  reviewsResource = this.reviewService
    .getReviewsByUserResource(() => this.memberInfo().id);

  hasCoverImage = computed(() => false);
  activeTabId = model<TabID>(TabID.Overview);

  constructor() {
    this.route.fragment.pipe(tap(frag => {
      const fragId = frag as TabID;
      if (frag !== null && this.activeTabId() !== fragId) {
        this.updateUrl(fragId);
        this.activeTabId.set(fragId); // BUG: This is not auto-selecting the active tab
      }
    }), takeUntilDestroyed(this.destroyRef)).subscribe();
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
