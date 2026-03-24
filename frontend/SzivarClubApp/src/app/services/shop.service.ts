import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Shop {
  id: number;
  name: string;
  address: string;
  city: string;
  latitude: number;
  longitude: number;
  distance: number;
  hasCigars: boolean;
  rating?: number;
  reviewCount?: number;
}

export interface ShopSearchParams {
  latitude: number;
  longitude: number;
  maxDistance?: number;
  hasCigars?: boolean;
  search?: string;
  limit?: number;
  offset?: number;
}

export interface ShopSearchResponse {
  success: boolean;
  data: Shop[];  // A backend közvetlenül a tömböt adja vissza
  total: number;
  limit: number;
  offset: number;
  hasMore: boolean;
}

export interface OpeningHour {
  id: number;
  dayOfWeek: string;
  openHour: string;
  closeHour: string;
}

export interface CigarBrand {
  id: number;
  brandId: number;
  brandName: string;
  addedBy: number;
  createdAt: string;
}

export interface Review {
  id: number;
  userId: number;
  userName: string;
  rating: number;
  comment: string;
  createdAt: string;
}

export interface ShopDetails {
  id: number;
  name: string;
  address: string;
  city: string;
  latitude: number;
  longitude: number;
  createdAt: string;
  updatedAt: string;
  openingHours: OpeningHour[];
  cigarBrands: CigarBrand[];
  reviews: Review[];
  averageRating: number;
  totalReviews: number;
  isOpenNow: boolean;
  nextClosingTime: string | null;
}

export interface ShopDetailsResponse {
  success: boolean;
  data: ShopDetails;
}

export interface ReceivedCigarResponse {
  success: boolean;
  data: {
    userId: number;
    shopId: number;
    receivedAt: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  private apiUrl = `${environment.apiUrl}/shops`;

  constructor(private http: HttpClient) {}

  // Boltok keresése
  searchShops(params: ShopSearchParams): Observable<ShopSearchResponse> {
    let httpParams = new HttpParams()
      .set('latitude', params.latitude.toString())
      .set('longitude', params.longitude.toString());

    if (params.maxDistance !== undefined) {
      httpParams = httpParams.set('maxDistance', params.maxDistance.toString());
    }
    if (params.hasCigars !== undefined) {
      httpParams = httpParams.set('hasCigars', params.hasCigars.toString());
    }
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.limit !== undefined) {
      httpParams = httpParams.set('limit', params.limit.toString());
    }
    if (params.offset !== undefined) {
      httpParams = httpParams.set('offset', params.offset.toString());
    }

    return this.http.get<ShopSearchResponse>(`${this.apiUrl}/search`, { params: httpParams });
  }

  // Bolt részleteinek lekérése
  getShopDetails(shopId: number): Observable<ShopDetailsResponse> {
    return this.http.get<ShopDetailsResponse>(`${this.apiUrl}/${shopId}`);
  }

  // "Kaptam szivart" esemény rögzítése
  receivedCigar(shopId: number): Observable<ReceivedCigarResponse> {
    return this.http.post<ReceivedCigarResponse>(`${this.apiUrl}/${shopId}/received-cigar`, {});
  }
}
