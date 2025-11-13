import {Router} from "express";
import {handleCityShops, handleShopDetails, handleShopSearch} from "./func/shop"

const router = Router();

router.get("/search", handleShopSearch)
router.get("/:id", handleShopDetails)
router.get("/cities/:city", handleCityShops)

export default router;