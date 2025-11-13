import pool from './db';
import {ShopFilter, ShopFilterResult, ShopDetails, Shop} from "../model/shop";
import {Review} from "../model/review";
import {CigarBrand} from "../model/cigarBrand";
import {OpeningHour} from "../model/openingHour";


async function getShopPage(filter: ShopFilter): Promise<ShopFilterResult[]> {
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

    const result = await pool.query<ShopFilterResult>(query, values);

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

async function getShopIdsInCity(city: string): Promise<number[]> {
    const query = `
        SELECT id
        FROM shops s
        WHERE city = $1
    `;

    const values = [city];

    const result = await pool.query<{ id: number }>(query, values);

    return result.rows.map(x => x.id);
}

async function getShopDetails(id: number): Promise<ShopDetails | undefined> {
    const shop = await getShop(id);
    if (!shop) return undefined;
    const reviews = await getReviews(id);
    const cigarBrands = await getCigarBrands(id);
    const openingHours = await getOpeningHours(id);

    const averageRating = reviews.length != 0 ?
        reviews.reduce((sum, review) => sum + review.rating, 0) / reviews.length :
        0;
    const {isOpenNow, nextClosingTime} = getNextClosingTime(openingHours);
    return {
        ...shop,
        reviews,
        cigarBrands,
        openingHours,
        averageRating,
        totalReviews: reviews.length,
        nextClosingTime,
        isOpenNow
    }

    function getNextClosingTime(openingHours: OpeningHour[]): { nextClosingTime: string | null, isOpenNow: boolean } {
        const now = new Date();
        const dayStr = getDay(now);

        const schedule = openingHours.find(x => x.dayOfWeek === dayStr);
        if (!schedule) {
            return {
                nextClosingTime: null,
                isOpenNow: false,
            }
        }
        const year = now.getFullYear();
        const month = String(now.getMonth() + 1).padStart(2, "0");
        const day = String(now.getDate()).padStart(2, "0");
        const dateStr = `${year}-${month}-${day}`;

        const opening = new Date(`${dateStr}T${schedule.openHour}`);
        const closing = new Date(`${dateStr}T${schedule.closeHour}`);

        if (opening <= now && now <= closing) {
            return {
                nextClosingTime: schedule.closeHour,
                isOpenNow: true,
            }
        }
        if (opening > now) {
            return {
                nextClosingTime: schedule.closeHour,
                isOpenNow: false,
            }
        }

        return {
            nextClosingTime: null,
            isOpenNow: false,
        }
    }

    function getDay(d: Date): 'Hétfő' | 'Kedd' | 'Szerda' | 'Csütörtök' | 'Péntek' | 'Szombat' | 'Vasárnap' {
        const todayDay = d.getDay();
        switch (todayDay) {
            case 0:
                return 'Vasárnap';
            case 1:
                return 'Hétfő';
            case 2:
                return 'Kedd';
            case 3:
                return 'Szerda';
            case 4:
                return 'Csütörtök';
            case 5:
                return 'Péntek';
            case 6:
                return 'Szombat';
            default:
                throw new Error(`Nincs ilyen nap: ${todayDay}`);
        }
    }
}

async function getShop(id: number): Promise<Shop | undefined> {
    const query = `
        SELECT id,
               name,
               address,
               city,
               ST_X(s.location) AS "longitude",
               ST_Y(s.location) AS "latitude",
               created_at       AS "createdAt",
               updated_at       AS "updatedAt"
        FROM shops s
        WHERE id = $1
    `;

    const values = [id];

    const result = await pool.query<Shop>(query, values);
    return result.rows[0];
}

async function getReviews(shopId: number): Promise<Review[]> {
    const query = `
        SELECT r.id,
               r.user_id    AS "userId",
               u.name       AS "userName",
               r.rating,
               r.comment,
               r.created_at AS "createdAt"
        FROM reviews r
                 JOIN users u ON u.id = r.user_id
        WHERE r.shop_id = $1
        ORDER BY r.created_at DESC
        LIMIT 50
    `;

    const values = [shopId];

    const result = await pool.query<Review>(query, values);
    return result.rows;
}

async function getCigarBrands(shopId: number): Promise<CigarBrand[]> {
    const query = `
        SELECT scb.id,
               scb.cigar_id   AS "brandId",
               cb.name        AS "brandName",
               scb.added_by   As "addedBy",
               scb.created_at AS "createdAt"
        FROM shop_cigar_brands scb
                 JOIN cigar_brands cb ON cb.id = scb.cigar_id
        WHERE scb.shop_id = $1
        ORDER BY cb.name
    `;

    const values = [shopId];

    const result = await pool.query<CigarBrand>(query, values);
    return result.rows;
}

async function getOpeningHours(shopId: number): Promise<OpeningHour[]> {
    const query = `
        SELECT soh.id         AS "id",
               dow.day        AS "dayOfWeek",
               soh.open_hour  AS "openHour",
               soh.close_hour AS "closeHour"
        FROM shop_opening_hours soh
                 JOIN days_of_week dow ON dow.id = soh.day_id
        WHERE soh.shop_id = $1
        ORDER BY soh.day_id
    `;

    const values = [shopId];

    const result = await pool.query<OpeningHour>(query, values);
    return result.rows;
}

export default {
    getShopPage,
    countShopPages,
    getShopDetails,
    getShopIdsInCity
}