# Szivarbolt Részletek API Endpoint

## GET /api/shops/:id

**Leírás:** Egy adott szivarbolt részletes információinak lekérése (nyitvatartás, márkák, értékelések).

### Request Parameters:

| Paraméter | Típus | Kötelező | Leírás | Példa |
|-----------|-------|----------|--------|-------|
| `id` | number | ✅ | Bolt egyedi azonosítója | `1` |

### Példa Request:

```http
GET /api/shops/1
Authorization: Bearer <jwt_token>
```

### Response (200 OK):

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Nemzeti Dohánybolt",
    "address": "Andrássy út 42",
    "city": "Budapest",
    "latitude": 47.5028,
    "longitude": 19.0620,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-10-10T14:20:00Z",
    "openingHours": [
      {
        "id": 1,
        "dayOfWeek": "Hétfő",
        "openHour": "08:00",
        "closeHour": "20:00"
      },
      {
        "id": 2,
        "dayOfWeek": "Kedd",
        "openHour": "08:00",
        "closeHour": "20:00"
      },
      {
        "id": 3,
        "dayOfWeek": "Szerda",
        "openHour": "08:00",
        "closeHour": "20:00"
      },
      {
        "id": 4,
        "dayOfWeek": "Csütörtök",
        "openHour": "08:00",
        "closeHour": "20:00"
      },
      {
        "id": 5,
        "dayOfWeek": "Péntek",
        "openHour": "08:00",
        "closeHour": "21:00"
      },
      {
        "id": 6,
        "dayOfWeek": "Szombat",
        "openHour": "09:00",
        "closeHour": "18:00"
      },
      {
        "id": 7,
        "dayOfWeek": "Vasárnap",
        "openHour": "10:00",
        "closeHour": "16:00"
      }
    ],
    "cigarBrands": [
      {
        "id": 1,
        "brandId": 5,
        "brandName": "Cohiba",
        "addedBy": 12,
        "createdAt": "2024-02-10T09:15:00Z"
      },
      {
        "id": 2,
        "brandId": 8,
        "brandName": "Montecristo",
        "addedBy": 12,
        "createdAt": "2024-02-10T09:16:00Z"
      },
      {
        "id": 3,
        "brandId": 15,
        "brandName": "Romeo y Julieta",
        "addedBy": 23,
        "createdAt": "2024-03-05T14:30:00Z"
      }
    ],
    "reviews": [
      {
        "id": 45,
        "userId": 12,
        "userName": "Kiss János",
        "rating": 5,
        "comment": "Kiváló kínálat, kedves kiszolgálás!",
        "createdAt": "2024-09-20T15:30:00Z"
      },
      {
        "id": 46,
        "userId": 23,
        "userName": "Nagy Péter",
        "rating": 4,
        "comment": "Jó árak, széles választék.",
        "createdAt": "2024-09-25T11:20:00Z"
      }
    ],
    "averageRating": 4.5,
    "totalReviews": 23,
    "isOpenNow": true,
    "nextClosingTime": "20:00"
  }
}
```

### Response (200 OK) - Ha még nincs értékelés (FONTOS!):

**⚠️ A frontend ne crasheljen, ha a reviews üres vagy nincs értékelés!**

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Nemzeti Dohánybolt",
    "address": "Andrássy út 42",
    "city": "Budapest",
    "latitude": 47.5028,
    "longitude": 19.0620,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-10-10T14:20:00Z",
    "openingHours": [
      {
        "id": 1,
        "dayOfWeek": "Hétfő",
        "openHour": "08:00",
        "closeHour": "20:00"
      }
    ],
    "cigarBrands": [],
    "reviews": [],
    "averageRating": 0,
    "totalReviews": 0,
    "isOpenNow": true,
    "nextClosingTime": "20:00"
  }
}
```

### Response (404 Not Found):

```json
{
  "success": false,
  "error": "Not found",
  "message": "Shop with id 1 not found"
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

## Mező leírások:

### Kötelező mezők (mindig küldendő):
- `id`, `name`, `address`, `city`, `latitude`, `longitude`
- `createdAt`, `updatedAt`
- `openingHours` (tömb, lehet üres: `[]`)
- `cigarBrands` (tömb, lehet üres: `[]`)
- `reviews` (tömb, lehet üres: `[]`)
- `averageRating` (ha nincs értékelés: `0`)
- `totalReviews` (ha nincs értékelés: `0`)
- `isOpenNow` (boolean)
- `nextClosingTime` (string vagy `null`)

### Opcionális/lehet üres:
- `reviews[]` - **Lehet üres tömb, ha még nincs értékelés**
- `cigarBrands[]` - **Lehet üres tömb, ha még nincs marka hozzáadva**
- `nextClosingTime` - **Lehet null, ha zárva van vagy nem lehet meghatározni**

---

## Backend SQL Query példa (PostgreSQL):

```sql
-- Bolt alap adatok
SELECT 
  s.id,
  s.name,
  s.address,
  s.city,
  s.latitude,
  s.longitude,
  s.created_at,
  s.updated_at
