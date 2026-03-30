import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from '../../../core/http/api-client.service';
import { AuthResponse } from '../models/auth-response.model';
import { LoginUserRequest } from '../models/login-user-request.model';
import { RegisterUserRequest } from '../models/register-user-request.model';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private readonly apiClient = inject(ApiClientService);

  register(request: RegisterUserRequest): Observable<AuthResponse> {
    return this.apiClient.post<RegisterUserRequest, AuthResponse>('auth/register', request);
  }

  login(request: LoginUserRequest): Observable<AuthResponse> {
    return this.apiClient.post<LoginUserRequest, AuthResponse>('auth/login', request);
  }
}
