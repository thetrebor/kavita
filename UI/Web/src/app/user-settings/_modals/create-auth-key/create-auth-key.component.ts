import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {TranslocoDirective} from "@jsverse/transloco";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {SettingItemComponent} from "../../../settings/_components/setting-item/setting-item.component";
import {DatePipe} from "@angular/common";

@Component({
  selector: 'app-create-auth-key',
  imports: [
    TranslocoDirective,
    ReactiveFormsModule,
    SettingItemComponent,
    DatePipe
  ],
  templateUrl: './create-auth-key.component.html',
  styleUrl: './create-auth-key.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateAuthKeyComponent {

  private readonly modalRef = inject(NgbActiveModal);

  settingsForm: FormGroup = new FormGroup({
    name: new FormControl('', [Validators.required]),
    keyLength: new FormControl(8, [Validators.required]),
    expiration: new FormControl('', []),
  });


  close() {
    this.modalRef.dismiss();
  }

  save() {


    this.modalRef.close(true);
  }
}
