import {Routes} from "@angular/router";
import {ProfileComponent} from "../profile/_components/profile/profile.component";
import {memberInfoResolver} from "../_resolvers/member-info.resolver";


export const routes: Routes = [
  {
    path: ':userId',
    component: ProfileComponent,
    pathMatch: 'full',
    resolve: {
      memberInfo: memberInfoResolver
    }
  },
];
