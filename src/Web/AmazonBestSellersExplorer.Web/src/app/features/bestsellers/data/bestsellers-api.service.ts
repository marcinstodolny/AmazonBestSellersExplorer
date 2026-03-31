import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from '../../../core/http/api-client.service';
import { BestsellerProduct } from '../../../shared/models/bestseller-product.model';

@Injectable({
  providedIn: 'root'
})
export class BestsellersApiService {
  private readonly apiClient = inject(ApiClientService);

  getSoftwareBestSellers(): Observable<BestsellerProduct[]> {
    return this.apiClient.get<BestsellerProduct[]>('bestsellers');
  }
}
