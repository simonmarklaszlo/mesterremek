import {OpeningHour} from "./openingHour";
import {CigarBrand} from "./cigarBrand";
import {Review} from "./review";

export type Shop = {
    id: number;
    name: string;
    address: string;
    city: string;

    latitude: number;
    longitude: number;

    createdAt: string;
    updatedAt: string;
}

export type ShopFilterResult = {
     id: number;
    name: string;
    address: string;
    city: string;
    latitude: number;
    longitude: number;
    distance: number;
    hasCigars: boolean;
    rating: number;
    reviewCount: number;
}

export type ShopFilter = {
    //required
    latitude: number;
    longitude: number;
    maxDistanceKm: number;
    //opt
    hasCigars: boolean | null;
    search: string | null;
    //page
    itemsPerPage: number;
    pageOffset: number;
}

export type ShopDetails = Shop & {
    openingHours: OpeningHour[];
    cigarBrands: CigarBrand[];
    reviews: Review[];

    averageRating: number;
    totalReviews: number;
    isOpenNow: boolean;
    nextClosingTime: string | null;
}