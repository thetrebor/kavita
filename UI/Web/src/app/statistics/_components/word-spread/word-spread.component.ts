import {ChangeDetectionStrategy, Component, input} from '@angular/core';

@Component({
  selector: 'app-word-spread',
  imports: [],
  templateUrl: './word-spread.component.html',
  styleUrl: './word-spread.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WordSpreadComponent {
  userId = input.required<number>();
}
