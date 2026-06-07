import {Pipe, PipeTransform} from '@angular/core';
import {MetadataProvider} from "../_models/kavitaplus/metadata-provider.enum";

@Pipe({
  name: 'metadataProviderImage',
  standalone: true
})
export class MetadataProviderImagePipe implements PipeTransform {

  transform(value: MetadataProvider, large: boolean = false): string {
    switch (value) {
      case MetadataProvider.Hardcover:
        return `assets/images/ExternalServices/hardcover${large ? '-lg' : ''}.png`;
      case MetadataProvider.Mangabaka:
        return `assets/images/ExternalServices/mangabaka${large ? '-lg' : ''}.png`;
      case MetadataProvider.ComicBookRoundup:
        return `assets/images/ExternalServices/ComicBookRoundup.png`;
    }
  }

}
