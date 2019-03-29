import { Component } from '@angular/core';
import * as log from 'electron-log';
import * as settings from 'electron-settings';
import { Router } from '@angular/router';
import { ElectronService } from '../../services/electron.service';

@Component({
    selector: 'app-setup',
    templateUrl: './setup.component.html',
    styleUrls: ['./setup.component.scss']
})
export class SetupComponent {
    step = 0;
    blocked = true;

    constructor(private router: Router, private electronService: ElectronService) {
        if (settings.get('setupDone')) {
            this.router.navigate(['home']);
            return;
        }
        log.debug('Setup');
    }

    get heading() {
        switch (this.step) {
            case 1: return 'Mods Location';
            case 2: return 'Game Profile';
            default: return 'Game Executable';
        }
    }

    blocking(state) {
        this.blocked = state;
    }

    back() {
        this.step--;
    }

    cancel() {
        this.electronService.remote.app.quit();
    }

    next() {
        this.step++;
    }

    finish() {

    }
}

