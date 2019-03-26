import { Routes, ActivatedRouteSnapshot, RouterStateSnapshot, RouterModule } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { SettingsComponent } from './pages/settings/settings.component';
import { LoginComponent } from './pages/login/login.component';
import { NgxPermissionsGuard } from 'ngx-permissions';
import { Permissions } from './services/permissions';
import * as log from 'electron-log';
import { NgModule } from '@angular/core';

const routes: Routes = [
    { path: '', redirectTo: 'home', pathMatch: 'full' },
    {
        path: 'home', component: HomeComponent, data: {
            permissions: {
                except: Permissions.UNLOGGED,
                redirectTo: 'login'
            }
        }, canActivate: [NgxPermissionsGuard]
    },
    {
        path: 'settings', component: SettingsComponent, data: {
            permissions: {
                except: Permissions.UNLOGGED,
                redirectTo: 'login'
            }
        }, canActivate: [NgxPermissionsGuard]
    },
    {
        path: 'login', component: LoginComponent, data: {
            permissions: {
                only: Permissions.UNLOGGED,
                redirectTo: 'login'
            }
        }, canActivate: [NgxPermissionsGuard]
    },
    { path: '**', redirectTo: 'login' }
];

@NgModule({
    imports: [RouterModule.forRoot(routes, { useHash: true })],
    exports: [RouterModule]
})
export class AppRoutingModule { }

export function loginRedirect(rejectedPermissionName: string, routeSnapshot: ActivatedRouteSnapshot, routerSnapshot: RouterStateSnapshot) {
    log.info(rejectedPermissionName);
    log.info(routeSnapshot);
    log.info(routerSnapshot);
    return 'login';
}
