import { Component, OnInit } from '@angular/core';
import { app, dialog, remote } from 'electron';
import { ThemeService } from './services/theme.service';
import * as log from 'electron-log';
import * as settings from 'electron-settings';
import { AppConfig } from '../environments/environment';

@Component({
    // tslint:disable-next-line:component-selector
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
    displayState = 0;
    isDev = true;

    constructor(private themeService: ThemeService) {
        this.initUpdate();
    }

    ngOnInit() {
        this.checkForUpdate();
    }

    initUpdate() {
        const isEnvSet = 'ELECTRON_IS_DEV' in process.env;
        const getFromEnv = parseInt(process.env.ELECTRON_IS_DEV, 10) === 1;
        this.isDev = isEnvSet ? getFromEnv : !(app || remote.app).isPackaged;

        remote.autoUpdater.setFeedURL({
            url: `${AppConfig.apiUrl}/launcher/update/${process.platform}/${remote.app.getVersion()}`
        });
        remote.autoUpdater.on('update-downloaded', (info) => {
            log.info(`Update available`);
            log.info(info);
            if (settings.get('autoUpdate', {}) === true) {
                this.update();
            }
        });
        remote.autoUpdater.on('error', message => {
            log.error('There was a problem updating the application');
            log.error(message);
        });
    }

    checkForUpdate() {
        if (!this.isDev) {
            log.info(`Checking for update - Current version:`);
            remote.autoUpdater.checkForUpdates();
        }
    }

    update() {
        const dialogOpts = {
            type: 'info',
            buttons: ['Restart', 'Later'],
            title: 'Application Update',
            message: process.platform === 'win32' ? 'Test' : 'Another test',
            detail: 'A new version has been downloaded. Restart the application to apply the updates.'
        };

        dialog.showMessageBox(dialogOpts, (response) => {
            if (response === 0) {
                remote.autoUpdater.quitAndInstall();
            }
        });
    }

    get Theme() {
        return this.themeService.theme;
    }
}
