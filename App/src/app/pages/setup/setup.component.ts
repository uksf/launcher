import { Component } from '@angular/core';
import * as log from 'electron-log';
import * as settings from 'electron-settings';
import { Router } from '@angular/router';
import { ElectronService } from '../../services/electron.service';
import { MatDialog } from '@angular/material';
import { ConfirmationModalComponent } from '../../modals/confirmation-modal/confirmation-modal.component';

@Component({
    selector: 'app-setup',
    templateUrl: './setup.component.html',
    styleUrls: ['./setup.component.scss']
})
export class SetupComponent {
    step = 0;
    blocked = true;

    constructor(private router: Router, private electronService: ElectronService, private matDialog: MatDialog) {
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
        this.matDialog.open(ConfirmationModalComponent, {
            data: { message: `Are you sure you want to cancel the setup?\nIf yes, your progress will be saved` }
        }).componentInstance.confirmEvent.subscribe(() => {
            this.electronService.remote.app.quit();
        });
    }

    next() {
        this.step++;
    }

    finish() {

    }
}

