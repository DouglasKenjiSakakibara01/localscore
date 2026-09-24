import { HttpClient } from '@angular/common/http';
import { computed, effect, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import {
  catchError,
  defer,
  finalize,
  firstValueFrom,
  Observable,
  shareReplay,
  tap,
  throwError,
} from 'rxjs';
import {
  AuthenticationResponse,
  AuthUser,
  LoginRequest,
  RegisterRequest,
} from './auth.models';
import { isTokenUsable } from './jwt-expiration';
import { RefreshCoordinator } from './refresh-coordinator.service';
import { TokenStorage } from './token-storage.service';

type AuthenticationStatus = 'initializing' | 'authenticated' | 'anonymous';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tokenStorage = inject(TokenStorage);
  private readonly refreshCoordinator = inject(RefreshCoordinator);

  private readonly userState = signal<AuthUser | null>(null);
  private readonly statusState = signal<AuthenticationStatus>('initializing');

  private initialization?: Promise<void>;
  private refreshRequest?: Observable<AuthenticationResponse | null>;
  private initialized = false;

  readonly user = this.userState.asReadonly();
  readonly status = this.statusState.asReadonly();
  readonly isAuthenticated = computed(
    () => this.statusState() === 'authenticated' && this.userState() !== null,
  );

  constructor() {
    effect(() => {
      const token = this.tokenStorage.token();

      if (this.initialized && token === null) {
        this.userState.set(null);
        this.statusState.set('anonymous');
      }
    });
  }

  initialize(): Promise<void> {
    this.initialization ??= this.initializeSession();
    return this.initialization;
  }

  register(request: RegisterRequest): Observable<AuthenticationResponse> {
    return this.http
      .post<AuthenticationResponse>('/api/v1/auth/register', request)
      .pipe(tap((response) => this.acceptSession(response)));
  }

  login(request: LoginRequest): Observable<AuthenticationResponse> {
    return this.http
      .post<AuthenticationResponse>('/api/v1/auth/login', request)
      .pipe(tap((response) => this.acceptSession(response)));
  }

  refresh(): Observable<AuthenticationResponse | null> {
    if (this.refreshRequest) {
      return this.refreshRequest;
    }

    const tokenBeforeRefresh = this.tokenStorage.token();

    this.refreshRequest = defer(() =>
      this.refreshCoordinator.runExclusive(
        () => {
          const currentToken = this.tokenStorage.token();
          return (
            currentToken !== null &&
            currentToken !== tokenBeforeRefresh &&
            isTokenUsable(currentToken)
          );
        },
        () =>
          firstValueFrom(
            this.http
              .post<AuthenticationResponse>('/api/v1/auth/refresh', {})
              .pipe(tap((response) => this.acceptSession(response))),
          ),
      ),
    ).pipe(
      catchError((error: unknown) => {
        this.clearSession();
        return throwError(() => error);
      }),
      finalize(() => {
        this.refreshRequest = undefined;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    return this.refreshRequest;
  }

  logout(): void {
    this.http
      .post<void>('/api/v1/auth/logout', {})
      .pipe(
        catchError(() => []),
        finalize(() => {
          this.clearSession();
          void this.router.navigateByUrl('/login');
        }),
      )
      .subscribe();
  }

  handleAuthenticationFailure(): void {
    this.clearSession();
    void this.router.navigateByUrl('/login');
  }

  private async initializeSession(): Promise<void> {
    try {
      const token = this.tokenStorage.token();

      if (token !== null && isTokenUsable(token)) {
        await firstValueFrom(this.loadCurrentUser());
      } else {
        const response = await firstValueFrom(this.refresh());

        if (response === null) {
          await firstValueFrom(this.loadCurrentUser());
        }
      }
    } catch {
      this.clearSession();
    } finally {
      this.initialized = true;

      if (this.statusState() === 'initializing') {
        this.statusState.set('anonymous');
      }
    }
  }

  private loadCurrentUser(): Observable<AuthUser> {
    return this.http.get<AuthUser>('/api/v1/auth/me').pipe(
      tap((user) => {
        this.userState.set(user);
        this.statusState.set('authenticated');
      }),
    );
  }

  private acceptSession(response: AuthenticationResponse): void {
    this.tokenStorage.set(response.accessToken);
    this.userState.set(response.user);
    this.statusState.set('authenticated');
  }

  private clearSession(): void {
    this.tokenStorage.clear();
    this.userState.set(null);
    this.statusState.set('anonymous');
  }
}
