import pool from './db';
import {ShopFilter, Shop} from "../model/shop";


async function getShopPage(filter: ShopFilter): Promise<Shop[]> {
    const query = `
        SELECT s.id                       AS id,
               s.name                     AS name,
               s.address                  AS address,
               s.city                     AS city,
               ST_X(s.location)           AS longitude,
               ST_Y(s.location)           AS latitude,
               CASE
                   WHEN COUNT(DISTINCT c.id) > 0 THEN true
                   ELSE false
                   END                    as hasCigars,
               (
                   6371 * acos(
                           cos(radians($1)) * cos(radians(ST_Y(s.location))) *
                           cos(radians(ST_X(s.location)) - radians($2)) +
                           sin(radians($1)) * sin(radians(ST_Y(s.location)))
                          )
                   )                      AS distance,
               COALESCE(AVG(r.rating), 0) AS rating,
               COUNT(DISTINCT r.id)       AS reviewCount
        FROM shops s
                 LEFT JOIN shop_cigar_brands scb ON scb.shop_id = s.id
                 LEFT JOIN cigars c ON c.id = scb.cigar_id
                 LEFT JOIN reviews r ON r.shop_id = s.id
        WHERE
          -- Távolság szűrés (előszűrés négyzet alapon - gyorsabb)
            ST_Y(s.location) BETWEEN $1 - ($3 / 111.0) AND $1 + ($3 / 111.0)
          AND ST_X(s.location) BETWEEN $2 - ($3 / (111.0 * cos(radians($1)))) AND $2 + ($3 / (111.0 * cos(radians($1))))
          -- Keresés szűrés (opcionális)
          AND (
            $5::text IS NULL
                OR s.name ILIKE '%' || $5 || '%'
                OR s.address ILIKE '%' || $5 || '%'
                OR s.city ILIKE '%' || $5 || '%'
            )
        GROUP BY s.id, s.name, s.address, s.city, ST_Y(s.location), ST_X(s.location)
        HAVING
           -- Pontos távolság szűrés
            (
                6371 * acos(
                        cos(radians($1)) * cos(radians(ST_Y(s.location))) *
                        cos(radians(ST_X(s.location)) - radians($2)) +
                        sin(radians($1)) * sin(radians(ST_Y(s.location)))
                       )
                ) <= $3
           -- Szivar szűrés (opcionális) - csak azok a boltok, ahol van szivar hozzárendelve
           AND ($4::boolean IS NULL OR
                ($4 = true AND COUNT(DISTINCT c.id) > 0) OR
                ($4 = false AND COUNT(DISTINCT c.id) = 0))
        ORDER BY distance
        LIMIT $6 OFFSET $7;
    `;

    const values = [
        filter.latitude,
        filter.longitude,
        filter.maxDistanceKm,
        filter.hasCigars,
        filter.search,
        filter.itemsPerPage,
        filter.pageOffset
    ];

    const result = await pool.query<Shop>(query, values);

    return result.rows.map(x => x);
}

async function countShopPages(filter: ShopFilter): Promise<number> {
    const query = `
        SELECT COUNT(*) AS totalCount
        FROM (SELECT s.id
              FROM shops s
                       LEFT JOIN shop_cigar_brands scb ON scb.shop_id = s.id
                       LEFT JOIN cigars c ON c.id = scb.cigar_id
              WHERE
                -- Bounding box pre-filter (fast)
                  ST_Y(s.location) BETWEEN $1 - ($3 / 111.0) AND $1 + ($3 / 111.0)
                AND ST_X(s.location) BETWEEN $2 - ($3 / (111.0 * cos(radians($1)))) AND $2 + ($3 / (111.0 * cos(radians($1))))
                -- Optional search filter
                AND (
                  $5::text IS NULL
                      OR s.name ILIKE '%' || $5 || '%'
                      OR s.address ILIKE '%' || $5 || '%'
                      OR s.city ILIKE '%' || $5 || '%'
                  )
              GROUP BY s.id, s.location
              HAVING
                 -- Exact distance filter
                  (
                      6371 * acos(
                              cos(radians($1)) * cos(radians(ST_Y(s.location))) *
                              cos(radians(ST_X(s.location)) - radians($2)) +
                              sin(radians($1)) * sin(radians(ST_Y(s.location)))
                             )
                      ) <= $3
                 -- Optional cigar filter
                 AND ($4::boolean IS NULL
                  OR ($4 = true AND COUNT(DISTINCT c.id) > 0)
                  OR ($4 = false AND COUNT(DISTINCT c.id) = 0)
                  )) AS filtered_shops;
    `;

    const values = [
        filter.latitude,
        filter.longitude,
        filter.maxDistanceKm,
        filter.hasCigars,
        filter.search,
    ];

    const result = await pool.query<{ totalcount: string }>(query, values);

    return parseInt(result.rows[0]?.totalcount ?? '0', 10);
}

export default {
    getShopPage,
    countShopPages,
}