import shopAccess from "../../db/shopAccess";
import {Request, Response} from "express";


export async function getAllCities(req : Request, res : Response): Promise<void> {
    res.send({
        cities : await shopAccess.getCities()
    })
}

export async function getCityAllShopLocations(req : Request, res : Response): Promise<void> {

    const city = req.params.city;

    if(!city){
        res.send({
            error: "City not found",
        })
        return;
    }

    const shopCoords = await shopAccess.getShopCoordsByCity(city);

    res.send(shopCoords);
}
