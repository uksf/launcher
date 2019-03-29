import { Component, OnInit } from '@angular/core';
import { NgxPermissionsService } from 'ngx-permissions';
import { Permissions } from '../../services/permissions';
import { Router } from '@angular/router';
import * as settings from 'electron-settings';
import { ElectronService } from '../../services/electron.service';

@Component({
    selector: 'app-header-bar',
    templateUrl: './header-bar.component.html',
    styleUrls: ['./header-bar.component.scss']
})
export class HeaderBarComponent implements OnInit {
    settingsState = false;

    constructor(private permissions: NgxPermissionsService, private router: Router, private electronService: ElectronService) { }

    ngOnInit(): void {
        if (window.location.hash === '#/settings') {
            this.settingsState = true;
            this.router.navigate(['settings']);
        } else if (window.location.hash === '#/home') {
            this.settingsState = false;
            this.router.navigate(['home']);
        }
    }

    canToggleSettings() {
        const grantedPermissions = this.permissions.getPermissions();
        return settings.get('setupDone') && (grantedPermissions[Permissions.MEMBER] || grantedPermissions[Permissions.CONFIRMED]);
    }

    settings() {
        this.settingsState = !this.settingsState;
        this.router.navigate([this.settingsState ? 'settings' : 'home']);
    }

    minimize() {
        this.electronService.remote.getCurrentWindow().minimize();
    }

    shut() {
        this.electronService.remote.app.quit();
    }
}
