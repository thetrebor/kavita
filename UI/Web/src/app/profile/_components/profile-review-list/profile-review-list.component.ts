import {ChangeDetectionStrategy, Component, inject, input} from '@angular/core';
import {TranslocoDirective} from "@jsverse/transloco";
import {NgxStarsModule} from "ngx-stars";
import {ReviewListItemComponent} from "../review-list-item/review-list-item.component";
import {VirtualScrollerModule} from "@iharbeck/ngx-virtual-scroller";
import {ThemeService} from "../../../_services/theme.service";
import {MemberInfo} from "../../../_models/user/member-info";
import {ReviewService} from "../../../_services/review.service";

@Component({
  selector: 'app-profile-review-list',
  imports: [
    TranslocoDirective,
    NgxStarsModule,
    ReviewListItemComponent,
    VirtualScrollerModule
  ],
  templateUrl: './profile-review-list.component.html',
  styleUrl: './profile-review-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileReviewListComponent {
  protected readonly themeService = inject(ThemeService);
  private readonly reviewService = inject(ReviewService);

  memberInfo = input.required<MemberInfo>();

  protected readonly reviewsResource = this.reviewService.getReviewsByUserResource(() => this.memberInfo().id);

  starColor = this.themeService.getCssVariable('--rating-star-color');
  filter: Record<string, number | string> = {};

  updateFilter(prop: string, value: number | string) {
    this.filter[prop] = value;
  }

}
