import {ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit} from '@angular/core';
import {TranslocoDirective} from "@jsverse/transloco";
import {MemberService} from "../../_services/member.service";
import {DefaultValuePipe} from "../../_pipes/default-value.pipe";
import {UtcToLocalTimePipe} from "../../_pipes/utc-to-local-time.pipe";
import {VirtualScrollerModule} from "@iharbeck/ngx-virtual-scroller";
import {UserTokenInfo} from "../../_models/kavitaplus/user-token-info";
import {NgxDatatableModule} from "@siemens/ngx-datatable";
import {ResponsiveTableComponent} from "../../shared/_components/responsive-table/responsive-table.component";
import {ScrobbleProvider, ScrobblingService} from "../../_services/scrobbling.service";
import {ScrobbleProviderNamePipe} from "../../_pipes/scrobble-provider-name.pipe";
import {
  ScrobbleProviderImageComponent
} from "../../shared/_components/scrobble-provider-image/scrobble-provider-image.component";

@Component({
  selector: 'app-manage-user-tokens',
  imports: [
    TranslocoDirective,
    DefaultValuePipe,
    UtcToLocalTimePipe,
    VirtualScrollerModule,
    NgxDatatableModule,
    ResponsiveTableComponent,
    ScrobbleProviderNamePipe,
    ScrobbleProviderImageComponent
  ],
  templateUrl: './manage-user-tokens.component.html',
  styleUrl: './manage-user-tokens.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageUserTokensComponent implements OnInit {

  private readonly cdRef = inject(ChangeDetectorRef);
  private readonly memberService = inject(MemberService);
  protected readonly scrobblingService = inject(ScrobblingService);

  isLoading = true;
  users: UserTokenInfo[] = [];

  trackBy = (idx: number, item: UserTokenInfo) => item;

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.cdRef.markForCheck();

    this.memberService.getUserTokenInfo().subscribe(users => {
      this.users = users;
      this.isLoading = false;
      this.cdRef.markForCheck();
    });
  }

  getTokenValidityInfo(user: UserTokenInfo, provider: ScrobbleProvider) {
    return user.tokens.find(token => token.provider == provider);
  }
}
