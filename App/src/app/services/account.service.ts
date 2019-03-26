import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppConfig } from '../../environments/environment';

@Injectable()
export class AccountService {
    public account;

    constructor(private httpClient: HttpClient) { }

    public getAccount(callback: (any) => void = null, callbackError: () => void = null) {
        const subscribable = this.httpClient.get(AppConfig.apiUrl + '/accounts');
        subscribable.subscribe((response: any) => {
            const account = response;
            this.account = account;
            if (callback) {
                callback(account);
            }
        }, _ => {
            this.clear();
            if (callbackError) {
                callbackError();
            }
        });
        return subscribable;
    }

    public clear() {
        this.account = undefined;
    }
}

export enum MembershipState {
    UNCONFIRMED,
    CONFIRMED,
    MEMBER,
    DISCHARGED
}

export enum ApplicationState {
    ACCEPTED,
    REJECTED,
    WAITING
}
