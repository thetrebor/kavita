import {ChangeDetectionStrategy, Component, inject, Input} from '@angular/core';
import {ProviderImagePipe} from "../../../_pipes/provider-image.pipe";
import {UserCollection} from "../../../_models/collection-tag";
import {TranslocoDirective} from "@jsverse/transloco";
import {AccountService} from "../../../_services/account.service";
import {ImageComponent} from "../../../shared/image/image.component";
import {NgbTooltip} from "@ng-bootstrap/ng-bootstrap";
import {ScrobbleProviderNamePipe} from "../../../_pipes/scrobble-provider-name.pipe";
import {ScrobbleProvider} from "../../../_models/kavitaplus/scrobble-providers/scrobble-provider.enum";

@Component({
  selector: 'app-collection-owner',
  imports: [
    ProviderImagePipe,
    TranslocoDirective,
    ImageComponent,
    NgbTooltip,
    ScrobbleProviderNamePipe
  ],
  templateUrl: './collection-owner.component.html',
  styleUrl: './collection-owner.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CollectionOwnerComponent {

  protected readonly accountService = inject(AccountService);

  @Input({required: true}) collection!: UserCollection;

  protected readonly ScrobbleProvider = ScrobbleProvider;
}
