import pool from "./db";

export type VoteType = "like" | "dislike";

export interface SuggestionListParams {
  filter: "all" | "own";
  statusCode?: string;
  typeCode?: string;
  shopId?: number;
  currentUserId: number;
}

export interface CreateSuggestionParams {
  typeCode: string;
  shopId: number | null;
  proposedValue: string;
  additionalData: any | null;
  userId: number;
}

export interface SuggestionRow {
  id: number;
  type_id: number;
  type_code: string;
  type_name: string;
  status_id: number;
  status_code: string;
  status_name: string;
  shop_id: number | null;
  shop_name: string | null;
  shop_address?: string | null;
  shop_city?: string | null;
  proposed_value: string;
  additional_data: any | null;
  user_id: number;
  user_name: string | null;
  created_at: string;
  updated_at: string;
  net_votes: number;
  user_vote: VoteType | null;
}

function mapSuggestion(r: SuggestionRow) {
  const normalize = (v: unknown): string | undefined => {
    if (v === null || v === undefined) return undefined;
    if (typeof v !== "string") return String(v);
    const trimmed = v.trim();
    if (!trimmed) return undefined;
    if (trimmed.toUpperCase() === "NULL") return undefined;
    return trimmed;
  };

  const shopName = normalize(r.shop_name) ?? "Traffik";

  return {
    id: r.id,
    typeId: r.type_id,
    typeCode: r.type_code,
    typeName: r.type_name,
    statusId: r.status_id,
    statusCode: r.status_code,
    statusName: r.status_name,
    shopId: r.shop_id ?? undefined,
    shopName,
    shopAddress: normalize((r as any).shop_address),
    shopCity: normalize((r as any).shop_city),
    proposedValue: r.proposed_value,
    additionalData: r.additional_data,
    userId: r.user_id,
    userName: normalize(r.user_name) ?? undefined,
    createdAt: r.created_at,
    netVotes: Number(r.net_votes ?? 0),
    userVote: r.user_vote,
  };
}

async function getUserVote(suggestionId: number, userId: number): Promise<VoteType | null> {
  const r = await pool.query(
    `SELECT vote_type FROM suggestion_votes WHERE suggestion_id = $1 AND user_id = $2`,
    [suggestionId, userId]
  );
  return r.rows[0]?.vote_type ?? null;
}

