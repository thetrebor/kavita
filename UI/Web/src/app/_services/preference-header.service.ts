import {Injectable, signal, TemplateRef} from '@angular/core';

/**
 * This is responsible for Preferences children components to inject custom actions (top-right) into the parent component
 */
@Injectable({
  providedIn: 'root'
})
export class PreferenceHeaderService {
  private readonly _currentTemplate = signal<TemplateRef<any> | null>(null);
  public readonly currentTemplate = this._currentTemplate.asReadonly();

  setTemplate(template: TemplateRef<any>) {
    this._currentTemplate.set(template);
  }

  clearTemplate() {
    this._currentTemplate.set(null);
  }
}
