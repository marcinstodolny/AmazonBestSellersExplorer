export interface FavoriteProduct {
  amazonProductId: string;
  title: string;
  price: number | null;
  rating: number | null;
  productUrl: string;
  imageUrl: string | null;
}
