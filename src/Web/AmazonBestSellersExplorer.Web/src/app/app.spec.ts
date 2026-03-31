import { provideZonelessChangeDetection, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AppComponent } from './app';
import { AuthStateService } from './core/auth/auth-state.service';

describe('AppComponent', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  async function createComponent(isAuthenticated = false, username: string | null = null) {
    const authState = {
      isAuthenticated: signal(isAuthenticated),
      username: signal<string | null>(username),
      clearToken: jasmine.createSpy('clearToken')
    };

    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        {
          provide: AuthStateService,
          useValue: authState as Pick<AuthStateService, 'isAuthenticated' | 'username' | 'clearToken'>
        }
      ]
    }).compileComponents();

    const fixture = TestBed.createComponent(AppComponent);
    const router = TestBed.inject(Router);
    spyOn(router, 'navigateByUrl').and.resolveTo(true);

    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    return { fixture, authState, router };
  }

  it('renders anonymous navigation when user is signed out', async () => {
    const { fixture } = await createComponent();
    const compiled = fixture.nativeElement as HTMLElement;
    const navigationLinks = Array.from(compiled.querySelectorAll('.shell-nav a')).map(link => link.textContent?.trim());

    expect(navigationLinks).toContain('Bestsellers');
    expect(navigationLinks).toContain('Sign in');
    expect(navigationLinks).toContain('Create account');
    expect(navigationLinks).not.toContain('Favorites');
    expect(compiled.textContent).toContain('Guest');
  });

  it('renders authenticated shell details when user is signed in', async () => {
    const { fixture } = await createComponent(true, 'marcin123');
    const compiled = fixture.nativeElement as HTMLElement;
    const navigationLinks = Array.from(compiled.querySelectorAll('.shell-nav a')).map(link => link.textContent?.trim());

    expect(navigationLinks).toContain('Bestsellers');
    expect(navigationLinks).toContain('Favorites');
    expect(compiled.textContent).toContain('Logout');
    expect(compiled.textContent).toContain('Signed in');
    expect(compiled.textContent).toContain('marcin123');
  });

  it('clears auth state and navigates to bestsellers on logout', async () => {
    const { fixture, authState, router } = await createComponent(true, 'marcin123');
    const logoutButton = fixture.nativeElement.querySelector('.nav-action') as HTMLButtonElement;

    logoutButton.click();
    await fixture.whenStable();

    expect(authState.clearToken).toHaveBeenCalled();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/bestsellers');
  });
});
