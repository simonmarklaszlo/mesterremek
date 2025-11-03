import shopAccess from "../../db/shopAccess";
import {Request, Response} from "express";
import {ShopFilter} from "../../model/shop";
import jwt from "jsonwebtoken";
import {config} from "../../config/config";

export async function handleShopSearch(req: Request, res: Response): Promise<void> {
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
        maxDistanceKm: maxDistance,
        hasCigars: hasCigars,
        search: search,
        itemsPerPage: limit,
        pageOffset: offset
    }

    const page = await shopAccess.getShopPage(filter);
    const allCount = await shopAccess.countShopPages(filter);

    console.log(allCount);

    res.status(200).json({
        success: true,
        data: page,
        total: page.length,
        limit : limit,
        offset: offset,
        hasMore : allCount > page.length
    })
}

function validateAuthToken(authHeader : string | undefined): boolean{
    try {
        const token = authHeader && authHeader.split(' ')[1];
        if(!token) {
            return false;
        }
        jwt.verify(token,config.jwt.secret);

        return true;
    }
    catch (error) {
        return false;
    }
}
