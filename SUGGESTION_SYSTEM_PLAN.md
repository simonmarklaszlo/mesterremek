# Közösségi Javaslatok Rendszer - Implementációs Terv

## Áttekintés

A közösségi javaslatok rendszer célja, hogy a felhasználók közösségi módon karban tudják tartani az adatbázist. Hibás adatok javítására, új boltok hozzáadására tehetnek javaslatokat, amelyeket a közösség szavazással (upvote/downvote) értékel. **Ha egy javaslat eléri a +5 nettó szavazatot (upvote - downvote >= 5), automatikusan jóváhagyásra kerül és az adatbázis frissül.**

---

## 1. Jelenlegi Állapot Felmérése

### 1.1 Adatbázis (database/szivar.sql)

**Meglévő táblák:**
- ✅ `suggestion` - létezik, de más struktúrával
- ✅ `suggestion_type` - javaslat típusok
- ✅ `suggestion_ratings` - kapcsolótábla
- ✅ `suggestion_user_rating` - felhasználói értékelések
- ✅ `db_tables` - tábla nevek
- ✅ `db_columns` - oszlop nevek

**Probléma:** A jelenlegi struktúra túl generikus és bonyolult. A `column_id`, `record_id`, `new_value` alapú megközelítés nehezen kezelhető.

**Javasolt új struktúra:**
```sql
-- Javaslat típusok táblája (referencia adat)
CREATE TABLE suggestion_types (
  id SERIAL PRIMARY KEY,
  code VARCHAR(50) NOT NULL UNIQUE,               -- 'new_shop', 'edit_name', 'edit_address', 'edit_hours'
  name VARCHAR(100) NOT NULL,                     -- Megjelenítendő név
  description TEXT
);

-- Javaslat státuszok táblája (referencia adat)
CREATE TABLE suggestion_statuses (
  id SERIAL PRIMARY KEY,
  code VARCHAR(50) NOT NULL UNIQUE,               -- 'pending', 'approved', 'rejected', 'applied'
  name VARCHAR(100) NOT NULL,                     -- Megjelenítendő név
  description TEXT
);

-- Egyszerűbb, konkrétabb suggestion tábla
CREATE TABLE suggestions (
  id SERIAL PRIMARY KEY,
  type_id INTEGER NOT NULL REFERENCES suggestion_types(id),
  shop_id INTEGER REFERENCES shops(id),           -- NULL ha új bolt, kitöltve ha módosítás
  proposed_value TEXT NOT NULL,                   -- JSON vagy TEXT (főértéket tartalmaz)
  additional_data JSONB,                          -- További adatok (cím, város, koordináták, stb.)
  user_id INTEGER NOT NULL REFERENCES users(id),
  status_id INTEGER NOT NULL DEFAULT 1 REFERENCES suggestion_statuses(id),  -- 1 = pending
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);

-- Nyitvatartási javaslatok külön táblában
CREATE TABLE suggestion_opening_hours (
  id SERIAL PRIMARY KEY,
  suggestion_id INTEGER NOT NULL REFERENCES suggestions(id) ON DELETE CASCADE,
  day_id INTEGER NOT NULL REFERENCES days_of_week(id),
  open_hour TIME,
  close_hour TIME
);

-- Szavazatok
CREATE TABLE suggestion_votes (
  id SERIAL PRIMARY KEY,
  suggestion_id INTEGER NOT NULL REFERENCES suggestions(id) ON DELETE CASCADE,
  user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  vote_type VARCHAR(10) NOT NULL,                 -- 'like', 'dislike'
  created_at TIMESTAMP DEFAULT NOW(),
  UNIQUE(suggestion_id, user_id)                  -- Egy user csak egyszer szavazhat
);

-- Indexek a gyorsabb lekérdezésekhez
CREATE INDEX idx_suggestions_status_id ON suggestions(status_id);
CREATE INDEX idx_suggestions_type_id ON suggestions(type_id);
CREATE INDEX idx_suggestions_user_id ON suggestions(user_id);
CREATE INDEX idx_suggestions_shop_id ON suggestions(shop_id) WHERE shop_id IS NOT NULL;
CREATE INDEX idx_votes_suggestion ON suggestion_votes(suggestion_id);
CREATE INDEX idx_votes_user ON suggestion_votes(user_id);
```

