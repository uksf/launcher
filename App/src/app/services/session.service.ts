import { Injectable } from '@angular/core';

@Injectable()
export class SessionService {
    constructor() { }

    setToken(token) {
        sessionStorage.setItem('access_token', token);
    }

    getToken() {
        return sessionStorage.getItem('access_token');
    }

    removeToken() {
        sessionStorage.removeItem('access_token');
    }
}
