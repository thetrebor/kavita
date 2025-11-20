import {ChangeDetectionStrategy, Component, effect, inject, input, model} from '@angular/core';
import {ImageService} from "../../_services/image.service";
import {ImageComponent} from "../../shared/image/image.component";

@Component({
  selector: 'app-profile-icon',
  imports: [
    ImageComponent
  ],
  templateUrl: './profile-icon.component.html',
  styleUrl: './profile-icon.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileIconComponent {
  protected readonly imageService = inject(ImageService);

  userId = input.required<number>();
  size = input<number>(32);

  currentImageUrl = model<string | null>(null);

  constructor() {

    effect(() => {
      // Show preview if available, otherwise show existing image
      const userId = this.userId();
      const url =  userId && this.imageService.getUserCoverImage(userId) || null;

      this.currentImageUrl.set(url);
    });

  }

}
