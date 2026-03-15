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

const router = Router();

router.get("/", handleListSuggestions);
router.get("/:id", handleGetSuggestionById);
router.post("/", handleCreateSuggestion);
router.post("/:id/vote", handleVoteSuggestion);
router.delete("/:id/vote", handleRemoveVote);
router.post("/:id/apply", handleApplySuggestion);
router.delete("/:id", handleDeleteSuggestion);

export default router;
