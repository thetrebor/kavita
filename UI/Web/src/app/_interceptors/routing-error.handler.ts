import {NavigationError} from "@angular/router";


export function routingErrorHandler(err: NavigationError) {
  console.error(err)
}
