import { Router } from "express";
import {getAllCities, getCityAllShopLocations} from "./func/city";

const router = Router();

router.get("/", getAllCities);
router.get("/:city", getCityAllShopLocations);



export default router;