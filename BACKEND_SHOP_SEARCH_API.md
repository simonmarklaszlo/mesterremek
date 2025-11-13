# Szivarbolt Lista API Endpoint Terv

## GET /api/shops/search

**Leírás:** Szivarboltok keresése és szűrése a felhasználó pozíciója és preferenciái alapján.

### Request Query Parameters:

| Paraméter | Típus | Kötelező | Leírás | Példa |
|-----------|-------|----------|--------|-------|
| `latitude` | number | ✅ | Felhasználó GPS szélessége | `47.4979` |
| `longitude` | number | ✅ | Felhasználó GPS hosszúsága | `19.0402` |
| `maxDistance` | number | ✅ | Max távolság km-ben | `10` |
| `hasCigars` | boolean | ❌ | Csak szivar kínálattal | `true` |
| `search` | string | ❌ | Keresési szöveg (név/cím) | `"nemzeti"` |
| `limit` | number | ❌ | Max találatok száma (default: 50) | `20` |
| `offset` | number | ❌ | Lapozáshoz (default: 0) | `0` |

### Példa Request:

```http
GET /api/shops/search?latitude=47.4979&longitude=19.0402&maxDistance=10&hasCigars=true&search=nemzeti&limit=20&offset=0
Authorization: Bearer <jwt_token>
```

### Response (200 OK):

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Nemzeti Dohánybolt",
      "address": "Andrássy út 42",
      "city": "Budapest",
      "latitude": 47.5028,
      "longitude": 19.0620,
      "distance": 0.8,
      "hasCigars": true,
      "rating": 4.5,
      "reviewCount": 23
    },
    {
      "id": 2,
      "name": "Tabán Dohány",
      "address": "Tabán utca 15",
      "city": "Budapest",
      "latitude": 47.4926,
      "longitude": 19.0354,
      "distance": 1.2,
      "hasCigars": true,
      "rating": 4.2,
      "reviewCount": 18
    }
  ],
  "total": 2,
  "limit": 20,
  "offset": 0,
  "hasMore": false
}
```

**Megjegyzés:** A `data` közvetlenül a Shop objektumok tömbje, nem egy `{ shops: [...] }` wrapper objektum.

### Response (400 Bad Request):

```json
{
  "success": false,
  "error": "Invalid parameters",
  "message": "latitude and longitude are required"
}
```

### Response (401 Unauthorized):

```json
{
  "success": false,
  "error": "Unauthorized",
  "message": "Invalid or missing token"
}
```

---

## Backend SQL Query példa (PostgreSQL):

```sql
SELECT 
  s.id,
  s.name,
  s.address,
  s.city,
  s.latitude,
  s.longitude,
  -- Szivar elérhetőség ellenőrzése (van-e hozzárendelve szivar a bolthoz)
  CASE WHEN COUNT(DISTINCT c.id) > 0 THEN true ELSE false END as has_cigars,
  -- Távolság számítás (Haversine formula)
  (
    6371 * acos(
      cos(radians($1)) * cos(radians(s.latitude)) * 
      cos(radians(s.longitude) - radians($2)) + 
      sin(radians($1)) * sin(radians(s.latitude))
    )
  ) AS distance,
  COALESCE(AVG(r.rating), 0) AS rating,
  COUNT(DISTINCT r.id) AS review_count
FROM shops s
LEFT JOIN shop_cigar_brands scb ON scb.shop_id = s.id
LEFT JOIN cigars c ON c.id = scb.cigar_id
LEFT JOIN reviews r ON r.shop_id = s.id
WHERE 
  -- Távolság szűrés (előszűrés négyzet alapon - gyorsabb)
  s.latitude BETWEEN $1 - ($3 / 111.0) AND $1 + ($3 / 111.0)
  AND s.longitude BETWEEN $2 - ($3 / (111.0 * cos(radians($1)))) AND $2 + ($3 / (111.0 * cos(radians($1))))
  -- Keresés szűrés (opcionális)
  AND (
    $5::text IS NULL 
    OR s.name ILIKE '%' || $5 || '%' 
    OR s.address ILIKE '%' || $5 || '%'
    OR s.city ILIKE '%' || $5 || '%'
  )
GROUP BY s.id, s.name, s.address, s.city, s.latitude, s.longitude
HAVING 
  -- Pontos távolság szűrés
  (
    6371 * acos(
      cos(radians($1)) * cos(radians(s.latitude)) * 
      cos(radians(s.longitude) - radians($2)) + 
      sin(radians($1)) * sin(radians(s.latitude))
    )
  ) <= $3
  -- Szivar szűrés (opcionális) - csak azok a boltok, ahol van szivar hozzárendelve
  AND ($4::boolean IS NULL OR 
       ($4 = true AND COUNT(DISTINCT c.id) > 0) OR 
       ($4 = false AND COUNT(DISTINCT c.id) = 0))
ORDER BY distance ASC
LIMIT $6 OFFSET $7;
```

**Paraméterek:**
- `$1` = latitude (felhasználó pozíció)
- `$2` = longitude (felhasználó pozíció)
- `$3` = maxDistance (km)
- `$4` = hasCigars (boolean vagy NULL) - ha true, csak olyan boltok, ahol van szivar hozzárendelve
- `$5` = search (string vagy NULL)
- `$6` = limit
- `$7` = offset

**FONTOS megjegyzések az adatbázis sémához:**

A `has_cigars` mező dinamikusan kerül kiszámításra a következő táblák alapján:
- `shops` → `shop_cigar_brands` (shop_id kapcsolat)
- `shop_cigar_brands` → `cigars` (cigar_id kapcsolat)

Ha egy bolthoz van legalább 1 szivar hozzárendelve a `shop_cigar_brands` táblán keresztül, akkor `has_cigars = true`.

**Adatbázis séma kapcsolatok:**
```
shops (id)
  ↓
