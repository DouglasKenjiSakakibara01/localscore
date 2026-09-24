import {
  HttpClient,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';
import { TokenStorage } from './token-storage.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let storage: TokenStorage;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            refresh: () => of(null),
            handleAuthenticationFailure: () => undefined,
          },
        },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    storage = TestBed.inject(TokenStorage);
    storage.clear();
  });

  afterEach(() => {
    storage.clear();
    httpTesting.verify();
  });

  it('adds the current bearer token', () => {
    storage.set('access-token');

    http.get('/api/v1/protected').subscribe();
    const request = httpTesting.expectOne('/api/v1/protected');

    expect(request.request.headers.get('Authorization')).toBe(
      'Bearer access-token',
    );

    request.flush({});
  });

  it('does not add an authorization header without an access token', () => {
    http.get('/api/v1/public').subscribe();
    const request = httpTesting.expectOne('/api/v1/public');

    expect(request.request.headers.has('Authorization')).toBeFalse();

    request.flush({});
  });
});
