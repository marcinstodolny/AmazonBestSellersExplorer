import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { AuthStateService } from '../../../core/auth/auth-state.service';
import { AddFavoriteProductRequest } from '../models/add-favorite-product-request.model';
import { FavoriteProduct } from '../models/favorite-product.model';
import { FavoritesApiService } from './favorites-api.service';

@Injectable({
  providedIn: 'root'
})
export class FavoritesStateService {
  private readonly favoritesApi = inject(FavoritesApiService);
  private readonly authState = inject(AuthStateService);
  private loadRequestVersion = 0;

  private readonly favoritesState = signal<FavoriteProduct[]>([]);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);
  private readonly operationIdsState = signal<Set<string>>(new Set<string>());
  private readonly hasLoadedState = signal(false);

  readonly favorites = this.favoritesState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly hasLoaded = this.hasLoadedState.asReadonly();
  readonly favoriteIds = computed(() => new Set(this.favoritesState().map(product => product.amazonProductId)));
  readonly operationIds = this.operationIdsState.asReadonly();
  readonly hasPendingOperations = computed(() => this.operationIdsState().size > 0);

  constructor() {
    effect(() => {
      if (this.authState.session() === null) {
        this.reset();
        return;
      }

      this.loadFavorites();
    });
  }

  ensureLoaded(): void {
    if (this.authState.session() === null) {
      this.reset();
      return;
    }

    if (this.loadingState()) {
      return;
    }

    if (this.hasLoadedState() && this.errorState() === null) {
      return;
    }

    this.loadFavorites();
  }

  loadFavorites(): void {
    const sessionKey = this.getCurrentSessionKey();

    if (sessionKey === null) {
      this.reset();
      return;
    }

    const requestVersion = ++this.loadRequestVersion;

    this.loadingState.set(true);
    this.errorState.set(null);

    this.favoritesApi.getFavorites()
      .pipe(finalize(() => {
        if (this.canApplyLoadResult(requestVersion, sessionKey)) {
          this.loadingState.set(false);
        }
      }))
      .subscribe({
        next: favorites => {
          if (!this.canApplyLoadResult(requestVersion, sessionKey)) {
            return;
          }

          this.favoritesState.set(favorites);
          this.hasLoadedState.set(true);
        },
        error: () => {
          if (!this.canApplyLoadResult(requestVersion, sessionKey)) {
            return;
          }

          this.favoritesState.set([]);
          this.errorState.set('Backend did not return favorite products.');
          this.hasLoadedState.set(true);
        }
      });
  }

  addFavorite(request: AddFavoriteProductRequest): void {
    if (!this.authState.isAuthenticated() || this.isOperationInProgress(request.amazonProductId)) {
      return;
    }

    this.setFavoriteOperation(request.amazonProductId, true);
    this.errorState.set(null);

    this.favoritesApi.addFavorite(request)
      .pipe(finalize(() => this.setFavoriteOperation(request.amazonProductId, false)))
      .subscribe({
        next: () => {
          const nextFavorites = this.favoritesState().filter(product => product.amazonProductId !== request.amazonProductId);
          nextFavorites.unshift({
            amazonProductId: request.amazonProductId,
            title: request.title,
            price: request.price,
            rating: request.rating,
            productUrl: request.productUrl,
            imageUrl: request.imageUrl
          });

          this.favoritesState.set(nextFavorites);
          this.hasLoadedState.set(true);
        },
        error: () => {
          this.errorState.set('Unable to add favorite product.');
        }
      });
  }

  removeFavorite(amazonProductId: string): void {
    if (!this.authState.isAuthenticated() || this.isOperationInProgress(amazonProductId)) {
      return;
    }

    this.setFavoriteOperation(amazonProductId, true);
    this.errorState.set(null);

    this.favoritesApi.removeFavorite(amazonProductId)
      .pipe(finalize(() => this.setFavoriteOperation(amazonProductId, false)))
      .subscribe({
        next: () => {
          this.favoritesState.update(products =>
            products.filter(product => product.amazonProductId !== amazonProductId));
          this.hasLoadedState.set(true);
        },
        error: () => {
          this.errorState.set('Unable to remove favorite product.');
        }
      });
  }

  isFavorite(amazonProductId: string): boolean {
    return this.favoriteIds().has(amazonProductId);
  }

  isOperationInProgress(amazonProductId: string): boolean {
    return this.operationIdsState().has(amazonProductId);
  }

  private reset(): void {
    this.loadRequestVersion++;
    this.favoritesState.set([]);
    this.loadingState.set(false);
    this.errorState.set(null);
    this.operationIdsState.set(new Set<string>());
    this.hasLoadedState.set(false);
  }

  private canApplyLoadResult(requestVersion: number, sessionKey: string): boolean {
    return requestVersion === this.loadRequestVersion
      && this.getCurrentSessionKey() === sessionKey;
  }

  private getCurrentSessionKey(): string | null {
    const session = this.authState.session();

    return session === null
      ? null
      : `${session.accessToken}:${session.expiresAtUtc}`;
  }

  private setFavoriteOperation(amazonProductId: string, inProgress: boolean): void {
    const nextOperationIds = new Set(this.operationIdsState());

    if (inProgress) {
      nextOperationIds.add(amazonProductId);
    } else {
      nextOperationIds.delete(amazonProductId);
    }

    this.operationIdsState.set(nextOperationIds);
  }
}
