import { Routes } from '@angular/router';

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
    loadComponent: () =>
      import('./features/auth/pages/login-page.component')
        .then(module => module.LoginPageComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/pages/register-page.component')
        .then(module => module.RegisterPageComponent)
  },
  {
    path: 'favorites',
    loadComponent: () =>
      import('./features/favorites/pages/favorites-page.component')
        .then(module => module.FavoritesPageComponent)
  },
  { path: '**', redirectTo: 'bestsellers' }
];
