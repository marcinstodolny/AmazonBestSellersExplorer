import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
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
  private stateCycleVersion = 0;
  private favoritesRevision = 0;
  private activeSessionKey: string | null = null;

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
      const sessionKey = this.getCurrentSessionKey();

      if (sessionKey === null) {
        this.reset();
        return;
      }

      if (sessionKey !== this.activeSessionKey) {
        this.startNewSession(sessionKey);
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
    const favoritesRevision = this.favoritesRevision;

    this.loadingState.set(true);
    this.errorState.set(null);

    this.favoritesApi.getFavorites()
      .pipe(finalize(() => {
        if (this.canApplyLoadResult(requestVersion, sessionKey, favoritesRevision)) {
          this.loadingState.set(false);
        }
      }))
      .subscribe({
        next: favorites => {
          if (!this.canApplyLoadResult(requestVersion, sessionKey, favoritesRevision)) {
            return;
          }

          this.favoritesState.set(favorites);
          this.hasLoadedState.set(true);
        },
        error: () => {
          if (!this.canApplyLoadResult(requestVersion, sessionKey, favoritesRevision)) {
            return;
          }

          this.favoritesState.set([]);
          this.errorState.set(`We couldn't load your saved products right now.`);
          this.hasLoadedState.set(true);
        }
      });
  }

  addFavorite(request: AddFavoriteProductRequest): void {
    const sessionKey = this.getCurrentSessionKey();
    const stateCycleVersion = this.stateCycleVersion;

    if (sessionKey === null || this.isOperationInProgress(request.amazonProductId)) {
      return;
    }

    this.setFavoriteOperation(request.amazonProductId, true);
    this.errorState.set(null);

    this.favoritesApi.addFavorite(request)
      .pipe(finalize(() => {
        if (this.canApplyMutationResult(stateCycleVersion, sessionKey)) {
          this.setFavoriteOperation(request.amazonProductId, false);
        }
      }))
      .subscribe({
        next: () => {
          if (!this.canApplyMutationResult(stateCycleVersion, sessionKey)) {
            return;
          }

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
          this.favoritesRevision++;
          this.hasLoadedState.set(true);
        },
        error: error => {
          if (!this.canApplyMutationResult(stateCycleVersion, sessionKey)) {
            return;
          }

          this.errorState.set(extractFavoritesError(error, `We couldn't save this product to your favorites.`));
        }
      });
  }

  removeFavorite(amazonProductId: string): void {
    const sessionKey = this.getCurrentSessionKey();
    const stateCycleVersion = this.stateCycleVersion;

    if (sessionKey === null || this.isOperationInProgress(amazonProductId)) {
      return;
    }

    this.setFavoriteOperation(amazonProductId, true);
    this.errorState.set(null);

    this.favoritesApi.removeFavorite(amazonProductId)
      .pipe(finalize(() => {
        if (this.canApplyMutationResult(stateCycleVersion, sessionKey)) {
          this.setFavoriteOperation(amazonProductId, false);
        }
      }))
      .subscribe({
        next: () => {
          if (!this.canApplyMutationResult(stateCycleVersion, sessionKey)) {
            return;
          }

          this.favoritesState.update(products =>
            products.filter(product => product.amazonProductId !== amazonProductId));
          this.favoritesRevision++;
          this.hasLoadedState.set(true);
        },
        error: error => {
          if (!this.canApplyMutationResult(stateCycleVersion, sessionKey)) {
            return;
          }

          this.errorState.set(extractFavoritesError(error, `We couldn't remove this product from your favorites.`));
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
    this.activeSessionKey = null;
    this.loadRequestVersion++;
    this.stateCycleVersion++;
    this.favoritesRevision++;
    this.favoritesState.set([]);
    this.loadingState.set(false);
    this.errorState.set(null);
    this.operationIdsState.set(new Set<string>());
    this.hasLoadedState.set(false);
  }

  private startNewSession(sessionKey: string): void {
    this.activeSessionKey = sessionKey;
    this.loadRequestVersion++;
    this.stateCycleVersion++;
    this.favoritesRevision++;
    this.favoritesState.set([]);
    this.loadingState.set(false);
    this.errorState.set(null);
    this.operationIdsState.set(new Set<string>());
    this.hasLoadedState.set(false);
  }

  private canApplyLoadResult(requestVersion: number, sessionKey: string, favoritesRevision: number): boolean {
    return requestVersion === this.loadRequestVersion
      && favoritesRevision === this.favoritesRevision
      && this.getCurrentSessionKey() === sessionKey;
  }

  private canApplyMutationResult(stateCycleVersion: number, sessionKey: string): boolean {
    return stateCycleVersion === this.stateCycleVersion
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

function extractFavoritesError(error: unknown, fallbackMessage: string): string {
  if (error instanceof HttpErrorResponse) {
    const payload = error.error as { errors?: unknown; detail?: unknown } | null;

    if (payload && Array.isArray(payload.errors)) {
      const firstError = payload.errors.find((item): item is string =>
        typeof item === 'string' && item.trim().length > 0);

      if (firstError) {
        return firstError;
      }
    }

    if (payload && typeof payload.detail === 'string' && payload.detail.trim().length > 0) {
      return payload.detail;
    }
  }

  return fallbackMessage;
}
