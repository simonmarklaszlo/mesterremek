import shopAccess from "../../db/shopAccess";
import {Request, Response} from "express";
import {ShopFilter} from "../../model/shop";
import jwt from "jsonwebtoken";

export async function routeShopAll(req: Request, res: Response) {
    const {isValid, message, id} = handleId(req);

    if (!isValid) {
        res.send({err: message, id: id});
        return;
    }

    const shopCoords = await shopAccess.getShopCoords(id);
    const shopDetails = await shopAccess.getShopDetails(id);
    const shopSchedules = await shopAccess.getShopSchedules(id);

    res.send({
        coords: shopCoords,
        details: shopDetails,
        schedules: shopSchedules,
    })
}

export async function routeShopCoords(req: Request, res: Response): Promise<void> {
    const {isValid, message, id} = handleId(req);

    if (!isValid) {
        res.send({err: message, id: id});
        return;
    }

    const shopCoords = await shopAccess.getShopCoords(id);

    res.send(shopCoords);
}

export async function routeShopDetails(req: Request, res: Response): Promise<void> {
    const {isValid, message, id} = handleId(req);

    if (!isValid) {
        res.send({err: message, id: id});
        return;
    }

    const shopDetails = await shopAccess.getShopDetails(id);

    res.send(shopDetails);
}

export async function routeShopSchedules(req: Request, res: Response): Promise<void> {
    const {isValid, message, id} = handleId(req);

    if (!isValid) {
        res.send({err: message, id: id});
        return;
    }

    const shopSchedules = await shopAccess.getShopSchedules(id);

    res.send(shopSchedules);
}

export async function shopSearch(req: Request, res: Response): Promise<void> {
    const authHeader = req.headers['authorization'];
    if(!validateAuthToken(req.headers['authorization'])) {
        res.status(401).json({
            success: false,
            error: "Unauthorized",
            message: "Invalid or missing token"
        });
        return;
    }

    const reqLat = req.query.latitude as string | undefined;
    const reqLong = req.query.longitude as string | undefined;
    const reqMaxDistance = req.query.maxDistance as string | undefined;

    if (!reqLat || !reqLong || !reqMaxDistance) {
        res.status(400).json({
            success: false,
            error: "Invalid parameters",
            message: "latitude, longitude and maxDistance are required"
        });
        return;
    }

    const reqHasCigars = req.query.hasCigars as string | undefined;
    const reqSearch = req.query.search as string | undefined;
    const reqLimit = req.query.limit as string | undefined;
    const reqOffset = req.query.offset as string | undefined;

    const latitude = parseFloat(reqLat);
    const longitude = parseFloat(reqLong);
    const maxDistance = parseFloat(reqMaxDistance);
    const hasCigars = reqHasCigars ? reqHasCigars.toLowerCase() === "true" : null;
    const search = reqSearch ? reqSearch : null;
    const limit = reqLimit ? parseInt(reqLimit) : 20;
    const offset = reqOffset ? parseInt(reqOffset) : 0;

    const filter: ShopFilter = {
        latitude: latitude,
        longitude: longitude,
        maxDistance: maxDistance,
        hasCigars: hasCigars,
        search: search,
        itemsPerPage: limit,
        pageOffset: offset
    }

    const page = await shopAccess.getShopPage(filter);
}

function handleId(req: Request): { isValid: boolean, message: string | null, id: number } {
    const reqId = req.params.id;

    if (!reqId) {
        return {
            isValid: false,
            message: "No id",
            id: -1
        }
    }

    const id = parseInt(reqId);

    if (id < 1) {
        return {
            isValid: false,
            message: "Invalid id",
            id: id
        }
    }

    return {
        isValid: true,
        message: null,
        id: id
    }
}
function validateAuthToken(authHeader : string | undefined): boolean{
    try {
        const token = authHeader && authHeader.split(' ')[1];
        if(!token) {
            return false;
        }
        jwt.verify(token, process.env.JWT_SECRET!);

        return true;
    }
    catch (error) {
        return false;
    }
}
