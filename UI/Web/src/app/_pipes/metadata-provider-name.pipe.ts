import {Pipe, PipeTransform} from '@angular/core';
import {MetadataProvider} from "../_models/kavitaplus/metadata-provider.enum";

@Pipe({
  name: 'metadataProviderName',
  standalone: true
})
export class MetadataProviderNamePipe implements PipeTransform {

  transform(value: MetadataProvider): string {
    switch (value) {
      case MetadataProvider.Hardcover: return 'Hardcover';
      case MetadataProvider.Mangabaka: return 'MangaBaka';
      case MetadataProvider.ComicBookRoundup: return 'Comicbook Roundup';
    }
  }

}
