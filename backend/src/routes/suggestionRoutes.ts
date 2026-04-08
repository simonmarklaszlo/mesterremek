import { Router } from "express";
import {
  handleListSuggestions,
  handleGetSuggestionById,
  handleCreateSuggestion,
  handleVoteSuggestion,
  handleRemoveVote,
  handleApplySuggestion,
  handleDeleteSuggestion,
} from "./func/suggestion";
import { requireAuth } from "../middleware/auth";

const router = Router();

router.get("/", requireAuth, handleListSuggestions);
router.get("/:id", requireAuth, handleGetSuggestionById);
router.post("/", requireAuth, handleCreateSuggestion);
router.post("/:id/vote", requireAuth, handleVoteSuggestion);
router.delete("/:id/vote", requireAuth, handleRemoveVote);
router.post("/:id/apply", requireAuth, handleApplySuggestion);
router.delete("/:id", requireAuth, handleDeleteSuggestion);

export default router;
