import { Injectable, signal } from '@angular/core';
import { AuthSession } from '../../shared/models/auth-session.model';

const authSessionStorageKey = 'amazon-best-sellers-explorer.auth-session';

@Injectable({
  providedIn: 'root'
})
export class AuthStateService {
  private readonly sessionState = signal<AuthSession | null>(this.readSession());
  private expirationTimerId: ReturnType<typeof globalThis.setTimeout> | null = null;

  readonly session = (): AuthSession | null => this.getValidSession();
  readonly accessToken = (): string | null => this.getValidSession()?.accessToken ?? null;
  readonly isAuthenticated = (): boolean => this.getValidSession() !== null;

  setToken(session: AuthSession): void {
    const normalizedSession = this.normalizeSession(session);

    if (normalizedSession === null) {
      this.clearToken();
      return;
    }

    this.sessionState.set(normalizedSession);
    localStorage.setItem(authSessionStorageKey, JSON.stringify(normalizedSession));
    this.scheduleExpiration(normalizedSession);
  }

  clearToken(): void {
    this.clearExpirationTimer();
    this.sessionState.set(null);
    localStorage.removeItem(authSessionStorageKey);
  }

  private readSession(): AuthSession | null {
    const rawSession = localStorage.getItem(authSessionStorageKey);

    if (!rawSession) {
      return null;
    }

    try {
      const parsedSession = JSON.parse(rawSession) as Partial<AuthSession>;
      const normalizedSession = this.normalizeSession(parsedSession);

      if (normalizedSession === null) {
        localStorage.removeItem(authSessionStorageKey);
        return null;
      }

      this.scheduleExpiration(normalizedSession);
      return normalizedSession;
    } catch {
      localStorage.removeItem(authSessionStorageKey);
      return null;
    }
  }

  private normalizeSession(session: Partial<AuthSession>): AuthSession | null {
    if (typeof session.accessToken !== 'string' || !session.accessToken.trim()) {
      return null;
    }

    if (typeof session.expiresAtUtc !== 'string') {
      return null;
    }

    const expirationTime = Date.parse(session.expiresAtUtc);

    if (Number.isNaN(expirationTime) || expirationTime <= Date.now()) {
      return null;
    }

    return {
      accessToken: session.accessToken.trim(),
      expiresAtUtc: session.expiresAtUtc
    };
  }

  private isSessionValid(session: AuthSession): boolean {
    const expirationTime = Date.parse(session.expiresAtUtc);

    return !Number.isNaN(expirationTime) && expirationTime > Date.now();
  }

  private getValidSession(): AuthSession | null {
    const session = this.sessionState();

    if (session === null) {
      return null;
    }

    if (!this.isSessionValid(session)) {
      this.clearToken();
      return null;
    }

    return session;
  }

  private scheduleExpiration(session: AuthSession): void {
    this.clearExpirationTimer();

    const expirationDelay = Date.parse(session.expiresAtUtc) - Date.now();

    if (expirationDelay <= 0) {
      this.clearToken();
      return;
    }

    this.expirationTimerId = globalThis.setTimeout(() => {
      this.clearToken();
    }, expirationDelay);
  }

  private clearExpirationTimer(): void {
    if (this.expirationTimerId !== null) {
      clearTimeout(this.expirationTimerId);
      this.expirationTimerId = null;
    }
  }
}
