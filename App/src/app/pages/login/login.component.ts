import { Component, HostListener } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material';
import { AuthorizationService } from '../../services/authorization.service';
import * as log from 'electron-log';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent {
    public form: FormGroup;
    loginError = '';
    submitted;

    constructor(
        public dialog: MatDialog,
        public formbuilder: FormBuilder,
        private authorizationService: AuthorizationService,
        private router: Router
    ) {
        this.form = formbuilder.group({
            name: ['', Validators.maxLength(0)],
            email: ['', Validators.required],
            password: ['', Validators.required]
        }, {});
        log.debug('Login');
    }

    submit() {
        // Honeypot field must be empty
        if (this.form.value.name !== '' || !this.form.valid) { return; }
        this.loginError = '';
        this.submitted = true;
        this.authorizationService.login({
            email: this.form.value.email, password: this.form.value.password
        }, response => {
            this.submitted = false;
            if (response === undefined) {
                this.router.navigate(['home']);
                return;
            }
            if (response.message) {
                this.loginError = response.message;
            }
        });
    }

    @HostListener('window:keyup', ['$event'])
    keyEvent(event: KeyboardEvent) {
        if (event.key === 'Enter') {
            this.submit();
        }
    }
}
