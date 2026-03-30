import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from '../../../core/http/api-client.service';
import { FavoriteProduct } from '../models/favorite-product.model';

@Injectable({
  providedIn: 'root'
})
export class FavoritesApiService {
  private readonly apiClient = inject(ApiClientService);

  getFavorites(): Observable<FavoriteProduct[]> {
    return this.apiClient.get<FavoriteProduct[]>('favorites');
  }

  removeFavorite(amazonProductId: string): Observable<void> {
    return this.apiClient.delete<void>(`favorites/${encodeURIComponent(amazonProductId)}`);
  }
}
