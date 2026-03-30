import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthStateService } from '../auth/auth-state.service';
import { apiConfig } from '../config/api.config';

export const authTokenInterceptor: HttpInterceptorFn = (request, next) => {
  const authState = inject(AuthStateService);
  const accessToken = authState.accessToken();

  if (!accessToken || !request.url.startsWith(apiConfig.baseUrl)) {
    return next(request);
  }

  return next(request.clone({
    setHeaders: {
      Authorization: `Bearer ${accessToken}`
    }
  }));
};
