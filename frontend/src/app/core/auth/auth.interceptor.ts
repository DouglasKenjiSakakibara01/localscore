import {
  HttpContextToken,
  HttpErrorResponse,
  HttpInterceptorFn,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { TokenStorage } from './token-storage.service';

const authenticationRetry = new HttpContextToken<boolean>(() => false);

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const tokenStorage = inject(TokenStorage);
  const token = tokenStorage.token();

  const authenticatedRequest =
    token === null
      ? request
      : request.clone({
          setHeaders: { Authorization: `Bearer ${token}` },
        });

  return next(authenticatedRequest).pipe(
    catchError((error: unknown) => {
      if (
        !(error instanceof HttpErrorResponse) ||
        error.status !== 401 ||
        request.context.get(authenticationRetry) ||
        skipsAutomaticRefresh(request.url)
      ) {
        return throwError(() => error);
      }

      return authService.refresh().pipe(
        switchMap(() => {
          const renewedToken = tokenStorage.token();

          if (renewedToken === null) {
            authService.handleAuthenticationFailure();
            return throwError(() => error);
          }

          return next(
            request.clone({
              context: request.context.set(authenticationRetry, true),
              setHeaders: { Authorization: `Bearer ${renewedToken}` },
            }),
          );
        }),
        catchError((refreshError: unknown) => {
          authService.handleAuthenticationFailure();
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};

function skipsAutomaticRefresh(url: string): boolean {
  return [
    '/api/v1/auth/register',
    '/api/v1/auth/login',
    '/api/v1/auth/refresh',
    '/api/v1/auth/logout',
  ].some((endpoint) => url.endsWith(endpoint));
}
