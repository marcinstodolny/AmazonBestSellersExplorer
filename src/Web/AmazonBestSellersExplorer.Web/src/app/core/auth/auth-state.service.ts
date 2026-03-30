import { Injectable, computed, signal } from '@angular/core';
import { AuthSession } from '../../shared/models/auth-session.model';

const authSessionStorageKey = 'amazon-best-sellers-explorer.auth-session';

@Injectable({
  providedIn: 'root'
})
export class AuthStateService {
  private readonly sessionState = signal<AuthSession | null>(this.readSession());

  readonly session = this.sessionState.asReadonly();
  readonly accessToken = computed(() => this.sessionState()?.accessToken ?? null);
  readonly isAuthenticated = computed(() => this.accessToken() !== null);

  setToken(accessToken: string): void {
    const normalizedAccessToken = accessToken.trim();

    if (!normalizedAccessToken) {
      this.clearToken();
      return;
    }

    const session: AuthSession = { accessToken: normalizedAccessToken };

    this.sessionState.set(session);
    localStorage.setItem(authSessionStorageKey, JSON.stringify(session));
  }

  clearToken(): void {
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

      return typeof parsedSession.accessToken === 'string' && parsedSession.accessToken.trim()
        ? { accessToken: parsedSession.accessToken }
        : null;
    } catch {
      localStorage.removeItem(authSessionStorageKey);
      return null;
    }
  }
}
