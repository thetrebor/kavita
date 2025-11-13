import { ChangeDetectionStrategy, Component } from '@angular/core';
import {TranslocoDirective} from "@jsverse/transloco";

@Component({
  selector: 'app-fiction-graph',
  imports: [
    TranslocoDirective
  ],
  templateUrl: './fiction-graph.component.html',
  styleUrl: './fiction-graph.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FictionGraphComponent {

}