const suggestionAccess = {
  async listSuggestions(params: SuggestionListParams) {
    const where: string[] = [];
    const values: any[] = [];

    if (params.filter === "own") {
      values.push(params.currentUserId);
      where.push(`user_id = $${values.length}`);
    }

    if (params.statusCode) {
      values.push(params.statusCode);
      where.push(`status_code = $${values.length}`);
    }

    if (params.typeCode) {
      values.push(params.typeCode);
      where.push(`type_code = $${values.length}`);
    }

    if (typeof params.shopId === "number") {
      values.push(params.shopId);
      where.push(`shop_id = $${values.length}`);
    }

    const sql = `
      SELECT
        s.id,
        s.type_id,
        s.type_code,
        s.type_name,
        s.status_id,
        s.status_code,
        s.status_name,
        s.shop_id,
        COALESCE(s.shop_name, sh.name) AS shop_name,
        COALESCE(s.shop_address, sh.address) AS shop_address,
        COALESCE(s.shop_city, sh.city) AS shop_city,
        s.proposed_value,
        s.additional_data,
        s.user_id,
        s.user_name,
        s.created_at,
        s.updated_at,
        s.net_votes
      FROM suggestions_with_votes s
      LEFT JOIN shops sh ON sh.id = s.shop_id
      ${where.length ? `WHERE ${where.join(" AND ")}` : ""}
      ORDER BY s.created_at DESC
    `;

    const result = await pool.query(sql, values);

    const mapped = await Promise.all(
      result.rows.map(async (row: any) => {
        const userVote = await getUserVote(row.id, params.currentUserId);
        return mapSuggestion({ ...row, user_vote: userVote } as SuggestionRow);
      })
    );

    return mapped;
  },

  async getSuggestionById(id: number, currentUserId: number) {
    const r = await pool.query(
      `SELECT
        s.id,
        s.type_id,
        s.type_code,
        s.type_name,
        s.status_id,
        s.status_code,
        s.status_name,
        s.shop_id,
        COALESCE(s.shop_name, sh.name) AS shop_name,
        COALESCE(s.shop_address, sh.address) AS shop_address,
        COALESCE(s.shop_city, sh.city) AS shop_city,
        s.proposed_value,
        s.additional_data,
        s.user_id,
        s.user_name,
        s.created_at,
        s.updated_at,
        s.net_votes
      FROM suggestions_with_votes s
      LEFT JOIN shops sh ON sh.id = s.shop_id
      WHERE s.id = $1`,
      [id]
    );

    const row = r.rows[0];
    if (!row) return null;

    const userVote = await getUserVote(id, currentUserId);
    return mapSuggestion({ ...row, user_vote: userVote } as SuggestionRow);
  },

  async createSuggestion(params: CreateSuggestionParams) {
    const typeRes = await pool.query(`SELECT id FROM suggestion_types WHERE code = $1`, [params.typeCode]);
    const typeId = typeRes.rows[0]?.id;
    if (!typeId) throw new Error(`Unknown suggestion type: ${params.typeCode}`);

    const insertRes = await pool.query(
      `INSERT INTO suggestions (type_id, shop_id, proposed_value, additional_data, user_id, status_id)
       VALUES ($1, $2, $3, $4::jsonb, $5, (SELECT id FROM suggestion_statuses WHERE code = 'pending'))
       RETURNING id`,
      [typeId, params.shopId, params.proposedValue, params.additionalData ? JSON.stringify(params.additionalData) : null, params.userId]
    );

    const id = insertRes.rows[0]?.id;
    return this.getSuggestionById(id, params.userId);
  },

  async upsertVote(params: { suggestionId: number; userId: number; voteType: VoteType }) {
    await pool.query(
      `INSERT INTO suggestion_votes (suggestion_id, user_id, vote_type)
       VALUES ($1, $2, $3)
       ON CONFLICT (suggestion_id, user_id)
       DO UPDATE SET vote_type = EXCLUDED.vote_type, updated_at = NOW()`,
      [params.suggestionId, params.userId, params.voteType]
    );
  },

  async removeVote(params: { suggestionId: number; userId: number }) {
    await pool.query(
      `DELETE FROM suggestion_votes WHERE suggestion_id = $1 AND user_id = $2`,
      [params.suggestionId, params.userId]
    );
  },

  async deleteSuggestion(id: number, userId: number) {
    const r = await pool.query(`DELETE FROM suggestions WHERE id = $1 AND user_id = $2`, [id, userId]);
    return (r.rowCount ?? 0) > 0;
  },

  async applySuggestion(id: number) {
    // Only approved suggestions can be applied
    const r = await pool.query(
      `SELECT s.*
       FROM suggestions_with_votes s
       WHERE s.id = $1`,
      [id]
    );
    const suggestion = r.rows[0];
    if (!suggestion) throw new Error("Suggestion not found");
    if (suggestion.status_code !== "approved") throw new Error("Only approved suggestions can be applied");

    // Apply based on type_code
    if (suggestion.type_code === "edit_name") {
      await pool.query(`UPDATE shops SET name = $1 WHERE id = $2`, [suggestion.proposed_value, suggestion.shop_id]);
    } else if (suggestion.type_code === "edit_address") {
      const city = suggestion.additional_data?.city;
      await pool.query(`UPDATE shops SET address = $1, city = COALESCE($2, city) WHERE id = $3`, [suggestion.proposed_value, city ?? null, suggestion.shop_id]);
    } else if (suggestion.type_code === "edit_hours") {
      // Expect additional_data.openingHours: [{dayOfWeek, openHour, closeHour}]
      const openingHours = suggestion.additional_data?.openingHours;
      if (!Array.isArray(openingHours)) throw new Error("Missing openingHours in additionalData");

      // Map day name -> day_id from days_of_week
      const daysRes = await pool.query(`SELECT id, name FROM days_of_week`);
      const nameToId = new Map<string, number>(daysRes.rows.map((d: any) => [d.name, d.id]));

      // Delete existing opening hours and insert new
      await pool.query(`DELETE FROM opening_hours WHERE shop_id = $1`, [suggestion.shop_id]);

      for (const oh of openingHours) {
        const dayId = nameToId.get(oh.dayOfWeek);
        if (!dayId) continue;
        const open = oh.openHour === "Zárva" ? null : oh.openHour;
        const close = oh.openHour === "Zárva" ? null : oh.closeHour;
        await pool.query(
          `INSERT INTO opening_hours (shop_id, day_id, open_hour, close_hour)
           VALUES ($1, $2, $3::time, $4::time)`,
          [suggestion.shop_id, dayId, open, close]
        );
      }
    } else if (suggestion.type_code === "new_shop") {
      // Minimal implement: create shop if your schema supports fields in additional_data
      // Here we require: {address, city, latitude, longitude}
      const ad = suggestion.additional_data ?? {};
      const name = suggestion.proposed_value;
      const address = ad.address;
      const city = ad.city;
      const latitude = ad.latitude;
      const longitude = ad.longitude;
      if (!address || !city || latitude === undefined || longitude === undefined) {
        throw new Error("Missing required additionalData for new_shop");
      }

      // Try to insert into shops. This assumes shops has name,address,city,latitude,longitude columns.
      await pool.query(
        `INSERT INTO shops (name, address, city, latitude, longitude)
         VALUES ($1, $2, $3, $4, $5)`,
        [name, address, city, latitude, longitude]
      );
    }

    // Mark as applied
    await pool.query(
      `UPDATE suggestions
       SET status_id = (SELECT id FROM suggestion_statuses WHERE code = 'applied'),
           updated_at = NOW()
       WHERE id = $1`,
      [id]
    );

    return { id };
  },
};

export default suggestionAccess;

