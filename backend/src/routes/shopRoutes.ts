import {Router} from "express";
import {handleShopDetails, handleShopSearch} from "./func/shop"

const router = Router();

router.get("/search", handleShopSearch)
router.get("/:id", handleShopDetails)

export default router;