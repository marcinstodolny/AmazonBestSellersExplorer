import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { DataViewModule } from 'primeng/dataview';
import { FavoritesStateService } from '../data/favorites-state.service';

@Component({
  selector: 'app-favorites-page',
  standalone: true,
  imports: [DataViewModule],
  template: `
    <section class="page-shell">
      <header class="page-header">
        <div>
          <span class="page-kicker">Saved for later</span>
          <h1>Your favorites</h1>
        </div>

        <button type="button" class="refresh-button" (click)="reload()" [disabled]="isLoading() || isRemoving()">
          {{ isLoading() ? 'Refreshing...' : 'Refresh list' }}
        </button>
      </header>

      @if (isLoading()) {
        <section class="state-card">
          <h2>Loading your favorites</h2>
          <p>Fetching the products you saved to revisit later.</p>
        </section>
      } @else if (hasError()) {
        <section class="state-card is-error">
          <h2>Couldn't load your favorites</h2>
          <p>{{ errorMessage() }}</p>
        </section>
      } @else if (isEmpty()) {
        <section class="state-card">
          <h2>You haven't saved any favorites yet</h2>
          <p>Products saved from the bestseller list will appear here.</p>
        </section>
      } @else {
        <p-dataview class="catalog-view" [value]="products()" layout="list">
          <ng-template #list let-items>
            <div class="products-grid">
              @for (product of items; track product.amazonProductId) {
                <article class="product-card">
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

                      <button
                        type="button"
                        class="remove-button"
                        (click)="remove(product.amazonProductId)"
                        [disabled]="favoritesState.isOperationInProgress(product.amazonProductId)"
                      >
                        {{ favoritesState.isOperationInProgress(product.amazonProductId) ? 'Removing...' : 'Remove from favorites' }}
                      </button>
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
      color: #1840b3;
    }

    h1,
    h2 {
      margin: 0;
      font-size: clamp(2rem, 3vw, 2.75rem);
      line-height: 1;
    }

    .refresh-button,
    .remove-button {
      border: 0;
      border-radius: 999px;
      padding: 0.85rem 1.15rem;
      color: #ffffff;
      font-weight: 700;
      cursor: pointer;
    }

    .refresh-button {
      background: #15308d;
    }

    .remove-button {
      background: #8f1d35;
    }

    .refresh-button:disabled,
    .remove-button:disabled {
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
    }

.product-media {
      background: #ffffff;
      display: grid;
      place-items: center;
      overflow: hidden;
      padding: 10px;
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
      display: flex;
      justify-content: space-between;
      gap: 1rem;
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
      text-align: right;
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

    .remove-button {
      width: 100%;
      border: 1px solid rgba(143, 29, 53, 0.22);
      border-radius: 999px;
      padding: 0.82rem 1rem;
      background: rgba(255, 255, 255, 0.96);
      color: #7d1730;
      font-weight: 700;
      transition:
        transform 160ms ease,
        opacity 160ms ease,
        background-color 160ms ease,
        border-color 160ms ease,
        color 160ms ease;
    }

    .remove-button:hover:not(:disabled) {
      transform: translateY(-1px);
      background: rgba(143, 29, 53, 0.06);
      border-color: rgba(143, 29, 53, 0.3);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FavoritesPageComponent {
  protected readonly favoritesState = inject(FavoritesStateService);

  protected readonly products = this.favoritesState.favorites;
  protected readonly errorMessage = computed(() => this.favoritesState.error() ?? `We couldn't load your favorites right now.`);
  protected readonly isLoading = this.favoritesState.loading;
  protected readonly hasError = computed(() => this.favoritesState.error() !== null);
  protected readonly isEmpty = computed(() =>
    this.favoritesState.hasLoaded()
    && !this.isLoading()
    && !this.hasError()
    && this.products().length === 0);
  protected readonly isRemoving = this.favoritesState.hasPendingOperations;

  constructor() {
    this.favoritesState.ensureLoaded();
  }

  protected reload(): void {
    this.favoritesState.loadFavorites();
  }

  protected remove(amazonProductId: string): void {
    this.favoritesState.removeFavorite(amazonProductId);
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
}
