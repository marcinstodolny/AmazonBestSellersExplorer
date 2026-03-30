import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from '../../../core/http/api-client.service';
import { AddFavoriteProductRequest } from '../models/add-favorite-product-request.model';
import { FavoriteProduct } from '../models/favorite-product.model';

@Injectable({
  providedIn: 'root'
})
export class FavoritesApiService {
  private readonly apiClient = inject(ApiClientService);

  getFavorites(): Observable<FavoriteProduct[]> {
    return this.apiClient.get<FavoriteProduct[]>('favorites');
  }

  addFavorite(request: AddFavoriteProductRequest): Observable<void> {
    return this.apiClient.post<AddFavoriteProductRequest, void>('favorites', request);
  }

  removeFavorite(amazonProductId: string): Observable<void> {
    return this.apiClient.delete<void>(`favorites/${encodeURIComponent(amazonProductId)}`);
  }
}