### 1.2 Backend (backend/src/)

**Meglévő struktúra:**
- ✅ `routes/userRoutes.ts` - user authentikáció
- ✅ `routes/shopRoutes.ts` - bolt lekérdezések
- ✅ `model/` - típus definíciók
- ✅ `db/` - adatbázis hozzáférés

**Hiányzik:**
- ❌ `routes/suggestionRoutes.ts` - javaslatok kezelése
- ❌ `routes/func/suggestion.ts` - javaslat funkciók
- ❌ `model/suggestion.ts` - javaslat típusok
- ❌ `db/suggestionAccess.ts` - javaslat DB műveletek

### 1.3 Frontend (frontend/SzivarClubApp/src/)

**Meglévő:**
- ✅ `app/pages/community/community.page.ts` - javaslatok listázása (mock adat)
- ✅ `app/pages/community/community.page.html` - UI elkészült
- ✅ `app/pages/community/create-suggestion-modal/` - új javaslat modal
- ✅ Szavazás UI elkészült (mock funkció)

**Hiányzik:**
- ❌ API service (suggestion.service.ts)
- ❌ Backend integráció
- ❌ Authentikáció token küldés
- ❌ Bolt kiválasztó modal módosítási javaslatokhoz
- ❌ Térkép integráció új bolt hozzáadásakor (koordináták)

---

## 2. Javaslat Típusok Részletesen

### 2.1 Új Bolt Hozzáadása (`type: 'new_shop'`)

**Szükséges mezők:**
- `proposed_value` (TEXT): Bolt neve
- `additional_data` (JSONB):
  ```json
  {
    "address": "Budapest, Kossuth utca 12.",
    "city": "Budapest",
    "latitude": 47.4983,
    "longitude": 19.0704
  }
  ```
- `suggestion_opening_hours`: Nyitvatartás (opcionális)

**Flow:**
1. User kitölti az űrlapot (név, cím, város)
2. Frontend geocoding API hívás (Nominatim/Mapbox) → koordináták
3. POST `/api/suggestions` → backend elmenti
4. Közösség szavaz
5. +5 nettó szavazatnál → `status: 'approved'`
6. Automatikus vagy manuális alkalmazás → új rekord a `shops` táblában

### 2.2 Bolt Nevének Módosítása (`type: 'edit_name'`)

**Szükséges mezők:**
- `shop_id` (INTEGER): Melyik bolt
- `proposed_value` (TEXT): Új név

**Flow:**
1. User kiválasztja a boltot
2. Beírja az új nevet
3. POST `/api/suggestions`
4. +5 szavazat után → `UPDATE shops SET name = proposed_value WHERE id = shop_id`

### 2.3 Bolt Címének Módosítása (`type: 'edit_address'`)

**Szükséges mezők:**
- `shop_id` (INTEGER): Melyik bolt
- `proposed_value` (TEXT): Új cím
- `additional_data` (JSONB):
  ```json
  {
    "city": "Budapest",
    "latitude": 47.4983,
    "longitude": 19.0704
  }
  ```

### 2.4 Nyitvatartás Módosítása (`type: 'edit_hours'`)

**Szükséges mezők:**
- `shop_id` (INTEGER): Melyik bolt
- `proposed_value` (TEXT): "Nyitvatartás módosítása" (description)
- `suggestion_opening_hours`: Új nyitvatartási adatok (7 nap)

**Flow:**
1. User kiválasztja a boltot
2. Módosítja a nyitvatartást
3. POST `/api/suggestions` + opening hours rekordok
4. +5 szavazat után:
   - `DELETE FROM shop_opening_hours WHERE shop_id = ?`
   - `INSERT INTO shop_opening_hours` (új adatok)

---

## 3. Szavazási Rendszer

### 3.1 Szavazás Logika

