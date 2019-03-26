import { Component } from '@angular/core';
import * as log from 'electron-log';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.scss']
})
export class HomeComponent {

    constructor() {
        log.debug('Home');
    }
}

