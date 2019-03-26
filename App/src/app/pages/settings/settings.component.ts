import { Component } from '@angular/core';
import * as log from 'electron-log';

@Component({
    selector: 'app-settings',
    templateUrl: './settings.component.html',
    styleUrls: ['./settings.component.scss']
})
export class SettingsComponent {
    constructor() {
        log.debug('Settings');
    }
}