**Szabályok:**
- Egy user egy javaslatra csak egyszer szavazhat
- Lehet: `like`, `dislike`, vagy semmi
- Átszavazás: törli az előző szavazatot, új létrehozza
- Visszavonás: törli a szavazatot

**Nettó szavazat számítás:**
```sql
SELECT 
  COUNT(CASE WHEN vote_type = 'like' THEN 1 END) as likes,
  COUNT(CASE WHEN vote_type = 'dislike' THEN 1 END) as dislikes,
  COUNT(CASE WHEN vote_type = 'like' THEN 1 END) - 
  COUNT(CASE WHEN vote_type = 'dislike' THEN 1 END) as net_votes
FROM suggestion_votes
WHERE suggestion_id = ?
```

### 3.2 Automatikus Jóváhagyás

**Trigger vagy Backend logika:**

**Opció A: PostgreSQL Trigger**
```sql
CREATE OR REPLACE FUNCTION check_suggestion_approval()
RETURNS TRIGGER AS $$
DECLARE
  net_votes INTEGER;
BEGIN
  SELECT COUNT(CASE WHEN vote_type = 'like' THEN 1 END) - 
         COUNT(CASE WHEN vote_type = 'dislike' THEN 1 END)
  INTO net_votes
  FROM suggestion_votes
  WHERE suggestion_id = NEW.suggestion_id;
  
  IF net_votes >= 5 THEN
    UPDATE suggestions 
    SET status = 'approved', updated_at = NOW()
    WHERE id = NEW.suggestion_id AND status = 'pending';
  END IF;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_suggestion_approval
AFTER INSERT OR UPDATE OR DELETE ON suggestion_votes
FOR EACH ROW
EXECUTE FUNCTION check_suggestion_approval();
```

**Opció B: Backend logika (egyszerűbb, ajánlott kezdéshez)**
```typescript
async function voteOnSuggestion(suggestionId: number, userId: number, voteType: 'like' | 'dislike') {
  // 1. Szavazat kezelése (insert/update/delete)
  await handleVote(suggestionId, userId, voteType);
  
  // 2. Nettó szavazat lekérdezése
  const netVotes = await getNetVotes(suggestionId);
  
  // 3. Ha >= 5, jóváhagyás
  if (netVotes >= 5) {
    await db.query('UPDATE suggestions SET status = $1, updated_at = NOW() WHERE id = $2 AND status = $3', 
      ['approved', suggestionId, 'pending']);
  }
  
  return await getSuggestionById(suggestionId);
}
```

---

## 4. Backend Implementáció Terv

### 4.1 Adatbázis Migráció

**Lépések:**
1. Backup készítése a jelenlegi `suggestion*` táblákról
2. Új táblák létrehozása (`suggestions`, `suggestion_opening_hours`, `suggestion_votes`)
3. Régi táblák átnevezése vagy törlése (ha nincs használatban)

**Fájl:** `database/migrations/001_create_suggestions.sql`

### 4.2 Model Definíciók

**backend/src/model/suggestion.ts**
```typescript
export type SuggestionType = {
  id: number;
  code: 'new_shop' | 'edit_name' | 'edit_address' | 'edit_hours';
  name: string;
  description?: string;
};

export type SuggestionStatus = {
  id: number;
  code: 'pending' | 'approved' | 'rejected' | 'applied';
  name: string;
  description?: string;
};

export type Suggestion = {
  id: number;
  typeId: number;
  type?: SuggestionType;                // JOIN-olt
  typeCode?: string;                    // Gyors hozzáférés
  shopId: number | null;
  shopName?: string;                    // JOIN-olt, display célra
  proposedValue: string;
  additionalData?: SuggestionAdditionalData;
  userId: number;
  userName: string;                     // JOIN-olt
  statusId: number;
  status?: SuggestionStatus;            // JOIN-olt
  statusCode?: string;                  // Gyors hozzáférés
  createdAt: Date;
  updatedAt: Date;
  likes: number;                        // Számított
  dislikes: number;                     // Számított
  userVote?: 'like' | 'dislike' | null; // JOIN-olt
  openingHours?: SuggestionOpeningHour[];
};

export type SuggestionAdditionalData = {
  address?: string;
  city?: string;
  latitude?: number;
  longitude?: number;
};

export type SuggestionOpeningHour = {
  id?: number;
  suggestionId: number;
  dayId: number;
  dayName?: string;                     // JOIN-olt
  openHour: string | null;              // TIME vagy NULL
  closeHour: string | null;
};

export type SuggestionVote = {
  id: number;
  suggestionId: number;
  userId: number;
  voteType: 'like' | 'dislike';
  createdAt: Date;
};

export type CreateSuggestionRequest = {
  type: 'new_shop' | 'edit_name' | 'edit_address' | 'edit_hours';  // Code-ot küld
  shopId?: number;
  proposedValue: string;
  additionalData?: SuggestionAdditionalData;
  openingHours?: Omit<SuggestionOpeningHour, 'id' | 'suggestionId'>[];
};
```

