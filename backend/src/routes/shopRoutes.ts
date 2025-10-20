import {Router} from "express";
import {routeShopAll, routeShopCoords, routeShopDetails, routeShopSchedules, shopSearch} from "./func/shop"


const router = Router();

router.get("/:id", routeShopAll);
router.get("/:id/coords", routeShopCoords);
router.get("/:id/details", routeShopDetails);
router.get("/:id/schedule", routeShopSchedules)

router.get("/search", shopSearch)

export default router;