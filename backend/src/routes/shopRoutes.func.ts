import shopAccess from "../db/shopAccess";
import {Request, Response} from "express";

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
