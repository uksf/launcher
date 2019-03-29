import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SessionService } from './session.service';
import { PermissionsService } from './permissions.service';
import { AccountService } from './account.service';
import { AppConfig } from '../../environments/environment';
import * as settings from 'electron-settings';
import * as log from 'electron-log';
import { CredentialHelper } from './library/credential-helper.service';

@Injectable()
export class AuthorizationService {

    constructor(
        private httpClient: HttpClient,
        private sessionService: SessionService,
        private permissionService: PermissionsService,
        private accountService: AccountService,
        private credentialHelper: CredentialHelper
    ) { }

    async startup() {
        try {
            return new Promise((resolve, reject) => {
                log.info('Trying login');
                if (settings.has('login.email') && settings.has('login.password')) {
                    const email = settings.get('login.email');
                    this.credentialHelper.decrypt(settings.get('login.password'), (error, password: string) => {
                        const details = { email: email, password: password };
                        log.info('Found stored credentials');
                        this.login(details, (loginError) => {
                            if (loginError) {
                                reject();
                            } else {
                                resolve();
                            }
                        });
                    });
                } else {
                    log.info('No stored credentials');
                    reject();
                }
            }).catch(() => {
                this.sessionService.removeToken();
                this.permissionService.refresh();
            });
        } catch (error) {
            log.error(error);
        }
    }

    login(details: any, callback: (any?) => void = null) {
        log.info('Logging in');
        const request = this.httpClient.post(`${AppConfig.apiUrl}/login`, details);
        request.subscribe(response => {
            log.info('Logged in');
            this.sessionService.setToken(response);
            settings.set('login.email', details.email);
            this.credentialHelper.encrypt(details.password, (error, password: string) => {
                settings.set('login.password', password);
                this.permissionService.refresh().then(() => {
                    if (callback) {
                        callback();
                    }
                });
            });
        }, (error) => {
            log.error(`Failed to login`);
            if (callback) {
                callback(error.error);
            }
        });
        return request;
    }

    logout() {
        log.info('Logging out');
        settings.delete('login');
        this.sessionService.removeToken();
        this.accountService.clear();
        this.permissionService.refresh();
    }
}
