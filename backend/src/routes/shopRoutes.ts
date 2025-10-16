import { Router } from "express";
import {routeShopAll, routeShopCoords, routeShopDetails, routeShopSchedules} from "./shopRoutes.func"


const router = Router();

router.get("/:id", routeShopAll);
router.get("/:id/coords", routeShopCoords);
router.get("/:id/details", routeShopDetails);
router.get("/:id/schedule", routeShopSchedules)

export default router;