### 4.3 Database Access Layer

**backend/src/db/suggestionAccess.ts**

Funkciók:
- `getAllSuggestions(userId: number, filter?: string, status?: string)`: Összes javaslat lekérdezése
- `getSuggestionById(id: number, userId: number)`: Egy javaslat részletei
- `createSuggestion(data: CreateSuggestionRequest, userId: number)`: Új javaslat
- `voteSuggestion(suggestionId: number, userId: number, voteType: string)`: Szavazás
- `removeVote(suggestionId: number, userId: number)`: Szavazat visszavonás
- `deleteSuggestion(id: number, userId: number)`: Javaslat törlése (csak saját)
- `applySuggestion(id: number)`: Javaslat alkalmazása (admin/auto)
- `getNetVotes(suggestionId: number)`: Nettó szavazat számítás

### 4.4 Route Handlers

**backend/src/routes/func/suggestion.ts**

- `handleGetSuggestions`: GET `/api/suggestions`
- `handleCreateSuggestion`: POST `/api/suggestions`
- `handleVoteSuggestion`: POST `/api/suggestions/:id/vote`
- `handleDeleteVote`: DELETE `/api/suggestions/:id/vote`
- `handleDeleteSuggestion`: DELETE `/api/suggestions/:id`
- `handleApplySuggestion`: POST `/api/suggestions/:id/apply` (admin)

**backend/src/routes/suggestionRoutes.ts**
```typescript
import { Router } from 'express';
import { authMiddleware } from '../middleware/auth';
import {
  handleGetSuggestions,
  handleCreateSuggestion,
  handleVoteSuggestion,
  handleDeleteVote,
  handleDeleteSuggestion,
  handleApplySuggestion
} from './func/suggestion';

const router = Router();

// Minden route védett (auth szükséges)
router.use(authMiddleware);

router.get('/', handleGetSuggestions);
router.post('/', handleCreateSuggestion);
router.post('/:id/vote', handleVoteSuggestion);
router.delete('/:id/vote', handleDeleteVote);
router.delete('/:id', handleDeleteSuggestion);
router.post('/:id/apply', handleApplySuggestion); // Admin only

export default router;
```

**backend/src/main.ts** - route hozzáadása:
```typescript
import suggestionRoutes from "./routes/suggestionRoutes";
app.use("/api/suggestions", suggestionRoutes);
```

### 4.5 Authentikáció Middleware

**backend/src/middleware/auth.ts**
```typescript
import jwt from 'jsonwebtoken';
import { Request, Response, NextFunction } from 'express';

export interface AuthRequest extends Request {
  userId?: number;
  userRole?: string;
}

export const authMiddleware = (req: AuthRequest, res: Response, next: NextFunction) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  
  if (!token) {
    return res.status(401).json({ error: 'Unauthorized' });
  }
  
  try {
    const decoded = jwt.verify(token, process.env.JWT_SECRET || 'secret') as any;
    req.userId = decoded.userId;
    req.userRole = decoded.role;
    next();
  } catch (error) {
    return res.status(401).json({ error: 'Invalid token' });
  }
};

export const adminMiddleware = (req: AuthRequest, res: Response, next: NextFunction) => {
  if (req.userRole !== 'admin') {
    return res.status(403).json({ error: 'Forbidden' });
  }
  next();
};
```

