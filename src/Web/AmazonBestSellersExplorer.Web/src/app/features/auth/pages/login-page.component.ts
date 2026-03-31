import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, NonNullableFormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthStateService } from '../../../core/auth/auth-state.service';
import { AuthApiService } from '../data/auth-api.service';
import { LoginUserRequest } from '../models/login-user-request.model';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="auth-page">
      <header class="auth-header">
        <span class="auth-kicker">Account</span>
        <h1>Sign in</h1>
        <p>Sign in to access your saved products and keep your favorites in sync.</p>
      </header>

      <form class="auth-form" [formGroup]="form" (ngSubmit)="submit()">
        <label class="field">
          <span>Username</span>
          <input type="text" formControlName="username" autocomplete="username" />
          @if (showFieldError('username')) {
            <small>{{ usernameError() }}</small>
          }
        </label>

        <label class="field">
          <span>Password</span>
          <input type="password" formControlName="password" autocomplete="current-password" />
          @if (showFieldError('password')) {
            <small>{{ passwordError() }}</small>
          }
        </label>

        @if (errorMessages().length > 0) {
          <section class="error-panel" aria-live="polite">
            <h2>Sign-in failed</h2>
            <ul>
              @for (message of errorMessages(); track message) {
                <li>{{ message }}</li>
              }
            </ul>
          </section>
        }

        <div class="actions">
          <button type="submit" [disabled]="isSubmitting()">
            {{ isSubmitting() ? 'Signing in...' : 'Sign in' }}
          </button>

          <a routerLink="/register">Need an account? Create one</a>
        </div>
      </form>
    </section>
  `,
  styles: `
    .auth-page {
      display: grid;
      gap: 1.5rem;
      max-width: 36rem;
    }

    .auth-header {
      display: grid;
      gap: 0.75rem;
    }

    .auth-kicker {
      text-transform: uppercase;
      letter-spacing: 0.12em;
      font-size: 0.78rem;
      font-weight: 700;
      color: #135e46;
    }

    h1, h2, p {
      margin: 0;
    }

    h1 {
      font-size: clamp(2rem, 3vw, 2.8rem);
      line-height: 1;
    }

    p {
      color: #4c5b67;
      line-height: 1.6;
    }

    .auth-form {
      display: grid;
      gap: 1rem;
      padding: 1.5rem;
      border-radius: 1.25rem;
      background: rgba(255, 255, 255, 0.88);
      border: 1px solid rgba(19, 32, 40, 0.08);
    }

    .field {
      display: grid;
      gap: 0.45rem;
    }

    .field span {
      font-weight: 700;
    }

    .field input {
      border: 1px solid rgba(19, 32, 40, 0.15);
      border-radius: 0.85rem;
      padding: 0.9rem 1rem;
      background: #ffffff;
      color: #132028;
      caret-color: #132028;
      -webkit-text-fill-color: #132028;
    }

    .field input::placeholder {
      color: #7b8790;
    }

    .field input:-webkit-autofill,
    .field input:-webkit-autofill:hover,
    .field input:-webkit-autofill:focus {
      -webkit-text-fill-color: #132028;
    }

    .field small {
      color: #b42318;
      font-size: 0.85rem;
      font-weight: 600;
    }

    .error-panel {
      display: grid;
      gap: 0.65rem;
      padding: 1rem 1.1rem;
      border-radius: 1rem;
      background: rgba(255, 240, 243, 0.92);
      border: 1px solid rgba(170, 36, 59, 0.18);
    }

    .error-panel ul {
      margin: 0;
      padding-left: 1.2rem;
      color: #7a1f2f;
    }

    .actions {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 1rem;
      justify-content: space-between;
      margin-top: 0.5rem;
    }

    button {
      border: 0;
      border-radius: 999px;
      padding: 0.9rem 1.15rem;
      background: #15308d;
      color: #ffffff;
      font-weight: 700;
      cursor: pointer;
    }

    button:disabled {
      opacity: 0.7;
      cursor: wait;
    }

    a {
      color: #15308d;
      font-weight: 700;
      text-decoration: none;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPageComponent {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly authState = inject(AuthStateService);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessages = signal<string[]>([]);

  protected readonly form = this.formBuilder.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  protected readonly usernameError = computed(() => {
    const control = this.form.controls.username;

    if (control.hasError('required')) {
      return 'Username is required.';
    }

    return '';
  });

  protected readonly passwordError = computed(() => {
    const control = this.form.controls.password;

    if (control.hasError('required')) {
      return 'Password is required.';
    }

    return '';
  });

  protected submit(): void {
    this.errorMessages.set([]);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const request: LoginUserRequest = this.form.getRawValue();

    this.authApi.login(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: response => {
          this.authState.setToken(response);
          const returnUrl = this.activatedRoute.snapshot.queryParamMap.get('returnUrl');
          const targetUrl = isSafeReturnUrl(returnUrl) ? returnUrl : '/favorites';
          void this.router.navigateByUrl(targetUrl);
        },
        error: error => {
          this.errorMessages.set(extractApiErrors(error, `We couldn't sign you in.`));
        }
      });
  }

  protected showFieldError(controlName: 'username' | 'password'): boolean {
    const control = this.form.controls[controlName];

    return control.invalid && (control.touched || control.dirty);
  }
}

function extractApiErrors(error: unknown, fallbackMessage: string): string[] {
  if (error instanceof HttpErrorResponse) {
    const payload = error.error as { errors?: unknown; detail?: unknown } | null;

    if (payload && Array.isArray(payload.errors)) {
      return payload.errors.filter((item): item is string => typeof item === 'string' && item.trim().length > 0);
    }

    if (payload && typeof payload.detail === 'string' && payload.detail.trim().length > 0) {
      return [payload.detail];
    }
  }

  return [fallbackMessage];
}

function isSafeReturnUrl(returnUrl: string | null): returnUrl is string {
  return typeof returnUrl === 'string'
    && returnUrl.startsWith('/')
    && !returnUrl.startsWith('//');
}
