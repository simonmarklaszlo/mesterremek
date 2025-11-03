export type Shop = {
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