---

## 5. Frontend Implementáció Terv

### 5.1 Suggestion Service

**frontend/SzivarClubApp/src/app/services/suggestion.service.ts**

```typescript
@Injectable({
  providedIn: 'root'
})
export class SuggestionService {
  private apiUrl = environment.apiUrl + '/suggestions';
  
  constructor(private http: HttpClient) {}
  
  getSuggestions(filter?: string, status?: string): Observable<Suggestion[]> {
    let params = new HttpParams();
    if (filter) params = params.set('filter', filter);
    if (status) params = params.set('status', status);
    
    return this.http.get<{suggestions: Suggestion[]}>(this.apiUrl, { params })
      .pipe(map(res => res.suggestions));
  }
  
  createSuggestion(data: CreateSuggestionRequest): Observable<Suggestion> {
    return this.http.post<{suggestion: Suggestion}>(this.apiUrl, data)
      .pipe(map(res => res.suggestion));
  }
  
  voteSuggestion(id: number, voteType: 'like' | 'dislike'): Observable<Suggestion> {
    return this.http.post<{suggestion: Suggestion}>(`${this.apiUrl}/${id}/vote`, { voteType })
      .pipe(map(res => res.suggestion));
  }
  
  removeVote(id: number): Observable<Suggestion> {
    return this.http.delete<{suggestion: Suggestion}>(`${this.apiUrl}/${id}/vote`)
      .pipe(map(res => res.suggestion));
  }
  
  deleteSuggestion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
```

### 5.2 Community Page Integráció

**Módosítások a `community.page.ts`-ben:**
- Mock adatok eltávolítása
- Service injection
- `loadSuggestions()` valós API hívás
- `vote()` valós API hívás
- Error handling (toast üzenetek)

### 5.3 Create Suggestion Modal Bővítés

**Új funkciók:**
- Suggestion típus választó (új bolt / módosítás)
- Ha módosítás: bolt keresés és kiválasztás
- Geocoding integráció (új boltnál)
- Térkép előnézet (leaflet)

**frontend/SzivarClubApp/src/app/services/geocoding.service.ts**
```typescript
@Injectable({
  providedIn: 'root'
})
export class GeocodingService {
  private nominatimUrl = 'https://nominatim.openstreetmap.org/search';
  
  geocodeAddress(address: string, city: string): Observable<{lat: number, lon: number}> {
    const fullAddress = `${address}, ${city}, Hungary`;
    const params = new HttpParams()
      .set('q', fullAddress)
      .set('format', 'json')
      .set('limit', '1');
    
    return this.http.get<any[]>(this.nominatimUrl, { params }).pipe(
      map(results => {
        if (results.length === 0) {
          throw new Error('Cím nem található');
        }
        return {
          lat: parseFloat(results[0].lat),
          lon: parseFloat(results[0].lon)
        };
      })
    );
  }
}
```

### 5.4 Auth Token Kezelés

**HTTP Interceptor létrehozása:**

**frontend/SzivarClubApp/src/app/interceptors/auth.interceptor.ts**
```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return next(req);
};
```

**Regisztráció az `app.config.ts`-ben:**
```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
    // ...
  ]
};
```

---

## 6. Javaslat Alkalmazása (Apply)

### 6.1 Automatikus Alkalmazás

**Opció A: Cron job / Scheduled task**
- Periodikusan (pl. 5 percenként) ellenőrzi az `approved` státuszú javaslatokat
- Alkalmazza őket és `applied` státuszra állítja

**Opció B: Azonnal (szavazáskor)**
- Ha `status` → `approved`, azonnal alkalmazza
- Trigger vagy backend logika

**Ajánlott: Opció B**

### 6.2 Apply Logika

