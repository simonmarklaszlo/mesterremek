import {Router} from "express";
import {handleCityShops, handleReceivedCigar, handleShopDetails, handleShopSearch} from "./func/shop"

const router = Router();

router.get("/search", handleShopSearch)
router.get("/cities/:city", handleCityShops)
router.post("/:id/received-cigar", handleReceivedCigar)
router.get("/:id", handleShopDetails)

export default router;