import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpErrorResponse, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AuthorizationService } from './authorization.service';
import * as log from 'electron-log';
import { Router } from '@angular/router';

@Injectable()
export class HttpInterceptorService implements HttpInterceptor {
    constructor(private authorizationService: AuthorizationService, private router: Router) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(tap(() => { }, error => {
            if (error instanceof HttpErrorResponse) {
                if (error.status === 401 || error.status === 403) {
                    log.warn('Unauthorized, logging out');
                    this.authorizationService.logout();
                    this.router.navigate(['login']);
                }
            }
        }));
    }
}
