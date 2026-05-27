import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { DataViewModule } from 'primeng/dataview';
import { ProductCardComponent } from '../../../shared/components/product-card/product-card.component';
import { FavoritesStateService } from '../data/favorites-state.service';

@Component({
  selector: 'app-favorites-page',
  standalone: true,
  imports: [DataViewModule, ProductCardComponent],
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
                <app-product-card
                  [product]="product"
                  [isFavorite]="true"
                  [showFavoriteAction]="true"
                  [isFavoriteActionLoading]="favoritesState.isOperationInProgress(product.amazonProductId)"
                  (removeFavorite)="remove($event)"
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
      color: #1840b3;
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
      color: #ffffff;
      font-weight: 700;
      cursor: pointer;
    }

    .refresh-button {
      background: #15308d;
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

}
