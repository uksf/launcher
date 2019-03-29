import { Component } from '@angular/core';
import * as log from 'electron-log';
import * as settings from 'electron-settings';
import { AuthorizationService } from '../../services/authorization.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.scss']
})
export class HomeComponent {

    constructor(private authorizationService: AuthorizationService, private router: Router) {
        if (!settings.get('setupDone')) {
            this.router.navigate(['setup']);
            return;
        }
        log.debug('Home');
    }

    logout() {
        this.authorizationService.logout();
        this.router.navigate(['login']);
    }

    refresh() {
        // refresh
    }
}

