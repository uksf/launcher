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

export function login(authorizationService: AuthorizationService) {
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
        LoginComponent
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
            useFactory: login,
            deps: [AuthorizationService],
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: HttpInterceptorService,
            multi: true
        },
        CredentialHelper
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
