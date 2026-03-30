import { Routes } from '@angular/router';
import { BestsellersPageComponent } from './features/bestsellers/pages/bestsellers-page.component';
import { LoginPageComponent } from './features/auth/pages/login-page.component';
import { RegisterPageComponent } from './features/auth/pages/register-page.component';
import { FavoritesPageComponent } from './features/favorites/pages/favorites-page.component';

export const appRoutes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'bestsellers' },
  { path: 'bestsellers', component: BestsellersPageComponent },
  { path: 'login', component: LoginPageComponent },
  { path: 'register', component: RegisterPageComponent },
  { path: 'favorites', component: FavoritesPageComponent },
  { path: '**', redirectTo: 'bestsellers' }
];
