import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { guestOnlyGuard } from './core/auth/guest-only.guard';

export const appRoutes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'bestsellers' },
  {
    path: 'bestsellers',
    loadComponent: () =>
      import('./features/bestsellers/pages/bestsellers-page.component')
        .then(module => module.BestsellersPageComponent)
  },
  {
    path: 'login',
    canActivate: [guestOnlyGuard],
    loadComponent: () =>
      import('./features/auth/pages/login-page.component')
        .then(module => module.LoginPageComponent)
  },
  {
    path: 'register',
    canActivate: [guestOnlyGuard],
    loadComponent: () =>
      import('./features/auth/pages/register-page.component')
        .then(module => module.RegisterPageComponent)
  },
  {
    path: 'favorites',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/favorites/pages/favorites-page.component')
        .then(module => module.FavoritesPageComponent)
  },
  { path: '**', redirectTo: 'bestsellers' }
];
