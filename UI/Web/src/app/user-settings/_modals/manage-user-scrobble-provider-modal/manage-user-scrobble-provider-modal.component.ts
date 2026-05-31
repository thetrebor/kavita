import {ChangeDetectionStrategy, Component, computed, inject, model, OnInit} from '@angular/core';
import {UserScrobbleProvider} from "../../../_models/kavitaplus/scrobble-provider-settings";
import {ScrobbleProvider, ScrobblingService} from "../../../_services/scrobbling.service";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {translate, TranslocoDirective} from "@jsverse/transloco";
import {FormGroup, NonNullableFormBuilder, ReactiveFormsModule, Validators} from "@angular/forms";
import {SettingItemComponent} from "../../../settings/_components/setting-item/setting-item.component";
import {DefaultValuePipe} from "../../../_pipes/default-value.pipe";
import {ScrobbleProviderNamePipe} from "../../../_pipes/scrobble-provider-name.pipe";
import {TruncatePipe} from "../../../_pipes/truncate.pipe";
import {UtcToLocalTimePipe} from "../../../_pipes/utc-to-local-time.pipe";
import {TimeAgoPipe} from "../../../_pipes/time-ago.pipe";
import {ProviderImagePipe} from "../../../_pipes/provider-image.pipe";
import {UtcToLocalDatePipe} from "../../../_pipes/utc-to-locale-date.pipe";
import {ToastrService} from "ngx-toastr";
import {
  ScrobbleProviderImageComponent
} from "../../../shared/_components/scrobble-provider-image/scrobble-provider-image.component";

@Component({
  selector: 'app-manage-user-scrobble-provider-modal-modal',
  imports: [
    TranslocoDirective,
    ReactiveFormsModule,
    SettingItemComponent,
    DefaultValuePipe,
    ScrobbleProviderNamePipe,
    TruncatePipe,
    UtcToLocalTimePipe,
    TimeAgoPipe,
    ProviderImagePipe,
    UtcToLocalDatePipe,
    ScrobbleProviderImageComponent
  ],
  templateUrl: './manage-user-scrobble-provider-modal.component.html',
  styleUrl: './manage-user-scrobble-provider-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ManageUserScrobbleProviderModalComponent implements OnInit {

  private readonly scrobblingService = inject(ScrobblingService);
  private readonly modal = inject(NgbActiveModal);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly toastr = inject(ToastrService);

  userScrobbleProvider = model.required<UserScrobbleProvider>();

  generateTokenLink = computed<string | null>(() => {
    switch (this.userScrobbleProvider().provider) {
      case ScrobbleProvider.AniList:
        return "https://anilist.co/api/v2/oauth/authorize?client_id=12809&redirect_url=https://anilist.co/api/v2/oauth/pin&response_type=token";
      case ScrobbleProvider.Hardcover:
        return "https://hardcover.app/account/api";
      case ScrobbleProvider.MangaBaka:
        return "https://mangabaka.org/my/settings/api-and-apps";
    }

    return null;
  });

  canGenerateEvents = computed(() => {
    if (this.userScrobbleProvider().provider === ScrobbleProvider.Mal) {
      return false;
    }

    return this.userScrobbleProvider().authenticationToken !== '';
  });

  formGroup!: FormGroup;

  ngOnInit() {
    this.formGroup = this.fb.group({
      userName: [this.userScrobbleProvider().userName],
      authenticationToken: [this.userScrobbleProvider().authenticationToken],
    });
  }

  generateEvents() {
    this.scrobblingService.triggerScrobbleEventGeneration(this.userScrobbleProvider().provider).subscribe(_ => {
      this.toastr.info(translate('toasts.scrobble-gen-init'));
      this.close();
    });
  }

  close() {
    this.modal.close();
  }

  save() {
    this.scrobblingService.saveUserScrobbleProvider({
      ...this.userScrobbleProvider(),
      ...this.formGroup.value,
    }).subscribe(() => this.close());
  }

  protected readonly ScrobbleProvider = ScrobbleProvider;
}
