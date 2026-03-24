import shopAccess from "../../db/shopAccess";
import {Request, Response} from "express";
import {ShopFilter} from "../../model/shop";
import jwt from "jsonwebtoken";
import {config} from "../../config/config";
import userActionsAccess from "../../db/userActionsAccess";

export async function handleShopSearch(req: Request, res: Response): Promise<void> {
    if (!validateAuthToken(req.headers['authorization'])) {
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
        limit: limit,
        offset: offset,
        hasMore: allCount > page.length
    })
}

export async function handleShopDetails(req: Request, res: Response): Promise<void> {
    if (!validateAuthToken(req.headers['authorization'])) {
        res.status(401).json({
            success: false,
            error: "Unauthorized",
            message: "Invalid or missing token"
        });
        return;
    }

    const reqId = req.params.id as string | undefined;
    if (!reqId) {
        res.status(404).json({
            success: false,
            error: "Not Found",
            message: "Shop id missing"
        });
        return;
    }

    const id = parseInt(reqId);

    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1];
    if (!token) {
        res.status(401).json({
            success: false,
            error: "Unauthorized",
            message: "Missing token"
        });
        return;
    }

    let payload: any;
    try {
        payload = jwt.verify(token, config.jwt.secret) as any;
    } catch {
        res.status(401).json({
            success: false,
            error: "Unauthorized",
            message: "Invalid token"
        });
        return;
    }

    const details = await shopAccess.getShopDetails(id);

    if (!details) {
        res.status(404).json({
            success: false,
            error: "Not Found",
            message: `Shop with id ${id} not found`
        });
        return;
    }

    const userId = payload.userId;
    let hasReceivedCigar = false;
    let lastReceivedAt: string | null = null;
    try {
        hasReceivedCigar = await userActionsAccess.hasReceivedCigar(userId, id);
        if (hasReceivedCigar) {
            lastReceivedAt = await userActionsAccess.getLastReceivedCigarAt(userId, id);
        }
    } catch (e) {
        console.warn(`[SHOP_DETAILS] cigar_availability lookup failed userId=${userId} shopId=${id}:`, e);
    }

    res.status(200).json({
        success: true,
        data: {
            ...details,
            userActions: {
                hasReceivedCigar,
                lastReceivedAt
            }
        },
    });
}

export async function handleCityShops(req: Request, res: Response): Promise<void> {
    if (!validateAuthToken(req.headers['authorization'])) {
        res.status(401).json({
            success: false,
            error: "Unauthorized",
            message: "Invalid or missing token"
        });
        return;
    }

    const city = req.params.city as string | undefined;

    if (!city) {
        res.status(400).json({
            success: false,
            error: "Invalid parameters",
            message: "city is required"
        });
        return;
    }

    const result = await shopAccess.getShopIdsInCity(city);

    res.status(200).json({
        success: true,
        data: result,
    })
}

export async function handleReceivedCigar(req: Request, res: Response): Promise<void> {
    if (!validateAuthToken(req.headers['authorization'])) {
        res.status(401).json({
            success: false,
            error: "Unauthorized",
            message: "Invalid or missing token"
        });
        return;
    }

    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1];
    if (!token) {
        res.status(401).json({
            success: false,
            error: "Unauthorized",
            message: "Missing token"
        });
        return;
    }

    let payload: any;
    try {
        payload = jwt.verify(token, config.jwt.secret) as any;
    } catch {
        res.status(401).json({
            success: false,
            error: "Unauthorized",
            message: "Invalid token"
        });
        return;
    }

    const reqId = req.params.id as string | undefined;
    if (!reqId) {
        res.status(400).json({
            success: false,
            error: "Invalid parameters",
            message: "Shop id missing"
        });
        return;
    }

    const shopId = parseInt(reqId, 10);
    if (Number.isNaN(shopId)) {
        res.status(400).json({
            success: false,
            error: "Invalid parameters",
            message: "Shop id must be a number"
        });
        return;
    }

    // Validate shop exists
    const details = await shopAccess.getShopDetails(shopId);
    if (!details) {
        res.status(404).json({
            success: false,
            error: "Not Found",
            message: `Shop with id ${shopId} not found`
        });
        return;
    }

    const userId = payload.userId;

    try {
        const { id, createdAt } = await userActionsAccess.recordReceivedCigar(userId, shopId);
        res.status(201).json({
            success: true,
            data: {
                id,
                userId,
                shopId,
                receivedAt: createdAt
            }
        });
    } catch (err: any) {
        console.error(`[RECEIVED_CIGAR] insert failed userId=${userId} shopId=${shopId}:`, err);
        res.status(500).json({
            success: false,
            error: "Server Error",
            message: "Failed to record received cigar"
        });
    }
}

function validateAuthToken(authHeader: string | undefined): boolean {
    try {
        const token = authHeader && authHeader.split(' ')[1];
        if (!token) {
            return false;
        }
        jwt.verify(token, config.jwt.secret);

        return true;
    } catch (error) {
        return false;
    }
}
