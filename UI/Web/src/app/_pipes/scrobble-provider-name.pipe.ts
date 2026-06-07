import {Pipe, PipeTransform} from '@angular/core';
import {ScrobbleProvider} from "../_models/kavitaplus/scrobble-providers/scrobble-provider.enum";

@Pipe({
  name: 'scrobbleProviderName',
  standalone: true
})
export class ScrobbleProviderNamePipe implements PipeTransform {

  transform(value: ScrobbleProvider): string {
    switch (value) {
      case ScrobbleProvider.AniList: return 'AniList';
      case ScrobbleProvider.Mal: return 'MAL';
      case ScrobbleProvider.Kavita: return 'Kavita';
      case ScrobbleProvider.Cbr: return 'Comicbook Roundup';
      case ScrobbleProvider.Hardcover: return 'Hardcover';
      case ScrobbleProvider.MangaBaka: return 'MangaBaka';
    }
  }

}
