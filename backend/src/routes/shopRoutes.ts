import { Router } from "express";
import { handleCityShops, handleReceivedCigar, handleShopDetails, handleShopSearch } from "./func/shop";
import { requireAuth } from "../middleware/auth";

const router = Router();

router.get("/search", requireAuth, handleShopSearch);
router.get("/cities/:city", requireAuth, handleCityShops);
router.post("/:id/received-cigar", requireAuth, handleReceivedCigar);
router.get("/:id", requireAuth, handleShopDetails);

export default router;