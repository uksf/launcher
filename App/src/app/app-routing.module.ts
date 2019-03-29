import { Routes, RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { NgxPermissionsGuard } from 'ngx-permissions';
import { Permissions } from './services/permissions';
import { HomeComponent } from './pages/home/home.component';
import { SetupComponent } from './pages/setup/setup.component';
import { SettingsComponent } from './pages/settings/settings.component';
import { LoginComponent } from './pages/login/login.component';

const routes: Routes = [
    { path: '', redirectTo: 'setup', pathMatch: 'full' },
    {
        path: 'setup', component: SetupComponent, data: {
            permissions: {
                except: Permissions.UNLOGGED,
                redirectTo: 'login'
            }
        }, canActivate: [NgxPermissionsGuard]
    },
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
