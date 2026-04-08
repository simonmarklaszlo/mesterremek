import type { Request, Response, NextFunction } from "express";
import jwt from "jsonwebtoken";

import { config } from "../config/config";
import type { JwtPayload } from "../model/auth";

export interface AuthenticatedRequest extends Request {
  user?: JwtPayload;
}

function extractBearerToken(authHeader: string | undefined): string | null {
  if (!authHeader) return null;
  const [scheme, token] = authHeader.split(" ");
  if (!scheme || scheme.toLowerCase() !== "bearer") return null;
  if (!token) return null;
  return token;
}

/**
 * Validates JWT from `Authorization: Bearer <token>`.
 * On success attaches the decoded payload to `req.user`.
 */
export function requireAuth(req: AuthenticatedRequest, res: Response, next: NextFunction): void {
  try {
    const token = extractBearerToken(req.header("authorization"));
    if (!token) {
      res.status(401).json({ success: false, error: "Unauthorized", message: "Missing token" });
      return;
    }

    const payload = jwt.verify(token, config.jwt.secret) as JwtPayload;
    if (!payload) {
      res.status(401).json({ success: false, error: "Unauthorized", message: "Invalid token payload" });
      return;
    }

    req.user = payload;
    next();
  } catch {
    res.status(401).json({ success: false, error: "Unauthorized", message: "Invalid or expired token" });
  }
}

/**
 * Optional role gate. Use as: `router.post("/x", requireAuth, requireRole("admin"), handler)`.
 */
// Helper for future use (e.g. admin-only endpoints)
export function requireRole(role: string) {
  return (req: AuthenticatedRequest, res: Response, next: NextFunction): void => {
    const userRole = req.user?.role;
    if (!userRole || userRole !== role) {
      res.status(403).json({ success: false, error: "Forbidden", message: "Insufficient permissions" });
      return;
    }
    next();
  };
}


