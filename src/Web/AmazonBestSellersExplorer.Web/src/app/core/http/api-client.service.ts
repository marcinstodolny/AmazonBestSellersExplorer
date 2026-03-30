import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiConfig } from '../config/api.config';

type RequestOptions = {
  headers?: HttpHeaders | Record<string, string | string[]>;
  params?: HttpParams | Record<string, string | number | boolean | readonly (string | number | boolean)[]>;
};

@Injectable({
  providedIn: 'root'
})
export class ApiClientService {
  private readonly httpClient = inject(HttpClient);

  get<TResponse>(path: string, options?: RequestOptions): Observable<TResponse> {
    return this.httpClient.get<TResponse>(this.buildUrl(path), options);
  }

  post<TRequest, TResponse>(path: string, body: TRequest, options?: RequestOptions): Observable<TResponse> {
    return this.httpClient.post<TResponse>(this.buildUrl(path), body, options);
  }

  delete<TResponse>(path: string, options?: RequestOptions): Observable<TResponse> {
    return this.httpClient.delete<TResponse>(this.buildUrl(path), options);
  }

  private buildUrl(path: string): string {
    const normalizedPath = path.replace(/^\/+/, '');

    return normalizedPath
      ? `${apiConfig.baseUrl}/${normalizedPath}`
      : apiConfig.baseUrl;
  }
}
