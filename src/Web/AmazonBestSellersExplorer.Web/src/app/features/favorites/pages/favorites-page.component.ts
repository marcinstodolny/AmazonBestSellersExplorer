import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { DataViewModule } from 'primeng/dataview';
import { FavoriteProduct } from '../models/favorite-product.model';
import { FavoritesApiService } from '../data/favorites-api.service';

type FavoritesViewState = 'loading' | 'error' | 'empty' | 'success';

@Component({
  selector: 'app-favorites-page',
  standalone: true,
  imports: [DataViewModule],
  template: `
    <section class="page-shell">
      <header class="page-header">
        <div>
          <span class="page-kicker">Private API</span>
          <h1>Your favorites</h1>
        </div>

        <button type="button" class="refresh-button" (click)="reload()" [disabled]="isLoading() || isRemoving()">
          {{ isLoading() ? 'Loading...' : 'Refresh' }}
        </button>
      </header>

      @if (isLoading()) {
        <section class="state-card">
          <h2>Loading favorites</h2>
          <p>Frontend pobiera dane z chronionego endpointu <code>GET /api/favorites</code>.</p>
        </section>
      } @else if (hasError()) {
        <section class="state-card is-error">
          <h2>Unable to load favorites</h2>
          <p>{{ errorMessage() }}</p>
        </section>
      } @else if (isEmpty()) {
        <section class="state-card">
          <h2>No favorites yet</h2>
          <p>Your favorites list is empty. Saved products will appear here.</p>
        </section>
      } @else {
        <p-dataview [value]="products()" layout="list">
          <ng-template #list let-items>
            <div class="products-grid">
              @for (product of items; track product.amazonProductId) {
                <article class="product-card">
                  <div class="product-media">
                    @if (product.imageUrl) {
                      <img [src]="product.imageUrl" [alt]="product.title" loading="lazy" />
                    } @else {
                      <div class="image-fallback">No image</div>
                    }
                  </div>

                  <div class="product-body">
                    <h2>{{ product.title }}</h2>

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
                        Zobacz w Amazon
                      </a>

                      <button
                        type="button"
                        class="remove-button"
                        (click)="remove(product.amazonProductId)"
                        [disabled]="removingProductId() === product.amazonProductId"
                      >
                        {{ removingProductId() === product.amazonProductId ? 'Removing...' : 'Usuń z ulubionych' }}
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
      color: #4c5b67;
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
    }

    .product-card {
      display: grid;
      grid-template-rows: 220px 1fr;
      border-radius: 1.25rem;
      overflow: hidden;
      background: rgba(255, 255, 255, 0.92);
      border: 1px solid rgba(19, 32, 40, 0.08);
      box-shadow: 0 12px 34px rgba(19, 32, 40, 0.08);
    }

    .product-media {
      background:
        linear-gradient(180deg, rgba(24, 64, 179, 0.08), rgba(19, 94, 70, 0.12)),
        #f4f6f7;
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
      color: #5d6d78;
      font-size: 0.95rem;
      font-weight: 600;
    }

    .product-body {
      display: grid;
      gap: 1rem;
      padding: 1.25rem;
      align-content: start;
    }

    .product-body h2 {
      font-size: 1.2rem;
      line-height: 1.25;
    }

    .product-metrics {
      display: grid;
      gap: 0.85rem;
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
      color: #5d6d78;
      font-weight: 600;
    }

    .product-metrics dd {
      color: #132028;
      font-weight: 700;
      text-align: right;
    }

    .product-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .amazon-link {
      width: fit-content;
      display: inline-flex;
      align-items: center;
      border-radius: 999px;
      padding: 0.8rem 1rem;
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
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FavoritesPageComponent {
  private readonly favoritesApi = inject(FavoritesApiService);

  protected readonly state = signal<FavoritesViewState>('loading');
  protected readonly products = signal<FavoriteProduct[]>([]);
  protected readonly errorMessage = signal('Unable to load favorites.');
  protected readonly removingProductId = signal<string | null>(null);

  protected readonly isLoading = computed(() => this.state() === 'loading');
  protected readonly hasError = computed(() => this.state() === 'error');
  protected readonly isEmpty = computed(() => this.state() === 'empty');
  protected readonly isRemoving = computed(() => this.removingProductId() !== null);

  constructor() {
    this.load();
  }

  protected reload(): void {
    this.load();
  }

  protected remove(amazonProductId: string): void {
    this.removingProductId.set(amazonProductId);

    this.favoritesApi.removeFavorite(amazonProductId)
      .pipe(finalize(() => this.removingProductId.set(null)))
      .subscribe({
        next: () => {
          const nextProducts = this.products().filter(product => product.amazonProductId !== amazonProductId);
          this.products.set(nextProducts);
          this.state.set(nextProducts.length > 0 ? 'success' : 'empty');
        },
        error: () => {
          this.errorMessage.set('Unable to remove favorite product.');
          this.state.set('error');
        }
      });
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

  private load(): void {
    this.state.set('loading');
    this.errorMessage.set('Unable to load favorites.');

    this.favoritesApi.getFavorites()
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
          this.errorMessage.set('Backend did not return favorite products.');
        }
      });
  }
}