**backend/src/db/suggestionAccess.ts**
```typescript
async function applySuggestion(suggestionId: number) {
  const suggestion = await getSuggestionById(suggestionId, 0); // Admin context
  
  if (suggestion.status !== 'approved') {
    throw new Error('Only approved suggestions can be applied');
  }
  
  const client = await pool.connect();
  
  try {
    await client.query('BEGIN');
    
    switch (suggestion.type) {
      case 'new_shop':
        await applyNewShop(suggestion, client);
        break;
      case 'edit_name':
        await applyEditName(suggestion, client);
        break;
      case 'edit_address':
        await applyEditAddress(suggestion, client);
        break;
      case 'edit_hours':
        await applyEditHours(suggestion, client);
        break;
    }
    
    // Státusz frissítés
    await client.query(
      'UPDATE suggestions SET status = $1, updated_at = NOW() WHERE id = $2',
      ['applied', suggestionId]
    );
    
    await client.query('COMMIT');
  } catch (error) {
    await client.query('ROLLBACK');
    throw error;
  } finally {
    client.release();
  }
}

async function applyNewShop(suggestion: Suggestion, client: any) {
  const { proposedValue, additionalData } = suggestion;
  
  // 1. Shop rekord létrehozása
  const shopResult = await client.query(
    `INSERT INTO shops (name, address, city, location, created_at, updated_at)
     VALUES ($1, $2, $3, ST_SetSRID(ST_MakePoint($4, $5), 4326), NOW(), NOW())
     RETURNING id`,
    [
      proposedValue,
      additionalData.address,
      additionalData.city,
      additionalData.longitude,
      additionalData.latitude
    ]
  );
  
  const newShopId = shopResult.rows[0].id;
  
  // 2. Nyitvatartás másolása (ha van)
  if (suggestion.openingHours && suggestion.openingHours.length > 0) {
    for (const oh of suggestion.openingHours) {
      await client.query(
        `INSERT INTO shop_opening_hours (shop_id, day_id, open_hour, close_hour)
         VALUES ($1, $2, $3, $4)`,
        [newShopId, oh.dayId, oh.openHour, oh.closeHour]
      );
    }
  }
}

async function applyEditName(suggestion: Suggestion, client: any) {
  await client.query(
    'UPDATE shops SET name = $1, updated_at = NOW() WHERE id = $2',
    [suggestion.proposedValue, suggestion.shopId]
  );
}

async function applyEditAddress(suggestion: Suggestion, client: any) {
  const { proposedValue, additionalData } = suggestion;
  await client.query(
    `UPDATE shops 
     SET address = $1, city = $2, location = ST_SetSRID(ST_MakePoint($3, $4), 4326), updated_at = NOW()
     WHERE id = $5`,
    [
      proposedValue,
      additionalData.city,
      additionalData.longitude,
      additionalData.latitude,
      suggestion.shopId
    ]
  );
}

async function applyEditHours(suggestion: Suggestion, client: any) {
  // 1. Régi nyitvatartás törlése
  await client.query('DELETE FROM shop_opening_hours WHERE shop_id = $1', [suggestion.shopId]);
  
  // 2. Új nyitvatartás beszúrása
  if (suggestion.openingHours && suggestion.openingHours.length > 0) {
    for (const oh of suggestion.openingHours) {
      await client.query(
        `INSERT INTO shop_opening_hours (shop_id, day_id, open_hour, close_hour)
         VALUES ($1, $2, $3, $4)`,
        [suggestion.shopId, oh.dayId, oh.openHour, oh.closeHour]
      );
    }
  }
}
```

---

## 7. Implementációs Sorrend

### Sprint 1: Adatbázis & Backend Alapok (1-2 nap)
1. ✅ Adatbázis migráció SQL elkészítése
2. ✅ Új táblák létrehozása (suggestions, suggestion_votes, suggestion_opening_hours)
3. ✅ Model típusok (suggestion.ts)
4. ✅ Database access layer (suggestionAccess.ts) - alapműveletek

### Sprint 2: Backend API (2-3 nap)
5. ✅ Auth middleware
6. ✅ Route handlers (GET, POST suggestions)
7. ✅ Vote endpoint (POST /suggestions/:id/vote)
8. ✅ Automatikus approval logika
9. ✅ Tesztelés Postman-nel

### Sprint 3: Frontend Service & Integration (2-3 nap)
10. ✅ Suggestion service
11. ✅ Auth interceptor
12. ✅ Community page integráció (valós API hívások)
13. ✅ Voting funkció bekötés
14. ✅ Error handling & toast üzenetek

