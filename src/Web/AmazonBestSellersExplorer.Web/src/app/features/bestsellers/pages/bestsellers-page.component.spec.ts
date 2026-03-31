import { provideZonelessChangeDetection, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AuthStateService } from '../../../core/auth/auth-state.service';
import { BestsellerProduct } from '../../../shared/models/bestseller-product.model';
import { FavoritesStateService } from '../../favorites/data/favorites-state.service';
import { BestsellersApiService } from '../data/bestsellers-api.service';
import { BestsellersPageComponent } from './bestsellers-page.component';

describe('BestsellersPageComponent', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('shows favorite mutation errors on the bestseller page', async () => {
    const favoriteError = signal<string | null>(`We couldn't save this product to your favorites.`);
    const bestsellersApi = {
      getSoftwareBestSellers: () => of<BestsellerProduct[]>([
        {
          amazonProductId: 'B09TEST001',
          title: 'Windows 11 Pro',
          price: 699.99,
          rating: 4.7,
          productUrl: 'https://www.amazon.pl/dp/B09TEST001',
          imageUrl: 'https://images.example.com/B09TEST001.jpg'
        }
      ])
    };

    const favoritesState = {
      error: favoriteError,
      isFavorite: () => false,
      isOperationInProgress: () => false,
      addFavorite: jasmine.createSpy('addFavorite'),
      removeFavorite: jasmine.createSpy('removeFavorite')
    };

    await TestBed.configureTestingModule({
      imports: [BestsellersPageComponent],
      providers: [
        provideZonelessChangeDetection(),
        { provide: BestsellersApiService, useValue: bestsellersApi },
        { provide: FavoritesStateService, useValue: favoritesState },
        {
          provide: AuthStateService,
          useValue: {
            isAuthenticated: signal(true)
          } as Pick<AuthStateService, 'isAuthenticated'>
        }
      ]
    }).compileComponents();

    const fixture = TestBed.createComponent(BestsellersPageComponent);

    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain(`We couldn't save this product to your favorites.`);
  });
});
