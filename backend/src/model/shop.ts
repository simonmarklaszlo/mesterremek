export type Shop = {
    id: number;
    details : ShopDetails | null;
    coords : ShopCoords | null;
    schedule : DaySchedule[] | null;
}

export type ShopDetails = {
    name : string | null;
    address : string;
}

export type ShopCoords = {
    longitude : number;
    latitude : number;
}

export type DaySchedule = {
    dayOfWeek : string,
    opening : string,
    closing  : string,
}


export function getErrorSchedule(): DaySchedule[]{
    return [
        {
            dayOfWeek : "err",
            opening : "00:01",
            closing  : "00:02",
        }
    ]
}

export function getErrorDetails() : ShopDetails {
    return {
        name : null,
        address : "err",
    }
}

export function getErrorCoords() : ShopCoords{
    return {
        longitude : NaN,
        latitude : NaN,
    }
}