import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { DataViewModule } from 'primeng/dataview';
import { AuthStateService } from '../../../core/auth/auth-state.service';
import { BestsellersApiService } from '../data/bestsellers-api.service';
import { BestsellerProduct } from '../../../shared/models/bestseller-product.model';
import { ProductCardComponent } from '../../../shared/components/product-card/product-card.component';
import { FavoritesStateService } from '../../favorites/data/favorites-state.service';

type BestsellersViewState = 'favorites-loading' | 'favorites-error' | 'loading' | 'unavailable' | 'error' | 'empty' | 'success';

@Component({
  selector: 'app-bestsellers-page',
  standalone: true,
  imports: [DataViewModule, ProductCardComponent],
  template: `
    <section class="page-shell">
      <header class="page-header">
        <div>
          <span class="page-kicker">Software bestsellers</span>
          <h1>Amazon Software Bestsellers</h1>
        </div>

        <button type="button" class="refresh-button" (click)="reload()" [disabled]="isLoading()">
          {{ isLoading() ? 'Refreshing...' : 'Refresh list' }}
        </button>
      </header>

      @if (isFavoritesLoading()) {
        <section class="state-card">
          <h2>Loading your favorites</h2>
          <p>Checking your saved products before showing the bestseller list.</p>
        </section>
      } @else if (hasFavoritesError()) {
        <section class="state-card is-error">
          <h2>Couldn't load your favorites</h2>
          <p>{{ favoriteLoadErrorMessage() }}</p>
          <button type="button" class="refresh-button" (click)="retryFavorites()" [disabled]="isFavoritesRetrying()">
            {{ isFavoritesRetrying() ? 'Retrying...' : 'Retry favorites' }}
          </button>
        </section>
      } @else if (isLoading()) {
        <section class="state-card">
          <h2>Loading software bestsellers</h2>
          <p>Fetching the latest Amazon Poland software bestsellers.</p>
        </section>
      } @else if (isUnavailable()) {
        <section class="state-card is-error">
          <h2>Bestsellers are currently unavailable.</h2>
          <p>{{ errorMessage() }}</p>
        </section>
      } @else if (hasError()) {
        <section class="state-card is-error">
          <h2>Couldn't load the bestseller list</h2>
          <p>{{ errorMessage() }}</p>
        </section>
      } @else if (isEmpty()) {
        <section class="state-card">
          <h2>No products available right now</h2>
          <p>Try refreshing again in a moment.</p>
        </section>
      } @else {
        @if (showFavoriteError()) {
          <section class="inline-error-panel" aria-live="polite">
            <p>{{ favoriteError() }}</p>
          </section>
        }

        <p-dataview class="catalog-view" [value]="products()" layout="list">
          <ng-template #list let-items>
            <div class="products-grid">
              @for (product of items; track product.amazonProductId) {
                <app-product-card
                  [product]="product"
                  [isFavorite]="isFavorite(product.amazonProductId)"
                  [showFavoriteAction]="isAuthenticated()"
                  [isFavoriteActionLoading]="isFavoriteOperationInProgress(product.amazonProductId)"
                  (addFavorite)="addFavorite($event)"
                  (removeFavorite)="removeFavorite($event)"
                />
              }
            </div>
          </ng-template>
        </p-dataview>
      }
    </section>
  `,
  styles: `
    .page-shell {
      display: grid;
      gap: 1.5rem;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: end;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .page-kicker {
      display: inline-block;
      margin-bottom: 0.5rem;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      font-size: 0.78rem;
      font-weight: 700;
      color: #8f4a11;
    }

    h1,
    h2 {
      margin: 0;
      font-size: clamp(2rem, 3vw, 2.75rem);
      line-height: 1;
    }

    .refresh-button {
      border: 0;
      border-radius: 999px;
      padding: 0.85rem 1.15rem;
      background: #15308d;
      color: #ffffff;
      font-weight: 700;
      cursor: pointer;
    }

    .refresh-button:disabled {
      cursor: wait;
      opacity: 0.7;
    }

    .state-card {
      display: grid;
      gap: 0.75rem;
      padding: 1.5rem;
      border-radius: 1.25rem;
      background: rgba(255, 255, 255, 0.78);
      border: 1px solid rgba(19, 32, 40, 0.08);
    }

    .state-card.is-error {
      border-color: rgba(170, 36, 59, 0.18);
      background: rgba(255, 240, 243, 0.92);
    }

    .inline-error-panel {
      padding: 0.9rem 1rem;
      border-radius: 1rem;
      background: rgba(255, 240, 243, 0.92);
      border: 1px solid rgba(170, 36, 59, 0.18);
    }

    p {
      margin: 0;
      color: #3c4b57;
      font-size: 1rem;
      line-height: 1.6;
    }

    code {
      padding: 0.12rem 0.4rem;
      border-radius: 0.4rem;
      background: rgba(19, 32, 40, 0.08);
    }

    .products-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: 1rem;
      align-items: start;
    }

    :host ::ng-deep .catalog-view,
    :host ::ng-deep .catalog-view .p-dataview,
    :host ::ng-deep .catalog-view .p-dataview-content {
      background: transparent;
      border: 0;
      box-shadow: none;
    }

    :host ::ng-deep .catalog-view .p-dataview-content {
      padding: 0;
      border-radius: 0;
    }

  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BestsellersPageComponent {
  private readonly bestsellersApi = inject(BestsellersApiService);
  private readonly favoritesState = inject(FavoritesStateService);
  private readonly authState = inject(AuthStateService);
  private lastLoadContext: 'anonymous' | 'authenticated' | null = null;

  protected readonly state = signal<BestsellersViewState>('loading');
  protected readonly products = signal<BestsellerProduct[]>([]);
  protected readonly errorMessage = signal(`We couldn't load the bestseller list right now.`);

  protected readonly isFavoritesLoading = computed(() => this.state() === 'favorites-loading');
  protected readonly hasFavoritesError = computed(() => this.state() === 'favorites-error');
  protected readonly isFavoritesRetrying = this.favoritesState.loading;
  protected readonly isLoading = computed(() => this.state() === 'loading');
  protected readonly isUnavailable = computed(() => this.state() === 'unavailable');
  protected readonly hasError = computed(() => this.state() === 'error');
  protected readonly isEmpty = computed(() => this.state() === 'empty');
  protected readonly isAuthenticated = this.authState.isAuthenticated;
  protected readonly favoriteLoadErrorMessage = computed(() =>
    this.favoritesState.loadError() ?? `We couldn't load your favorites right now.`);
  protected readonly favoriteError = computed(() =>
    this.isAuthenticated() ? this.favoritesState.error() : null);
  protected readonly showFavoriteError = computed(() =>
    !this.isLoading() && !this.hasError() && this.favoriteError() !== null);

  constructor() {
    effect(() => {
      this.syncInitialLoad();
    });
  }

  protected reload(): void {
    if (this.isAuthenticated() && !this.favoritesState.loadedSuccessfully()) {
      this.retryFavorites();
      return;
    }

    this.load();
  }

  protected isFavorite(amazonProductId: string): boolean {
    return this.favoritesState.isFavorite(amazonProductId);
  }

  protected isFavoriteOperationInProgress(amazonProductId: string): boolean {
    return this.favoritesState.isOperationInProgress(amazonProductId);
  }

  protected retryFavorites(): void {
    this.state.set('favorites-loading');
    this.favoritesState.loadFavorites();
  }

  private load(): void {
    this.state.set('loading');
    this.errorMessage.set(`We couldn't load the bestseller list right now.`);

    this.bestsellersApi.getSoftwareBestSellers()
      .pipe(finalize(() => {
        if (this.state() === 'loading') {
          this.state.set(this.products().length > 0 ? 'success' : 'empty');
        }
      }))
      .subscribe({
        next: products => {
          this.products.set(products);
        },
        error: error => {
          this.products.set([]);
          const apiError = extractBestsellersError(error, `We couldn't load the bestseller list right now.`);
          this.state.set(apiError.statusCode === 503 ? 'unavailable' : 'error');
          this.errorMessage.set(apiError.message);
        }
      });
  }

  private syncInitialLoad(): void {
    if (!this.isAuthenticated()) {
      if (this.lastLoadContext !== 'anonymous') {
        this.lastLoadContext = 'anonymous';
        this.load();
      }

      return;
    }

    if (this.favoritesState.loadedSuccessfully()) {
      if (this.lastLoadContext !== 'authenticated') {
        this.lastLoadContext = 'authenticated';
        this.load();
      }

      return;
    }

    this.lastLoadContext = null;

    if (this.favoritesState.loading() || !this.favoritesState.hasLoaded()) {
      this.state.set('favorites-loading');
      return;
    }

    if (this.favoritesState.hasLoadError()) {
      this.products.set([]);
      this.state.set('favorites-error');
    }
  }

  protected addFavorite(product: BestsellerProduct): void {
    this.favoritesState.addFavorite({
      amazonProductId: product.amazonProductId,
      title: product.title,
      price: product.price,
      rating: product.rating,
      productUrl: product.productUrl,
      imageUrl: product.imageUrl
    });
  }

  protected removeFavorite(amazonProductId: string): void {
    this.favoritesState.removeFavorite(amazonProductId);
  }
}

function extractBestsellersError(error: unknown, fallbackMessage: string): { message: string; statusCode: number | null } {
  if (error instanceof HttpErrorResponse) {
    const payload = error.error as { errors?: unknown; detail?: unknown } | null;

    if (payload && Array.isArray(payload.errors)) {
      const firstError = payload.errors.find((item): item is string =>
        typeof item === 'string' && item.trim().length > 0);

      if (firstError) {
        return { message: firstError, statusCode: error.status };
      }
    }

    if (payload && typeof payload.detail === 'string' && payload.detail.trim().length > 0) {
      return { message: payload.detail, statusCode: error.status };
    }

    return { message: fallbackMessage, statusCode: error.status };
  }

  return { message: fallbackMessage, statusCode: null };
}
