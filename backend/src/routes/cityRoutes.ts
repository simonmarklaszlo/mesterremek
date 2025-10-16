import { Router } from "express";
import shopAccess from "../db/shopAccess";

const router = Router();

router.get("/", async (req, res) => {
    res.send({
        cities : await shopAccess.getCities()
    })
});

router.get("/:city", async (req, res) => {
    const city = req.params.city;

    const shopCoords = await shopAccess.getShopCoordsByCity(city);

    res.send(shopCoords);
});



export default router;