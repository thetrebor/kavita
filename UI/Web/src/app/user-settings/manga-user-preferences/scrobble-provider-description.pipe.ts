import {Pipe, PipeTransform} from '@angular/core';
import {translate} from "@jsverse/transloco";
import {ScrobbleProvider} from "../../_models/kavitaplus/scrobble-providers/scrobble-provider.enum";

@Pipe({
  name: 'scrobbleProviderDescription',
})
export class ScrobbleProviderDescriptionPipe implements PipeTransform {

  transform(value: ScrobbleProvider): string {
    switch (value) {
      case ScrobbleProvider.AniList:
        return translate('scrobble-provider-description-pipe.anilist');
      case ScrobbleProvider.Mal:
        return translate('scrobble-provider-description-pipe.myanimelist');
      case ScrobbleProvider.Cbr:
        return translate('scrobble-provider-description-pipe.comicbookroundup');
      case ScrobbleProvider.Hardcover:
        return translate('scrobble-provider-description-pipe.hardcover');
      case ScrobbleProvider.MangaBaka:
        return translate('scrobble-provider-description-pipe.mangabaka');
      default:
        return '';
    }
  }

}
