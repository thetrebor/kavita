import {ChangeDetectionStrategy, Component, inject, input} from '@angular/core';
import {TranslocoDirective} from "@jsverse/transloco";
import {ReadingSession} from "../../_models/progress/reading-session";
import {ImageService} from "../../_services/image.service";
import {TimeAgoPipe} from "../../_pipes/time-ago.pipe";
import {DefaultValuePipe} from "../../_pipes/default-value.pipe";
import {RouterLink} from "@angular/router";
import {ImageComponent} from "../../shared/image/image.component";

@Component({
  selector: 'app-activity-card',
  imports: [
    TranslocoDirective,
    TimeAgoPipe,
    DefaultValuePipe,
    RouterLink,
    ImageComponent
  ],
  templateUrl: './activity-card.component.html',
  styleUrl: './activity-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityCardComponent {

  protected readonly imageService = inject(ImageService);

  session = input.required<ReadingSession>();

}
