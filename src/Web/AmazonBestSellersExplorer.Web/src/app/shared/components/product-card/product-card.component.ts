import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

export interface ProductCardProduct {
  amazonProductId: string;
  title: string;
  price: number | null;
  rating: number | null;
  productUrl: string;
  imageUrl: string | null;
}

@Component({
  selector: 'app-product-card',
  standalone: true,
  template: `
    <article class="product-card" [class.is-favorite]="isFavorite">
      @if (showFavoriteAction) {
        <button
          type="button"
          class="favorite-toggle-button"
          [class.is-active]="isFavorite"
          [disabled]="isFavoriteActionLoading"
          (click)="toggleFavorite()"
          [attr.aria-label]="favoriteActionLabel"
          [attr.title]="favoriteActionLabel"
        >
          <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M12 21.35 10.55 20.03C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09A6 6 0 0 1 16.5 3C19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54z" />
          </svg>
        </button>
      }

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
        </div>
      </div>
    </article>
  `,
  styles: `
    :host {
      display: block;
      min-width: 0;
    }

    .product-card {
      position: relative;
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

    .favorite-toggle-button {
      position: absolute;
      top: 0.85rem;
      right: 0.85rem;
      z-index: 1;
      width: 2.5rem;
      height: 2.5rem;
      display: inline-grid;
      place-items: center;
      border: 1px solid rgba(21, 48, 141, 0.16);
      border-radius: 999px;
      padding: 0;
      background: rgba(255, 255, 255, 0.92);
      color: #4b6280;
      cursor: pointer;
      backdrop-filter: blur(8px);
      transition:
        transform 160ms ease,
        background-color 160ms ease,
        border-color 160ms ease,
        color 160ms ease,
        box-shadow 160ms ease,
        opacity 160ms ease;
    }

    .favorite-toggle-button svg {
      width: 1.2rem;
      height: 1.2rem;
      display: block;
    }

    .favorite-toggle-button path {
      fill: none;
      stroke: currentColor;
      stroke-width: 1.8;
      stroke-linejoin: round;
    }

    .favorite-toggle-button.is-active {
      color: #8f1d35;
      border-color: rgba(143, 29, 53, 0.14);
    }

    .favorite-toggle-button.is-active path {
      fill: currentColor;
      stroke: currentColor;
    }

    .favorite-toggle-button:hover:not(:disabled) {
      transform: translateY(-1px);
      background: rgba(243, 246, 251, 0.98);
      border-color: rgba(21, 48, 141, 0.26);
      color: #163685;
      box-shadow: 0 8px 18px rgba(19, 32, 40, 0.1);
    }

    .favorite-toggle-button.is-active:hover:not(:disabled) {
      background: rgba(255, 244, 246, 0.98);
      border-color: rgba(143, 29, 53, 0.28);
      color: #7d1730;
    }

    .favorite-toggle-button:disabled {
      cursor: wait;
      opacity: 0.7;
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
      letter-spacing: 0;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      min-height: calc(1.4em * 2);
      margin: 0;
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
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductCardComponent {
  @Input({ required: true }) product!: ProductCardProduct;
  @Input() isFavorite = false;
  @Input() showFavoriteAction = false;
  @Input() isFavoriteActionLoading = false;

  @Output() addFavorite = new EventEmitter<ProductCardProduct>();
  @Output() removeFavorite = new EventEmitter<string>();

  protected get favoriteActionLabel(): string {
    return this.isFavorite ? 'Remove from favorites' : 'Add to favorites';
  }

  protected toggleFavorite(): void {
    if (this.isFavoriteActionLoading) {
      return;
    }

    if (this.isFavorite) {
      this.removeFavorite.emit(this.product.amazonProductId);
      return;
    }

    this.addFavorite.emit(this.product);
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
