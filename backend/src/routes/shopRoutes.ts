import {Router} from "express";
import {handleShopSearch} from "./func/shop"

const router = Router();

router.get("/search", handleShopSearch)

export default router;