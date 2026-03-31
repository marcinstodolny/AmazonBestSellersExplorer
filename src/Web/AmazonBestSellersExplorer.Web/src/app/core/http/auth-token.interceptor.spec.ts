import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthStateService } from '../auth/auth-state.service';
import { apiConfig } from '../config/api.config';
import { authTokenInterceptor } from './auth-token.interceptor';

describe('authTokenInterceptor', () => {
  let httpClient: HttpClient;
  let httpTestingController: HttpTestingController;
  let accessTokenState: ReturnType<typeof signal<string | null>>;
  let revalidateSessionSpy: jasmine.Spy;

  beforeEach(() => {
    accessTokenState = signal<string | null>(null);
    revalidateSessionSpy = jasmine.createSpy('revalidateSession');

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(withInterceptors([authTokenInterceptor])),
        provideHttpClientTesting(),
        {
          provide: AuthStateService,
          useValue: {
            revalidateSession: revalidateSessionSpy,
            accessToken: accessTokenState
          } as unknown as Pick<AuthStateService, 'revalidateSession' | 'accessToken'>
        }
      ]
    });

    httpClient = TestBed.inject(HttpClient);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController?.verify();
    TestBed.resetTestingModule();
  });

  it('adds Authorization header for requests targeting the configured API base URL', () => {
    accessTokenState.set('jwt-token');

    httpClient.get(`${apiConfig.baseUrl}/favorites`).subscribe();

    const request = httpTestingController.expectOne(`${apiConfig.baseUrl}/favorites`);

    expect(revalidateSessionSpy).toHaveBeenCalled();
    expect(request.request.headers.get('Authorization')).toBe('Bearer jwt-token');

    request.flush([]);
  });

  it('does not add Authorization header when there is no token', () => {
    httpClient.get(`${apiConfig.baseUrl}/favorites`).subscribe();

    const request = httpTestingController.expectOne(`${apiConfig.baseUrl}/favorites`);

    expect(revalidateSessionSpy).toHaveBeenCalled();
    expect(request.request.headers.has('Authorization')).toBeFalse();

    request.flush([]);
  });

  it('does not modify non-api requests', () => {
    accessTokenState.set('jwt-token');

    httpClient.get('/assets/logo.svg', { responseType: 'text' }).subscribe();

    const request = httpTestingController.expectOne('/assets/logo.svg');

    expect(revalidateSessionSpy).toHaveBeenCalled();
    expect(request.request.headers.has('Authorization')).toBeFalse();

    request.flush('<svg></svg>');
  });
});
