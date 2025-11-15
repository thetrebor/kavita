import {ChangeDetectionStrategy, Component, input} from '@angular/core';

@Component({
  selector: 'app-preferred-tag',
  imports: [],
  templateUrl: './preferred-tag.component.html',
  styleUrl: './preferred-tag.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PreferredTagComponent {
  userId = input.required<number>();
}
