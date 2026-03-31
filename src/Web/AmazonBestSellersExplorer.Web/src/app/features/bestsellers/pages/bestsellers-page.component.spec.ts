import { provideZonelessChangeDetection, signal, WritableSignal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { AuthStateService } from '../../../core/auth/auth-state.service';
import { BestsellerProduct } from '../../../shared/models/bestseller-product.model';
import { FavoritesStateService } from '../../favorites/data/favorites-state.service';
import { BestsellersApiService } from '../data/bestsellers-api.service';
import { BestsellersPageComponent } from './bestsellers-page.component';

describe('BestsellersPageComponent', () => {
  const bestsellers = [
    {
      amazonProductId: 'B09TEST001',
      title: 'Windows 11 Pro',
      price: 699.99,
      rating: 4.7,
      productUrl: 'https://www.amazon.pl/dp/B09TEST001',
      imageUrl: 'https://images.example.com/B09TEST001.jpg'
    }
  ] satisfies BestsellerProduct[];

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  async function createComponent(options?: {
    isAuthenticated?: boolean;
    favoriteError?: WritableSignal<string | null>;
    favoritesLoading?: WritableSignal<boolean>;
    favoritesHasLoaded?: WritableSignal<boolean>;
    favoritesLoadedSuccessfully?: WritableSignal<boolean>;
    favoritesHasLoadError?: WritableSignal<boolean>;
    bestsellersApi?: { getSoftwareBestSellers: jasmine.Spy };
    favoritesLoadSpy?: jasmine.Spy;
  }) {
    const bestsellersApi = options?.bestsellersApi ?? {
      getSoftwareBestSellers: jasmine.createSpy('getSoftwareBestSellers').and.returnValue(of<BestsellerProduct[]>(bestsellers))
    };

    const favoriteError = options?.favoriteError ?? signal<string | null>(null);
    const favoriteLoadError = signal<string | null>(favoriteError());
    const favoritesLoading = options?.favoritesLoading ?? signal(false);
    const favoritesHasLoaded = options?.favoritesHasLoaded ?? signal(true);
    const favoritesLoadedSuccessfully = options?.favoritesLoadedSuccessfully ?? signal(true);
    const favoritesHasLoadError = options?.favoritesHasLoadError ?? signal(false);
    const favoritesLoadSpy = options?.favoritesLoadSpy ?? jasmine.createSpy('loadFavorites');

    const favoritesState = {
      error: favoriteError,
      loadError: favoriteLoadError,
      loading: favoritesLoading,
      hasLoaded: favoritesHasLoaded,
      loadedSuccessfully: favoritesLoadedSuccessfully,
      hasLoadError: favoritesHasLoadError,
      loadFavorites: favoritesLoadSpy,
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
            isAuthenticated: signal(options?.isAuthenticated ?? true)
          } as Pick<AuthStateService, 'isAuthenticated'>
        }
      ]
    }).compileComponents();

    const fixture = TestBed.createComponent(BestsellersPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    return { fixture, bestsellersApi, favoritesState };
  }

  it('shows favorite mutation errors on the bestseller page', async () => {
    const { fixture } = await createComponent({
      favoriteError: signal<string | null>(`We couldn't save this product to your favorites.`)
    });
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain(`We couldn't save this product to your favorites.`);
  });

  it('waits for favorites to load successfully before loading bestsellers for authenticated users', async () => {
    const bestsellersApi = {
      getSoftwareBestSellers: jasmine.createSpy('getSoftwareBestSellers').and.returnValue(of<BestsellerProduct[]>(bestsellers))
    };

    await createComponent({
      isAuthenticated: true,
      favoritesLoading: signal(true),
      favoritesHasLoaded: signal(false),
      favoritesLoadedSuccessfully: signal(false),
      favoritesHasLoadError: signal(false),
      bestsellersApi
    });

    expect(bestsellersApi.getSoftwareBestSellers).not.toHaveBeenCalled();
  });

  it('shows retry state when authenticated favorites fail before bestsellers load', async () => {
    const bestsellersApi = {
      getSoftwareBestSellers: jasmine.createSpy('getSoftwareBestSellers').and.returnValue(of<BestsellerProduct[]>(bestsellers))
    };

    const { fixture, favoritesState } = await createComponent({
      isAuthenticated: true,
      favoriteError: signal<string | null>(`We couldn't load your saved products right now.`),
      favoritesLoading: signal(false),
      favoritesHasLoaded: signal(true),
      favoritesLoadedSuccessfully: signal(false),
      favoritesHasLoadError: signal(true),
      bestsellersApi
    });
    const compiled = fixture.nativeElement as HTMLElement;
    const retryButton = compiled.querySelector('.state-card .refresh-button') as HTMLButtonElement;

    expect(compiled.textContent).toContain(`Couldn't load your favorites`);
    expect(compiled.textContent).toContain(`We couldn't load your saved products right now.`);
    expect(bestsellersApi.getSoftwareBestSellers).not.toHaveBeenCalled();

    retryButton.click();

    expect(favoritesState.loadFavorites).toHaveBeenCalled();
  });

  it('shows dedicated unavailable state when the bestseller service is not configured correctly', async () => {
    const { fixture } = await createComponent({
      isAuthenticated: false,
      bestsellersApi: {
        getSoftwareBestSellers: jasmine.createSpy('getSoftwareBestSellers').and.returnValue(
          throwError(() => new HttpErrorResponse({
            status: 503,
            error: {
              detail: 'The service is not configured correctly. Please try again later.'
            }
          }))
        )
      }
    });
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Bestsellers are currently unavailable.');
    expect(compiled.textContent).toContain('The service is not configured correctly.');
  });
});
