import pool from './db';

/**
 * 기록: user got/received cigars at a shop.
 * Uses existing table: public.cigar_availability
 */
async function recordReceivedCigar(userId: number, shopId: number): Promise<{ id: number; createdAt: string }> {
  const query = `
    INSERT INTO cigar_availability (shop_id, user_id, confirmed, created_at)
    VALUES ($1, $2, false, now())
    RETURNING id, created_at AS "createdAt";
  `;

  const values = [shopId, userId];
  const result = await pool.query<{ id: number; createdAt: string }>(query, values);
  const row = result.rows[0];
  if (!row) {
    throw new Error('Failed to record received cigar (no row returned)');
  }
  return { id: row.id, createdAt: row.createdAt };
}

async function hasReceivedCigar(userId: number, shopId: number): Promise<boolean> {
  const query = `
    SELECT 1
    FROM cigar_availability
    WHERE shop_id = $1 AND user_id = $2
    LIMIT 1;
  `;
  const result = await pool.query(query, [shopId, userId]);
  return (result.rowCount ?? 0) > 0;
}

async function getLastReceivedCigarAt(userId: number, shopId: number): Promise<string | null> {
  const query = `
    SELECT created_at AS "createdAt"
    FROM cigar_availability
    WHERE shop_id = $1 AND user_id = $2
    ORDER BY created_at DESC
    LIMIT 1;
  `;
  const result = await pool.query<{ createdAt: string }>(query, [shopId, userId]);
  return result.rows[0]?.createdAt ?? null;
}

export default {
  recordReceivedCigar,
  hasReceivedCigar,
  getLastReceivedCigarAt
};
