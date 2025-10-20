import pool from './db';
import {
    ShopCoords,
    ShopDetails,
    DaySchedule,
    getErrorCoords,
    getErrorDetails,
    getErrorSchedule,
    ShopFilter, Shop
} from "../model/shop";


async function getCities(): Promise<string[]> {
    const query = `
        SELECT DISTINCT city
        FROM shops
    `;

    try {
        const result = await pool.query<{ city: string }>(query);
        return result.rows.map(row => row.city);
    } catch (err) {
        console.error('Error querying cities:', err);
        return [];
    }
}

async function getShopCoordsByCity(city: string): Promise<{ id: number, coords: ShopCoords }[]> {
    const query = `
        SELECT id,
               ST_X(location) AS longitude,
               ST_Y(location) AS latitude
        FROM shops
        WHERE city = $1
    `;

    const values = [city];

    try {
        const result = await pool.query<{ id: number, longitude: number, latitude: number }>(query, values);

        return result.rows.map(row => ({
            id: row.id,
            coords: {
                longitude: row.longitude,
                latitude: row.latitude
            }
        }));

    } catch (err) {
        console.error('Error querying shops:', err);
        return [];
    }
}

async function getShopCoords(id: number): Promise<ShopCoords> {
    const query = `
        SELECT ST_X(location) AS longitude,
               ST_Y(location) AS latitude
        FROM shops
        WHERE id = $1
    `;

    const values = [id];

    try {
        const result = await pool.query<{ longitude: number, latitude: number }>(query, values);

        if (result.rows[0] == null) {
            return getErrorCoords();
        }

        return {
            longitude: result.rows[0].longitude,
            latitude: result.rows[0].latitude
        }
    } catch (err) {
        console.error('Error querying shops:', err);
        return getErrorCoords();
    }
}

async function getShopDetails(id: number): Promise<ShopDetails> {
    const query = `
        SELECT name,
               address
        FROM shops
        WHERE id = $1
    `;

    const values = [id];

    try {
        const result = await pool.query<{ name: string | null, address: string }>(query, values);

        if (result.rows[0] == null) {
            return getErrorDetails()
        }

        return {
            name: result.rows[0].name,
            address: result.rows[0].address
        }
    } catch (err) {
        console.error('Error querying shops:', err);
        return getErrorDetails()
    }
}

async function getShopSchedules(id: number): Promise<DaySchedule[]> {
    const query = `
        SELECT days_of_week.day, open_hour, close_hour
        FROM shop_opening_hours
                 JOIN days_of_week ON shop_opening_hours.day_id = days_of_week.id
        WHERE shop_opening_hours.shop_id = $1
    `;

    const values = [id];

    try {
        const result = await pool.query<{ day: string, open_hour: string, close_hour: string }>(query, values);

        return result.rows.map(row => ({
            dayOfWeek: row.day,
            opening: row.open_hour,
            closing: row.close_hour,
        }));

    } catch (err) {
        console.error('Error querying shops:', err);
        return getErrorSchedule();
    }
}

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
        filter.maxDistance * 1000,
        filter.hasCigars,
        filter.search,
        filter.itemsPerPage,
        filter.pageOffset
    ];

    const result = await pool.query<Shop>(query, values);

    return result.rows.map(x => x);
}

async function countShopPages(filter : ShopFilter): Promise<number> {
const query = `
        SELECT COUNT(s.id) AS count
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
    `;

    const values = [
        filter.latitude,
        filter.longitude,
        filter.maxDistance * 1000,
        filter.hasCigars,
        filter.search,
        filter.itemsPerPage,
        filter.pageOffset
    ];

    const result = await pool.query<{count : string}>(query, values);

    return parseInt(result.rows[0]?.count ?? '0', 10);
}

export default {
    getCities,
    getShopCoordsByCity,
    getShopCoords,
    getShopDetails,
    getShopSchedules,

    getShopPage,
    countShopPages,
}