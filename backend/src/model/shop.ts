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
    maxDistance: number;
    //opt
    hasCigars: boolean | null;
    search: string | null;
    //page
    itemsPerPage: number;
    pageOffset: number;
}

export type ShopDetails = {
    name: string | null;
    address: string;
}

export type ShopCoords = {
    longitude: number;
    latitude: number;
}

export type DaySchedule = {
    dayOfWeek: string,
    opening: string,
    closing: string,
}


export function getErrorSchedule(): DaySchedule[] {
    return [
        {
            dayOfWeek: "err",
            opening: "00:01",
            closing: "00:02",
        }
    ]
}

export function getErrorDetails(): ShopDetails {
    return {
        name: null,
        address: "err",
    }
}

export function getErrorCoords(): ShopCoords {
    return {
        longitude: NaN,
        latitude: NaN,
    }
}