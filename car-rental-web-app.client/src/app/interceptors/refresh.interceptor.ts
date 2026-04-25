import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take } from 'rxjs';

import { TokenService } from '../services/token.service';
import { AuthService } from '../services/auth.service';

// BehaviorSubject pentru a serializa cererile de refresh
// (evităm mai multe cereri de refresh simultane)
let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

/**
 * Interceptează răspunsurile 401 și încearcă să reîmprospăteze token-ul.
 * Dacă refresh-ul eșuează, redirecționează la /login.
 * Cererile care primesc 401 în timp ce se face refresh sunt puse în așteptare
 * și reluate după ce noul token este obținut.
 */
export const refreshInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const authService  = inject(AuthService);
  const router       = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Tratăm doar 401 și numai pentru request-uri API proprii
      // Excludem endpoint-urile de autentificare (ar crea buclă infinită)
      const isAuthEndpoint = req.url.includes('/auth/login') ||
                             req.url.includes('/auth/register') ||
                             req.url.includes('/auth/refresh');

      if (error.status !== 401 || isAuthEndpoint) {
        return throwError(() => error);
      }

      if (isRefreshing) {
        // Altă cerere face deja refresh — așteptăm să se termine
        return refreshTokenSubject.pipe(
          filter(token => token !== null),
          take(1),
          switchMap(token => {
            return next(req.clone({
              setHeaders: { Authorization: `Bearer ${token}` }
            }));
          })
        );
      }

      // Pornimn procesul de refresh
      isRefreshing = true;
      refreshTokenSubject.next(null);

      return authService.refreshToken().pipe(
        switchMap(res => {
          isRefreshing = false;
          refreshTokenSubject.next(res.accessToken);

          // Reluăm request-ul original cu noul token
          return next(req.clone({
            setHeaders: { Authorization: `Bearer ${res.accessToken}` }
          }));
        }),
        catchError(refreshError => {
          // Refresh-ul a eșuat — curățăm sesiunea și trimitem la login
          isRefreshing = false;
          tokenService.clearTokens();
          router.navigate(['/login'], {
            queryParams: { reason: 'session-expired' }
          });
          return throwError(() => refreshError);
        })
      );
    })
  );
};
