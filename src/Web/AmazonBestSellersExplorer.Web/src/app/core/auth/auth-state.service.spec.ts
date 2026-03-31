import { AUTH_SESSION_STORAGE_KEY, AuthStateService } from './auth-state.service';
import { AuthSession } from '../../shared/models/auth-session.model';

describe('AuthStateService', () => {
  beforeEach(() => {
    jasmine.clock().install();
    jasmine.clock().mockDate(new Date('2026-03-31T12:00:00.000Z'));
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
    jasmine.clock().uninstall();
  });

  it('loads a valid session from storage', () => {
    const session = createSession('stored-token');
    localStorage.setItem(AUTH_SESSION_STORAGE_KEY, JSON.stringify(session));

    const service = new AuthStateService();

    expect(service.isAuthenticated()).toBeTrue();
    expect(service.accessToken()).toBe('stored-token');
    expect(service.session()).toEqual(session);
  });

  it('removes an expired session from storage', () => {
    const expiredSession = createSession('expired-token', -1);
    localStorage.setItem(AUTH_SESSION_STORAGE_KEY, JSON.stringify(expiredSession));

    const service = new AuthStateService();

    expect(service.isAuthenticated()).toBeFalse();
    expect(service.session()).toBeNull();
    expect(localStorage.getItem(AUTH_SESSION_STORAGE_KEY)).toBeNull();
  });

  it('clearToken clears storage and state', () => {
    const service = new AuthStateService();
    service.setToken(createSession('active-token'));

    service.clearToken();

    expect(service.isAuthenticated()).toBeFalse();
    expect(service.accessToken()).toBeNull();
    expect(service.session()).toBeNull();
    expect(localStorage.getItem(AUTH_SESSION_STORAGE_KEY)).toBeNull();
  });

  it('setToken persists the normalized session to storage', () => {
    const service = new AuthStateService();
    const session = createSession('  active-token  ');

    service.setToken(session);

    expect(service.accessToken()).toBe('active-token');
    expect(localStorage.getItem(AUTH_SESSION_STORAGE_KEY)).toBe(JSON.stringify({
      accessToken: 'active-token',
      expiresAtUtc: session.expiresAtUtc
    }));
  });

  it('removes an invalid json payload from storage', () => {
    localStorage.setItem(AUTH_SESSION_STORAGE_KEY, '{not-valid-json');

    const service = new AuthStateService();

    expect(service.isAuthenticated()).toBeFalse();
    expect(service.session()).toBeNull();
    expect(localStorage.getItem(AUTH_SESSION_STORAGE_KEY)).toBeNull();
  });

  it('extracts username from unique_name claim', () => {
    const service = new AuthStateService();
    service.setToken(createSession(createJwt({ unique_name: 'marcin123' })));

    expect(service.username()).toBe('marcin123');
  });

  it('extracts username from name claim', () => {
    const service = new AuthStateService();
    service.setToken(createSession(createJwt({ name: 'marcin123' })));

    expect(service.username()).toBe('marcin123');
  });

  it('extracts username from legacy name claim', () => {
    const service = new AuthStateService();
    service.setToken(createSession(createJwt({
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'marcin123'
    })));

    expect(service.username()).toBe('marcin123');
  });

  function createSession(accessToken: string, minutesFromNow = 60): AuthSession {
    return {
      accessToken,
      expiresAtUtc: new Date(Date.now() + minutesFromNow * 60_000).toISOString()
    };
  }

  function createJwt(payload: Record<string, unknown>): string {
    return [
      encodeBase64Url({ alg: 'none', typ: 'JWT' }),
      encodeBase64Url(payload),
      'signature'
    ].join('.');
  }

  function encodeBase64Url(value: unknown): string {
    return btoa(JSON.stringify(value))
      .replace(/\+/g, '-')
      .replace(/\//g, '_')
      .replace(/=+$/g, '');
  }
});