### Sprint 4: Create Suggestion Modal Bővítés (2-3 nap)
15. ✅ Típus választó UI
16. ✅ Bolt keresés modal (meglévő boltok listája)
17. ✅ Geocoding service
18. ✅ Leaflet térkép integráció
19. ✅ Create suggestion submit

### Sprint 5: Apply & Admin (1-2 nap)
20. ✅ Apply suggestion backend logika
21. ✅ Automatikus apply hook (szavazásnál)
22. ✅ Admin dashboard (opcionális)

### Sprint 6: Tesztelés & Bugfix (1-2 nap)
23. ✅ E2E tesztelés
24. ✅ Edge case-ek kezelése
25. ✅ UI finomítás
26. ✅ Dokumentáció frissítés

---

## 8. Kockázatok & Megoldások

### 8.1 Geocoding Rate Limit
**Probléma:** Nominatim 1 req/sec limit
**Megoldás:**
- Frontend debounce (min 1.5s várakozás)
- Backend cache (cím → koordináta cache)
- Mapbox átállás (100k/hó ingyenes)

### 8.2 Spam Javaslatok
**Probléma:** Rosszhiszemű felhasználók sok javaslatot küldenek
**Megoldás:**
- Rate limiting (max 10 javaslat/nap/user)
- Captcha (opcionális)
- Admin moderáció első pár javaslat esetén

### 8.3 Koordináta Pontosság
**Probléma:** Geocoding nem mindig pontos
**Megoldás:**
- Manual térkép pin lehetőség
- Address validation
- Community voting (javítási javaslat koordinátákra is)

### 8.4 Concurrent Votes
**Probléma:** Egyszerre többen szavaznak, race condition
**Megoldás:**
- Database UNIQUE constraint (suggestion_id, user_id)
- Transaction használata
- Optimistic locking

---

## 9. Jövőbeli Fejlesztések

### 9.1 Admin Dashboard
- Összes javaslat áttekintése
- Manuális jóváhagyás/elutasítás
- Statisztikák (legaktívabb userek, típusok)

### 9.2 Értesítések
- Push notification ha javaslat jóváhagyásra került
- Email notification a szerzőnek
- Badge a community page-en (új javaslatok száma)

### 9.3 Gamification
- Pontok gyűjtése javaslatokért
- Leaderboard (legaktívabb közreműködők)
- Badges (pl. "10 jóváhagyott javaslat")

### 9.4 Képek Javaslata
- Felhasználók képeket tölthetnek fel boltokról
- Community voting képekre is
- Automatikus jóváhagyás +5-nél

### 9.5 Térkép Alapú Keresés
- "Javaslatok a közelben" feature
- Térkép overlay: javaslatok láthatók

---

## 10. Összefoglalás

### Erőforrás Igény
- **Backend fejlesztő:** ~7-10 nap
- **Frontend fejlesztő:** ~7-10 nap
- **Tesztelő:** ~2-3 nap
- **Összesen:** ~3-4 hét (1 fő full-time esetén)

### Technológiai Stack
- **Backend:** Node.js, Express, TypeScript, PostgreSQL (PostGIS)
- **Frontend:** Angular, Ionic, Leaflet
- **External API:** OpenStreetMap Nominatim (Geocoding)
- **Auth:** JWT

### Sikerkritériumok
- ✅ Felhasználók tudnak új boltot javasolni
- ✅ Felhasználók tudnak módosítást javasolni (név, cím, nyitvatartás)
- ✅ Szavazási rendszer működik (like/dislike)
- ✅ +5 nettó szavazatnál automatikus jóváhagyás
- ✅ Jóváhagyott javaslatok automatikusan vagy manuálisan alkalmazva
- ✅ UI responsive és intuitív
- ✅ Backend API dokumentált és tesztelt

---

**Utolsó frissítés:** 2026-02-24  
**Készítette:** GitHub Copilot  
**Projekt:** SzivarClub - Közösségi Javaslatok Rendszer

