import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { DataViewModule } from 'primeng/dataview';
import { AuthStateService } from '../../../core/auth/auth-state.service';
import { BestsellersApiService } from '../data/bestsellers-api.service';
import { BestsellerProduct } from '../../../shared/models/bestseller-product.model';
import { FavoritesStateService } from '../../favorites/data/favorites-state.service';

type BestsellersViewState = 'loading' | 'error' | 'empty' | 'success';

@Component({
  selector: 'app-bestsellers-page',
  standalone: true,
  imports: [DataViewModule],
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

      @if (isLoading()) {
        <section class="state-card">
          <h2>Loading software bestsellers</h2>
          <p>Fetching the latest Amazon Poland software bestsellers.</p>
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
                <article class="product-card" [class.is-favorite]="isFavorite(product.amazonProductId)">
                  <div class="product-media">
                    @if (product.imageUrl) {
                      <img [src]="product.imageUrl" [alt]="product.title" loading="lazy" />
                    } @else {
                      <div class="image-fallback">Image unavailable</div>
                    }
                  </div>

                  <div class="product-body">
                    <h2 [attr.title]="product.title">{{ product.title }}</h2>

                    <dl class="product-metrics">
                      <div>
                        <dt>Price</dt>
                        <dd>{{ formatPrice(product.price) }}</dd>
                      </div>

                      <div>
                        <dt>Rating</dt>
                        <dd>{{ formatRating(product.rating) }}</dd>
                      </div>
                    </dl>

                    <div class="product-actions">
                      <a
                        class="amazon-link"
                        [href]="product.productUrl"
                        target="_blank"
                        rel="noopener noreferrer"
                      >
                        View on Amazon
                      </a>

                      @if (isAuthenticated()) {
                        <button
                          type="button"
                          class="favorite-button"
                          [class.is-active]="isFavorite(product.amazonProductId)"
                          [disabled]="isFavoriteOperationInProgress(product.amazonProductId)"
                          (click)="toggleFavorite(product)"
                        >
                          @if (isFavoriteOperationInProgress(product.amazonProductId)) {
                            Updating...
                          } @else if (isFavorite(product.amazonProductId)) {
                            Remove from favorites
                          } @else {
                            Add to favorites
                          }
                        </button>
                      }
                    </div>
                  </div>
                </article>
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

    .product-card {
      display: grid;
      grid-template-rows: 220px 1fr;
      border-radius: 1.25rem;
      overflow: hidden;
      background: rgba(255, 255, 255, 0.92);
      border: 1px solid rgba(19, 32, 40, 0.1);
      box-shadow: 0 10px 24px rgba(19, 32, 40, 0.07);
      transition: transform 180ms ease, box-shadow 180ms ease, border-color 180ms ease;
    }

    .product-card.is-favorite {
      border-color: rgba(24, 64, 179, 0.28);
      box-shadow: 0 12px 28px rgba(24, 64, 179, 0.1);
    }

    .product-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 16px 34px rgba(19, 32, 40, 0.1);
    }

    .product-media {
      background: #f4f6f7;
      display: grid;
      place-items: center;
      overflow: hidden;
    }

    .product-media img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      display: block;
    }

    .image-fallback {
      color: #43525d;
      font-size: 0.95rem;
      font-weight: 600;
      width: 100%;
      height: 100%;
      border-radius: 1rem 1rem 0 0;
      display: grid;
      place-items: center;
    }

    .product-body {
      display: grid;
      gap: 1.1rem;
      padding: 1.25rem;
      align-content: start;
    }

    .product-body h2 {
      color: #132028;
      font-size: 1.12rem;
      font-weight: 800;
      line-height: 1.4;
      letter-spacing: -0.01em;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      min-height: calc(1.4em * 2);
    }

    .product-metrics {
      display: grid;
      gap: 0.9rem;
      margin: 0;
    }

    .product-metrics div {
      display: grid;
      gap: 0.22rem;
      padding-bottom: 0.75rem;
      border-bottom: 1px solid rgba(19, 32, 40, 0.08);
    }

    .product-metrics dt,
    .product-metrics dd {
      margin: 0;
    }

    .product-metrics dt {
      color: #586874;
      font-size: 0.95rem;
      font-weight: 600;
    }

    .product-metrics dd {
      color: #132028;
      font-size: 1rem;
      font-weight: 800;
      text-align: left;
    }

    .product-actions {
      display: grid;
      gap: 0.75rem;
      margin-top: auto;
    }

    .amazon-link {
      width: 100%;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border-radius: 999px;
      padding: 0.82rem 1rem;
      background: #132028;
      color: #ffffff;
      text-decoration: none;
      font-weight: 700;
      transition: transform 160ms ease, opacity 160ms ease;
    }

    .amazon-link:hover {
      transform: translateY(-1px);
      opacity: 0.92;
    }

    .favorite-button {
      width: 100%;
      border: 1px solid rgba(21, 48, 141, 0.22);
      border-radius: 999px;
      padding: 0.82rem 1rem;
      background: rgba(255, 255, 255, 0.96);
      color: #163685;
      font-weight: 700;
      cursor: pointer;
      transition:
        transform 160ms ease,
        opacity 160ms ease,
        background-color 160ms ease,
        border-color 160ms ease,
        color 160ms ease;
    }

    .favorite-button.is-active {
      background: rgba(24, 64, 179, 0.1);
      border-color: rgba(24, 64, 179, 0.3);
      color: #102a78;
    }

    .favorite-button:hover:not(:disabled) {
      transform: translateY(-1px);
      background: rgba(24, 64, 179, 0.08);
      border-color: rgba(24, 64, 179, 0.32);
    }

    .favorite-button:disabled {
      cursor: wait;
      opacity: 0.7;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BestsellersPageComponent {
  private readonly bestsellersApi = inject(BestsellersApiService);
  private readonly favoritesState = inject(FavoritesStateService);
  private readonly authState = inject(AuthStateService);

  protected readonly state = signal<BestsellersViewState>('loading');
  protected readonly products = signal<BestsellerProduct[]>([]);
  protected readonly errorMessage = signal(`We couldn't load the bestseller list right now.`);

  protected readonly isLoading = computed(() => this.state() === 'loading');
  protected readonly hasError = computed(() => this.state() === 'error');
  protected readonly isEmpty = computed(() => this.state() === 'empty');
  protected readonly isAuthenticated = this.authState.isAuthenticated;
  protected readonly favoriteError = computed(() =>
    this.isAuthenticated() ? this.favoritesState.error() : null);
  protected readonly showFavoriteError = computed(() =>
    !this.isLoading() && !this.hasError() && this.favoriteError() !== null);

  constructor() {
    this.load();
  }

  protected reload(): void {
    this.load();
  }

  protected formatPrice(price: number | null): string {
    return price === null
      ? 'Price unavailable'
      : `${price.toFixed(2)} PLN`;
  }

  protected formatRating(rating: number | null): string {
    return rating === null
      ? 'No rating'
      : `${rating.toFixed(1)} / 5`;
  }

  protected isFavorite(amazonProductId: string): boolean {
    return this.favoritesState.isFavorite(amazonProductId);
  }

  protected isFavoriteOperationInProgress(amazonProductId: string): boolean {
    return this.favoritesState.isOperationInProgress(amazonProductId);
  }

  protected toggleFavorite(product: BestsellerProduct): void {
    if (this.isFavoriteOperationInProgress(product.amazonProductId)) {
      return;
    }

    if (this.isFavorite(product.amazonProductId)) {
      this.removeFavorite(product.amazonProductId);
      return;
    }

    this.addFavorite(product);
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
        error: () => {
          this.products.set([]);
          this.state.set('error');
          this.errorMessage.set(`We couldn't load the bestseller list right now.`);
        }
      });
  }

  private addFavorite(product: BestsellerProduct): void {
    this.favoritesState.addFavorite({
      amazonProductId: product.amazonProductId,
      title: product.title,
      price: product.price,
      rating: product.rating,
      productUrl: product.productUrl,
      imageUrl: product.imageUrl
    });
  }

  private removeFavorite(amazonProductId: string): void {
    this.favoritesState.removeFavorite(amazonProductId);
  }
}
