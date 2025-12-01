import {ChangeDetectionStrategy, ChangeDetectorRef, Component, DestroyRef, inject} from '@angular/core';
import {ApiKeyComponent} from "../api-key/api-key.component";
import {translate, TranslocoDirective} from "@jsverse/transloco";
import {AccountService} from "../../_services/account.service";
import {SettingsService} from "../../admin/settings.service";
import {User} from "../../_models/user/user";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {WikiLink} from "../../_models/wiki";
import {LicenseService} from "../../_services/license.service";
import {ColumnMode, NgxDatatableModule} from "@siemens/ngx-datatable";
import {AuthKey, AuthKeyProvider} from "../../_models/user/auth-key";
import {UtcToLocaleDatePipe} from "../../_pipes/utc-to-locale-date.pipe";
import {DefaultDatePipe} from "../../_pipes/default-date.pipe";
import {ToggleVisibilityDirective} from "../../_directives/toggle-visibility.directive";
import {ConfirmService} from "../../shared/confirm.service";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {DefaultModalOptions} from "../../_models/default-modal-options";
import {Device} from "../../_models/device/device";
import {CreateAuthKeyComponent} from "../_modals/create-auth-key/create-auth-key.component";

@Component({
  selector: 'app-manage-auth-keys',
  imports: [
    ApiKeyComponent,
    TranslocoDirective,
    NgxDatatableModule,
    UtcToLocaleDatePipe,
    DefaultDatePipe,
    ToggleVisibilityDirective,

  ],
  templateUrl: './manage-auth-keys.component.html',
  styleUrl: './manage-auth-keys.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageAuthKeysComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly accountService = inject(AccountService);
  private readonly settingsService = inject(SettingsService);
  private readonly cdRef = inject(ChangeDetectorRef);
  private readonly licenseService = inject(LicenseService);
  private readonly confirmService = inject(ConfirmService);
  private readonly modalService = inject(NgbModal);


  user: User | undefined = undefined;
  opdsUrlLink = `<a href="${WikiLink.OpdsClients}" target="_blank" rel="noopener noreferrer">Wiki</a>`

  opdsEnabled: boolean = false;
  opdsUrl: string = '';
  hasActiveLicense = false;
  makeUrl: (val: string) => string = (val: string) => { return this.opdsUrl; };

  protected readonly authKeysResource = this.accountService.getAuthKeysResource();

  constructor() {
    this.accountService.getOpdsUrl().subscribe(res => {
      this.opdsUrl = res;
      this.cdRef.markForCheck();
    });

    this.settingsService.getOpdsEnabled().subscribe(res => {
      this.opdsEnabled = res;
      this.cdRef.markForCheck();
    });

    this.licenseService.hasValidLicense$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(res => {
      this.hasActiveLicense = res;
      this.cdRef.markForCheck();
    });
  }

  createAuthKey() {
    const ref = this.modalService.open(CreateAuthKeyComponent, DefaultModalOptions);
    ref.componentInstance.device = null;

    ref.closed.subscribe((result: Device | null) => {
      if (result === null) return;

      //this.authKeysResource.
      //this.loadDevices();
    });
  }

  rotate(authKey: AuthKey) {
    // TODO
  }

  async delete(authKey: AuthKey) {
    if (!await this.confirmService.confirm(translate('toasts.confirm-delete-auth-key'))) {
      return;
    }
    // TODO
  }

  protected readonly ColumnMode = ColumnMode;
  protected readonly AuthKeyProvider = AuthKeyProvider;
}