FROM shops s
WHERE s.id = $1;

-- Nyitvatartás
SELECT 
  id,
  days_of_week as "dayOfWeek",
  open_hour as "openHour",
  close_hour as "closeHour"
FROM shop_opening_hours
WHERE shop_id = $1
ORDER BY 
  CASE days_of_week
    WHEN 'Hétfő' THEN 1
    WHEN 'Kedd' THEN 2
    WHEN 'Szerda' THEN 3
    WHEN 'Csütörtök' THEN 4
    WHEN 'Péntek' THEN 5
    WHEN 'Szombat' THEN 6
    WHEN 'Vasárnap' THEN 7
  END;

-- Szivar márkák
SELECT 
  scb.id,
  scb.cigar_id as "brandId",
  cb.name as "brandName",
  scb.added_by as "addedBy",
  scb.created_at as "createdAt"
FROM shop_cigar_brands scb
JOIN cigar_brands cb ON cb.id = scb.cigar_id
WHERE scb.shop_id = $1
ORDER BY cb.name;

-- Értékelések (LEHET ÜRES!)
SELECT 
  r.id,
  r.user_id as "userId",
  u.name as "userName",
  r.rating,
  r.comment,
  r.created_at as "createdAt"
FROM reviews r
JOIN users u ON u.id = r.user_id
WHERE r.shop_id = $1
ORDER BY r.created_at DESC
LIMIT 50;

-- Átlag értékelés és összesített adatok
SELECT 
  COALESCE(AVG(rating), 0) as "averageRating",
  COUNT(*) as "totalReviews"
FROM reviews
WHERE shop_id = $1;
```

---

## Backend TypeScript (Express) példa:

```typescript
// routes/shopRoutes.ts
import { Router } from 'express';
import { authMiddleware } from '../middleware/auth';
import { getShopDetails } from '../controllers/shopController';

const router = Router();

router.get('/:id', authMiddleware, getShopDetails);

export default router;
```

```typescript
// controllers/shopController.ts
import { Request, Response } from 'express';
import { pool } from '../db';

