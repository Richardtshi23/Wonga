import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../src/app/services/auth.service'; 
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  constructor(private auth: AuthService, private router: Router) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.auth.getAccessToken();
    let cloned = req.clone({ withCredentials: true });

    if (token) {
      cloned = cloned.clone({
        setHeaders: { Authorization: `Bearer ${token}` },
        withCredentials: true
      });
    }

    return next.handle(cloned).pipe(
      catchError(err => {
        if (err instanceof HttpErrorResponse && err.status === 401) {
          if (!this.isRefreshing) {
            this.isRefreshing = true;
            return this.auth.refreshAccessToken().pipe(
              switchMap(() => {
                this.isRefreshing = false;
                const newToken = this.auth.getAccessToken();
                const retry = req.clone({
                  setHeaders: newToken ? { Authorization: `Bearer ${newToken}` } : {},
                  withCredentials: true
                });
                return next.handle(retry);
              }),
              catchError(e => {
                this.isRefreshing = false;
                this.auth.logout().subscribe({ next: () => this.router.navigate(['/login']) });
                return throwError(() => e);
              })
            );
          }
        }
        return throwError(() => err);
      })
    );
  }
}
