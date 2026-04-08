import { Request, Response } from "express";
import suggestionAccess from "../../db/suggestionAccess";
import type { AuthenticatedRequest } from "../../middleware/auth";

export async function handleListSuggestions(req: Request, res: Response): Promise<void> {
  const userId = (req as AuthenticatedRequest).user!.userId;
  const filter = (req.query.filter as string | undefined) ?? "all";
  const status = req.query.status as string | undefined;
  const type = req.query.type as string | undefined;
  const shopId = req.query.shopId ? parseInt(req.query.shopId as string) : undefined;

  const normalizedFilter = filter === "own" ? "own" : "all";

  // Default behavior:
  // - public list (filter=all): only pending suggestions are shown unless client overrides via ?status=
  // - own list (filter=own): show all statuses by default
  // Desired behavior:
  // - public list (filter=all): ALWAYS pending (ignore ?status=)
  // - own list (filter=own): show all statuses by default, but allow ?status=
  const effectiveStatus = normalizedFilter === "all" ? "pending" : status;

  const suggestions = await suggestionAccess.listSuggestions({
    filter: normalizedFilter,
    ...(effectiveStatus ? { statusCode: effectiveStatus } : {}),
    ...(type ? { typeCode: type } : {}),
    ...(typeof shopId === "number" ? { shopId } : {}),
    currentUserId: userId,
  });

  // Debug: gyors sanity check, hogy jön-e shopName a backendből
  try {
    const sample = suggestions.slice(0, 5).map((s: any) => ({
      id: s.id,
      typeCode: s.typeCode,
      shopId: s.shopId,
      shopName: s.shopName,
      shopAddress: s.shopAddress,
      shopCity: s.shopCity,
      proposedValue: s.proposedValue,
      statusCode: s.statusCode,
    }));
    console.log("[handleListSuggestions] sample:", sample);
  } catch {
    // ignore logging errors
  }

  res.status(200).json({ success: true, suggestions, total: suggestions.length });
}

export async function handleGetSuggestionById(req: Request, res: Response): Promise<void> {
  const id = parseInt(req.params.id ?? "", 10);
  if (!Number.isFinite(id)) {
    res.status(400).json({ success: false, message: "Invalid id" });
    return;
  }

  const userId = (req as AuthenticatedRequest).user!.userId;

  const suggestion = await suggestionAccess.getSuggestionById(id, userId);
  if (!suggestion) {
    res.status(404).json({ success: false, message: "Not found" });
    return;
  }

  res.status(200).json({ success: true, suggestion });
}

export async function handleCreateSuggestion(req: Request, res: Response): Promise<void> {
  const userId = (req as AuthenticatedRequest).user!.userId;

  const { type, shopId, proposedValue, additionalData } = req.body ?? {};

  if (!type || !proposedValue) {
    res.status(400).json({ success: false, message: "type and proposedValue are required" });
    return;
  }

  const created = await suggestionAccess.createSuggestion({
    typeCode: type,
    shopId: shopId ?? null,
    proposedValue,
    additionalData: additionalData ?? null,
    userId,
  });

  res.status(201).json({ success: true, suggestion: created });
}

export async function handleVoteSuggestion(req: Request, res: Response): Promise<void> {
  const userId = (req as AuthenticatedRequest).user!.userId;

  const id = parseInt(req.params.id ?? "", 10);
  const { voteType } = req.body ?? {};
  if (!Number.isFinite(id) || (voteType !== "like" && voteType !== "dislike")) {
    res.status(400).json({ success: false, message: "Invalid id or voteType" });
    return;
  }

  await suggestionAccess.upsertVote({ suggestionId: id, userId, voteType });
  let suggestion = await suggestionAccess.getSuggestionById(id, userId);

  if (!suggestion) {
    res.status(404).json({ success: false, message: "Not found" });
    return;
  }

  // Auto-apply immediately when community threshold is reached.
  // DB trigger should flip status to 'approved' at net>=5, but we also allow pending+net>=5.
  // This keeps the flow simple: user votes -> suggestion becomes applied automatically.
  try {
    const status = (suggestion.statusCode ?? "").toLowerCase();
    const netVotes = Number((suggestion as any).netVotes ?? 0);
    if ((status === "approved" || status === "pending") && netVotes >= 5) {
      await suggestionAccess.applySuggestion(id);
      suggestion = await suggestionAccess.getSuggestionById(id, userId);
    }
  } catch (e: any) {
    // If apply fails because it's not eligible (race / trigger delay) we still return the vote result.
    // For other errors (schema issues, etc.) we'd rather not hide them completely.
    const msg = String(e?.message ?? "");
    if (!msg.includes("only be applied") && !msg.includes("not found")) {
      console.warn("[handleVoteSuggestion] auto-apply failed:", e);
    }
  }

  res.status(200).json({ success: true, suggestion });
}

export async function handleRemoveVote(req: Request, res: Response): Promise<void> {
  const userId = (req as AuthenticatedRequest).user!.userId;

  const id = parseInt(req.params.id ?? "", 10);
  if (!Number.isFinite(id)) {
    res.status(400).json({ success: false, message: "Invalid id" });
    return;
  }

  await suggestionAccess.removeVote({ suggestionId: id, userId });
  let suggestion = await suggestionAccess.getSuggestionById(id, userId);

  if (!suggestion) {
    res.status(404).json({ success: false, message: "Not found" });
    return;
  }

  // Edge case: if a vote removal still leaves the suggestion at/above threshold due to other votes,
  // we still want it auto-applied.
  try {
    const status = (suggestion.statusCode ?? "").toLowerCase();
    const netVotes = Number((suggestion as any).netVotes ?? 0);
    if ((status === "approved" || status === "pending") && netVotes >= 5) {
      await suggestionAccess.applySuggestion(id);
      suggestion = await suggestionAccess.getSuggestionById(id, userId);
    }
  } catch (e: any) {
    const msg = String(e?.message ?? "");
    if (!msg.includes("only be applied") && !msg.includes("not found")) {
      console.warn("[handleRemoveVote] auto-apply failed:", e);
    }
  }

  res.status(200).json({ success: true, suggestion });
}

export async function handleApplySuggestion(req: Request, res: Response): Promise<void> {
  const userId = (req as AuthenticatedRequest).user!.userId;

  const id = parseInt(req.params.id ?? "", 10);
  if (!Number.isFinite(id)) {
    res.status(400).json({ success: false, message: "Invalid id" });
    return;
  }

  try {
    const applied = await suggestionAccess.applySuggestion(id);
    const suggestion = await suggestionAccess.getSuggestionById(applied.id, userId);
    res.status(200).json({ success: true, suggestion });
  } catch (e: any) {
    res.status(400).json({ success: false, message: e?.message ?? "Could not apply suggestion" });
  }
}

export async function handleDeleteSuggestion(req: Request, res: Response): Promise<void> {
  const userId = (req as AuthenticatedRequest).user!.userId;

  const id = parseInt(req.params.id ?? "", 10);
  if (!Number.isFinite(id)) {
    res.status(400).json({ success: false, message: "Invalid id" });
    return;
  }

  const ok = await suggestionAccess.deleteSuggestion(id, userId);
  if (!ok) {
    res.status(404).json({ success: false, message: "Not found" });
    return;
  }

  res.status(204).send();
}