export const getShopDetails = async (req: Request, res: Response) => {
  try {
    const shopId = parseInt(req.params.id);

    if (isNaN(shopId)) {
      return res.status(400).json({
        success: false,
        error: 'Invalid parameter',
        message: 'Shop ID must be a number'
      });
    }

    // 1. Bolt alap adatok
    const shopResult = await pool.query(
      'SELECT id, name, address, city, latitude, longitude, created_at, updated_at FROM shops WHERE id = $1',
      [shopId]
    );

    if (shopResult.rows.length === 0) {
      return res.status(404).json({
        success: false,
        error: 'Not found',
        message: `Shop with id ${shopId} not found`
      });
    }

    const shop = shopResult.rows[0];

    // 2. Nyitvatartás
    const openingHoursResult = await pool.query(
      `SELECT id, days_of_week as "dayOfWeek", open_hour as "openHour", close_hour as "closeHour"
       FROM shop_opening_hours WHERE shop_id = $1
       ORDER BY CASE days_of_week
         WHEN 'Hétfő' THEN 1 WHEN 'Kedd' THEN 2 WHEN 'Szerda' THEN 3
         WHEN 'Csütörtök' THEN 4 WHEN 'Péntek' THEN 5 WHEN 'Szombat' THEN 6
         WHEN 'Vasárnap' THEN 7 END`,
      [shopId]
    );

    // 3. Szivar márkák
    const brandsResult = await pool.query(
      `SELECT sb.id, sb.cigar_id as "brandId", cb.name as "brandName", 
              sb.added_by as "addedBy", sb.created_at as "createdAt"
       FROM shop_cigar_brands sb
       JOIN cigar_brands cb ON cb.id = sb.cigar_id
       WHERE sb.shop_id = $1
       ORDER BY cb.name`,
      [shopId]
    );

    // 4. Értékelések (LEHET ÜRES!)
    const reviewsResult = await pool.query(
      `SELECT r.id, r.user_id as "userId", u.name as "userName",
              r.rating, r.comment, r.created_at as "createdAt"
       FROM reviews r
       JOIN users u ON u.id = r.user_id
       WHERE r.shop_id = $1
       ORDER BY r.created_at DESC
       LIMIT 50`,
      [shopId]
    );

    // 5. Átlag értékelés
    const ratingResult = await pool.query(
      `SELECT COALESCE(AVG(rating), 0) as "averageRating", COUNT(*) as "totalReviews"
       FROM reviews WHERE shop_id = $1`,
      [shopId]
    );

    const { averageRating, totalReviews } = ratingResult.rows[0];

    // 6. Most nyitva van-e? (egyszerűsített példa)
    const now = new Date();
    const dayOfWeek = ['Vasárnap', 'Hétfő', 'Kedd', 'Szerda', 'Csütörtök', 'Péntek', 'Szombat'][now.getDay()];
    const currentTime = now.toTimeString().slice(0, 5); // HH:MM
    
    const todayHours = openingHoursResult.rows.find(h => h.dayOfWeek === dayOfWeek);
    const isOpenNow = todayHours 
      ? currentTime >= todayHours.openHour && currentTime < todayHours.closeHour
      : false;
    const nextClosingTime = isOpenNow && todayHours ? todayHours.closeHour : null;

    // Válasz összeállítása
    return res.status(200).json({
      success: true,
      data: {
        id: shop.id,
        name: shop.name,
        address: shop.address,
        city: shop.city,
        latitude: parseFloat(shop.latitude),
        longitude: parseFloat(shop.longitude),
        createdAt: shop.created_at,
        updatedAt: shop.updated_at,
        openingHours: openingHoursResult.rows,
        cigarBrands: brandsResult.rows,
        reviews: reviewsResult.rows, // LEHET ÜRE TÖMB!
        averageRating: parseFloat(averageRating) || 0,
        totalReviews: parseInt(totalReviews) || 0,
        isOpenNow,
        nextClosingTime
      }
    });

  } catch (error) {
    console.error('Shop details error:', error);
    return res.status(500).json({
      success: false,
      error: 'Internal server error',
      message: 'Failed to fetch shop details'
    });
  }
};
```

---

## Frontend példa (Angular/Ionic):

```typescript
// services/shop.service.ts
getShopDetails(shopId: number): Observable<any> {
  return this.http.get(`${environment.apiUrl}/shops/${shopId}`);
}
```

```typescript
// components/shop-details-modal.component.ts
loadShopDetails() {
  this.isLoading = true;
  this.errorMessage = '';

  this.shopService.getShopDetails(this.shopId).subscribe({
    next: (response) => {
      this.shopDetails = response.data;
      this.isLoading = false;
    },
    error: (error) => {
      this.errorMessage = error.error?.message || 'Hiba történt a betöltés során';
      this.isLoading = false;
    }
  });
}
```

---

## Fontos megjegyzések a backend fejlesztőnek:

### ⚠️ KÖTELEZŐ követelmények:

1. **reviews tömb mindig legyen jelen** - ha nincs értékelés, üres tömb: `[]`
2. **cigarBrands tömb mindig legyen jelen** - ha nincs márka, üres tömb: `[]`
3. **averageRating mindig legyen jelen** - ha nincs értékelés: `0` (nem `null`!)
4. **totalReviews mindig legyen jelen** - ha nincs értékelés: `0` (nem `null`!)
5. **nextClosingTime lehet `null`** - ha zárva van vagy nem meghatározható

### ✅ Opcionális mezők kezelése:

- Ha egy bolt még új és nincs értékelése, a frontend ne crasheljen
- Ha egy bolthoz még nem adtak hozzá szivar márkát, üres lista jelenjen meg
- A comment mező lehet üres string vagy null az értékeléseknél

### 🔒 Biztonság:

- JWT token kötelező az endpoint eléréséhez
- Validáld a shop ID-t (csak szám lehet)
- SQL injection elleni védelem (parameterized queries)

---

## Példa válasz ha minden üres:

```json
{
  "success": true,
  "data": {
    "id": 999,
    "name": "Új Dohánybolt",
    "address": "Teszt utca 1",
    "city": "Budapest",
    "latitude": 47.4979,
    "longitude": 19.0402,
    "createdAt": "2025-01-17T10:00:00Z",
    "updatedAt": "2025-01-17T10:00:00Z",
    "openingHours": [],
    "cigarBrands": [],
    "reviews": [],
    "averageRating": 0,
    "totalReviews": 0,
    "isOpenNow": false,
    "nextClosingTime": null
  }
}
```
