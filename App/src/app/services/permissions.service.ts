import { Injectable } from '@angular/core';
import { NgxPermissionsService } from 'ngx-permissions';
import { AccountService, MembershipState } from './account.service';
import { SessionService } from './session.service';
import { Permissions } from './permissions';
import * as log from 'electron-log';

@Injectable()
export class PermissionsService {
    constructor(
        private ngxPermissionsService: NgxPermissionsService,
        private sessionService: SessionService,
        private accountService: AccountService
    ) { }

    refresh() {
        try {
            return new Promise((resolve) => {
                log.info('Refreshing permissions');
                this.ngxPermissionsService.flushPermissions();
                this.ngxPermissionsService.addPermission(Permissions.UNLOGGED);
                if (this.sessionService.getToken()) {
                    // logged in
                    this.accountService.getAccount(account => {
                        this.ngxPermissionsService.removePermission(Permissions.UNLOGGED);
                        if (account.membershipState === MembershipState.MEMBER) {
                            // member
                            this.ngxPermissionsService.addPermission(Permissions.MEMBER);
                            if (account.sr1) {
                                this.ngxPermissionsService.addPermission(Permissions.SR1);
                            }
                            if (account.sr10) {
                                this.ngxPermissionsService.addPermission(Permissions.SR10);
                            }
                            if (account.sr1Lead) {
                                this.ngxPermissionsService.addPermission(Permissions.SR1_LEAD);
                            }
                            if (account.command) {
                                this.ngxPermissionsService.addPermission(Permissions.COMMAND);
                            }
                            if (account.isNco) {
                                this.ngxPermissionsService.addPermission(Permissions.NCO);
                            }
                            if (account.admin) {
                                this.ngxPermissionsService.addPermission(Permissions.ADMIN);
                            }
                        } else if (account.membershipState === MembershipState.CONFIRMED) {
                            // guest
                            this.ngxPermissionsService.addPermission(Permissions.CONFIRMED);
                        } else {
                            // unconfirmed, any else
                            this.ngxPermissionsService.addPermission(Permissions.UNCONFIRMED);
                        }
                        log.debug(`New permissions: ${JSON.stringify(this.ngxPermissionsService.getPermissions())}`);
                        resolve();
                    });
                } else {
                    // not logged in
                    log.info('User is not logged in');
                    resolve();
                }
            });
        } catch (error) {
            log.error(error);
        }
    }
}
