import { Component, OnInit } from '@angular/core';
import { remote } from 'electron';
import { NgxPermissionsService } from 'ngx-permissions';
import { Permissions } from '../../services/permissions';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'app-header-bar',
    templateUrl: './header-bar.component.html',
    styleUrls: ['./header-bar.component.scss']
})
export class HeaderBarComponent implements OnInit {
    settingsState = false;

    constructor(private permissions: NgxPermissionsService, private router: Router) { }

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
        return grantedPermissions[Permissions.MEMBER] || grantedPermissions[Permissions.CONFIRMED];
    }

    settings() {
        this.settingsState = !this.settingsState;
        this.router.navigate([this.settingsState ? 'settings' : 'home']);
    }

    minimize() {
        remote.getCurrentWindow().minimize();
    }

    shut() {
        remote.app.quit();
    }
}
