import {ChangeDetectionStrategy, Component, computed, inject, input} from '@angular/core';
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

  currentImageUrl = computed(() => {
    const userId = this.userId();
    return this.imageService.getUserCoverImage(userId);
  });

}