shop_cigar_brands (shop_id, cigar_id)
  ↓
cigars (id, brand_id)
  ↓
cigar_brands (id, name)
```

---

## Backend TypeScript (Express) példa:

```typescript
// routes/shopRoutes.ts
import { Router } from 'express';
import { authMiddleware } from '../middleware/auth';
import { searchShops } from '../controllers/shopController';

const router = Router();

// Védett endpoint - JWT token szükséges
router.get('/search', authMiddleware, searchShops);

export default router;
```

```typescript
// controllers/shopController.ts
import { Request, Response } from 'express';
import { pool } from '../db';

export const searchShops = async (req: Request, res: Response) => {
  try {
    const {
      latitude,
      longitude,
      maxDistance,
      hasCigars,
      search,
      limit = 50,
      offset = 0
    } = req.query;

    // Validáció
    if (!latitude || !longitude || !maxDistance) {
      return res.status(400).json({
        success: false,
        error: 'Invalid parameters',
        message: 'latitude, longitude and maxDistance are required'
      });
    }

    const lat = parseFloat(latitude as string);
    const lng = parseFloat(longitude as string);
    const maxDist = parseFloat(maxDistance as string);
    const hasCigarsFilter = hasCigars === 'true' ? true : hasCigars === 'false' ? false : null;
    const searchText = search ? (search as string) : null;
    const limitNum = Math.min(parseInt(limit as string) || 50, 100); // Max 100
    const offsetNum = parseInt(offset as string) || 0;

    // SQL query végrehajtása
    const result = await pool.query(
      `[A FENTI SQL QUERY]`,
      [lat, lng, maxDist, hasCigarsFilter, searchText, limitNum, offsetNum]
    );

    // Total count lekérés (külön query - hasMore számításhoz)
    const countResult = await pool.query(
      `[UGYANAZ A QUERY, DE COUNT(*) és LIMIT/OFFSET nélkül]`,
      [lat, lng, maxDist, hasCigarsFilter, searchText]
    );

    const total = countResult.rows[0]?.count || 0;
    const hasMore = (offsetNum + limitNum) < total;

    return res.status(200).json({
      success: true,
      data: {
        shops: result.rows,
        total,
        limit: limitNum,
        offset: offsetNum,
        hasMore
      }
    });

  } catch (error) {
    console.error('Shop search error:', error);
    return res.status(500).json({
      success: false,
      error: 'Internal server error',
      message: 'Failed to search shops'
    });
  }
};
```

---

## Felhasználó pozíció megszerzése (Frontend):

A frontend-en a böngésző Geolocation API-t használjuk:

```typescript
// GeolocationService vagy közvetlenül a komponensben
navigator.geolocation.getCurrentPosition(
  (position) => {
    const latitude = position.coords.latitude;
    const longitude = position.coords.longitude;
    // API hívás ezekkel a koordinátákkal
  },
  (error) => {
    console.error('Geolocation error:', error);
    // Fallback: Budapest központ vagy user beállítás
    const latitude = 47.4979;
    const longitude = 19.0402;
  }
);
```

---

## Frontend API hívás példa:

```typescript
// services/shop.service.ts
searchShops(params: {
  latitude: number;
  longitude: number;
  maxDistance: number;
  hasCigars?: boolean;
  search?: string;
  limit?: number;
  offset?: number;
}): Observable<any> {
  const queryParams = new URLSearchParams();
  queryParams.append('latitude', params.latitude.toString());
  queryParams.append('longitude', params.longitude.toString());
  queryParams.append('maxDistance', params.maxDistance.toString());
  
  if (params.hasCigars !== undefined) {
    queryParams.append('hasCigars', params.hasCigars.toString());
  }
  
  if (params.search) {
    queryParams.append('search', params.search);
  }
  
  if (params.limit) {
    queryParams.append('limit', params.limit.toString());
  }
  
  if (params.offset) {
    queryParams.append('offset', params.offset.toString());
  }

  return this.http.get(`${environment.apiUrl}/shops/search?${queryParams.toString()}`);
}
```

---

## Előnyök:

1. ✅ **Skálázható** - csak a szükséges boltokat tölti le
2. ✅ **Gyors** - indexelt GPS koordináták, előszűrés
3. ✅ **Lapozható** - limit/offset támogatás
4. ✅ **Flexibilis** - kombinálható szűrők
5. ✅ **Biztonságos** - JWT védelem
6. ✅ **Pontos távolság** - Haversine formula

---

## Indexek az adatbázisban (FONTOS a gyorsasághoz!):

```sql
-- GPS koordináták indexelése
CREATE INDEX idx_shops_location ON shops (latitude, longitude);

-- Szivar kínálat indexelése
CREATE INDEX idx_shops_has_cigars ON shops (has_cigars);

-- Teljes szöveges keresés (opcionális)
CREATE INDEX idx_shops_name ON shops USING gin(to_tsvector('hungarian', name));
CREATE INDEX idx_shops_address ON shops USING gin(to_tsvector('hungarian', address));
```
