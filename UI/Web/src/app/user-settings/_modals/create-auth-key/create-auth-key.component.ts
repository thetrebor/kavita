import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {TranslocoDirective} from "@jsverse/transloco";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {ToastrService} from "ngx-toastr";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {devicePlatforms} from "../../../_models/device/device-platform";

@Component({
  selector: 'app-create-auth-key',
  imports: [
    TranslocoDirective,
    ReactiveFormsModule
  ],
  templateUrl: './create-auth-key.component.html',
  styleUrl: './create-auth-key.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateAuthKeyComponent {

  private readonly toastr = inject(ToastrService);
  private readonly modalRef = inject(NgbActiveModal);

  settingsForm: FormGroup = new FormGroup({
    name: new FormControl('', [Validators.required]),
    expiration: new FormControl('', [Validators.required]),
    keyLength: new FormControl(8, [Validators.required]),
  });


  close() {
    this.modalRef.dismiss();
  }

  save() {
    this.modalRef.close(true);
  }

  protected readonly devicePlatforms = devicePlatforms;
}
