import { Component } from '@angular/core';
import * as log from 'electron-log';
import { AuthorizationService } from '../../services/authorization.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-settings',
    templateUrl: './settings.component.html',
    styleUrls: ['./settings.component.scss']
})
export class SettingsComponent {
    constructor(private authorizationService: AuthorizationService, private router: Router) {
        log.debug('Settings');
    }
}
