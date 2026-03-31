import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStateService } from './auth-state.service';

export const guestOnlyGuard: CanActivateFn = () => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  authState.revalidateSession();

  return authState.isAuthenticated()
    ? router.createUrlTree(['/favorites'])
    : true;
};
