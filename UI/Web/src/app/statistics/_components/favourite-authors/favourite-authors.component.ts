import {ChangeDetectionStrategy, Component, inject, input, ViewEncapsulation} from '@angular/core';
import {StatisticsService} from "../../../_services/statistics.service";
import {TranslocoDirective} from "@jsverse/transloco";
import {LoadingComponent} from "../../../shared/loading/loading.component";
import {ImageService} from "../../../_services/image.service";
import {ImageComponent} from "../../../shared/image/image.component";
import {TagBadgeComponent} from "../../../shared/tag-badge/tag-badge.component";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule} from "@angular/forms";
import {toSignal} from "@angular/core/rxjs-interop";
import {SettingSwitchComponent} from "../../../settings/_components/setting-switch/setting-switch.component";
import {NgbTooltip} from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: 'app-favourite-authors',
  imports: [
    TranslocoDirective,
    LoadingComponent,
    ImageComponent,
    TagBadgeComponent,
    FormsModule,
    ReactiveFormsModule,
    SettingSwitchComponent,
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

  protected readonly favouriteAuthors = this.statsService.getFavouriteAuthors(() => this.userId());

  chapters(s: string): {Id: number, Title: string}[] {
    return JSON.parse(s)
  }

}
