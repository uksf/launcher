import 'reflect-metadata';
import '../polyfills';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import {
    MatInputModule,
    MatDialogModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatNativeDateModule,
    MatAutocompleteModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule, MatStepperModule,
    MatDatepickerModule,
    MatExpansionModule,
    MatGridListModule,
    MatIconModule,
    MatListModule,
    MatMenuModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatRippleModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
    MatTabsModule,
    MatTooltipModule,
    MatToolbarModule
} from '@angular/material';
import { JwtModule } from '@auth0/angular-jwt';

import { AppRoutingModule } from './app-routing.module';
import { ElectronService } from './services/electron.service';
import { WebviewDirective } from './directives/webview.directive';
import { SessionService } from './services/session.service';
import { AuthorizationService } from './services/authorization.service';
import { AccountService } from './services/account.service';
import { PermissionsService } from './services/permissions.service';
import { NgxPermissionsModule } from 'ngx-permissions';
import { ThemeService } from './services/theme.service';
import { SignalRService } from './services/signalr.service';
import { HttpInterceptorService } from './services/httpinterceptor.service';

import { AppComponent } from './app.component';
import { FlexFillerComponent } from './components/flex-filler/flex-filler.component';
import { HeaderBarComponent } from './components/header-bar/header-bar.component';
import { HomeComponent } from './pages/home/home.component';
import { SettingsComponent } from './pages/settings/settings.component';
import { LoginComponent } from './pages/login/login.component';
import { CredentialHelper } from './services/library/credential-helper.service';

import * as log from 'electron-log';
import * as settings from 'electron-settings';
import { AppConfig } from '../environments/environment';
import { LibraryService } from './services/library/library.service';
import { SetupComponent } from './pages/setup/setup.component';
import { SetupExeComponent } from './components/setup/exe/setup-exe.component';
import { RegistryHelper } from './services/library/registry-helper.service';
import { ArmaExeService } from './services/armaexe.service';
import { FilesService } from './services/files.service';
import { ConfirmationModalComponent } from './modals/confirmation-modal/confirmation-modal.component';
import { SetupModsComponent } from './components/setup/mods/setup-mods.component';
import { ModsLocationService } from './services/modslocation.service';
import { FileHelper } from './services/library/file-helper.service';
import { SetupProfileComponent } from './components/setup/profile/setup-profile.component';

export function preInit(authorizationService: AuthorizationService) {
    log.info(`Reading settings from '${settings.file()}'`);
    if (Object.keys(settings.getAll()).length === 0) {
        log.info(`No settings found, setting defaults`);
        settings.setAll(AppConfig.defaults);
    }

    const settingsAll = settings.getAll();
    Object.keys(settings.getAll()).forEach((key) => {
        settings.watch(key, newValue => {
            log.info(`Setting '${key}' updated to '${JSON.stringify(newValue)}'`);
        });
    });
    Object.keys(AppConfig.defaults).forEach((key) => {
        if (!settingsAll.hasOwnProperty(key)) {
            settings.set(key, AppConfig.defaults[key]);
        }
    });
    log.info(`Settings read`);
    return () => authorizationService.startup();
}

export function tokenGetter() {
    return sessionStorage.getItem('access_token');
}

@NgModule({
    declarations: [
        AppComponent,
        WebviewDirective,
        FlexFillerComponent,
        HeaderBarComponent,
        HomeComponent,
        SettingsComponent,
        LoginComponent,
        SetupComponent,
        SetupExeComponent,
        SetupModsComponent,
        SetupProfileComponent,
        ConfirmationModalComponent
    ],
    imports: [
        JwtModule.forRoot({
            config: {
                tokenGetter: tokenGetter,
                whitelistedDomains: ['localhost:5000', 'api.uk-sf.co.uk']
            }
        }),
        NgxPermissionsModule.forRoot(),
        HttpClientModule,
        AppRoutingModule,
        CommonModule,
        BrowserModule,
        BrowserAnimationsModule,
        MatButtonModule,
        MatButtonToggleModule,
        FormsModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatNativeDateModule,
        MatAutocompleteModule,
        MatButtonModule,
        MatButtonToggleModule,
        MatCardModule,
        MatCheckboxModule,
        MatChipsModule,
        MatStepperModule,
        MatDatepickerModule,
        MatDialogModule,
        MatExpansionModule,
        MatGridListModule,
        MatIconModule,
        MatInputModule,
        MatListModule,
        MatMenuModule,
        MatPaginatorModule,
        MatProgressBarModule,
        MatProgressSpinnerModule,
        MatRadioModule,
        MatRippleModule,
        MatSelectModule,
        MatSidenavModule,
        MatSliderModule,
        MatSlideToggleModule,
        MatSnackBarModule,
        MatSortModule,
        MatTableModule,
        MatTabsModule,
        MatTooltipModule,
        MatToolbarModule
    ],
    providers: [
        ElectronService,
        AccountService,
        AuthorizationService,
        PermissionsService,
        SessionService,
        SignalRService,
        ThemeService,
        {
            provide: APP_INITIALIZER,
            useFactory: preInit,
            deps: [AuthorizationService],
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: HttpInterceptorService,
            multi: true
        },
        FilesService,
        ArmaExeService,
        ModsLocationService,

        LibraryService,
        CredentialHelper,
        RegistryHelper,
        FileHelper
    ],
    entryComponents: [
        ConfirmationModalComponent
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
