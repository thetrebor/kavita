import {Directive, inject, OnDestroy, OnInit, TemplateRef} from "@angular/core";
import {PreferenceHeaderService} from "../../_services/preference-header.service";

@Directive({
  selector: '[appPreferenceHeaderActions]',
  standalone: true
})
export class PreferenceHeaderActionsDirective implements OnInit, OnDestroy {
  private template = inject(TemplateRef);
  private prefTitleService = inject(PreferenceHeaderService);

  ngOnInit() {
    this.prefTitleService.setTemplate(this.template);
  }

  ngOnDestroy() {
    this.prefTitleService.clearTemplate();
  }
}
