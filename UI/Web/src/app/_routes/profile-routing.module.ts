import {Routes} from "@angular/router";
import {ProfileComponent} from "../profile/_components/profile/profile.component";


export const routes: Routes = [
  {path: '', component: ProfileComponent, pathMatch: 'full'},
];
