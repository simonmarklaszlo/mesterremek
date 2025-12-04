# Backend Community (Közösségi Javaslatok) API Követelmények

## Áttekintés

A közösségi javaslatok funkció lehetővé teszi a felhasználók számára, hogy új boltokat, vagy meglévő boltok adatainak módosítását javasolják. A javaslatok szavazás útján (like/dislike) kerülnek jóváhagyásra. Ha egy javaslat eléri a +5 nettó szavazatot (likes - dislikes >= 5), automatikusan jóváhagyott státuszba kerül.

## Adatmodellek

### Suggestion (Javaslat)

```typescript
interface Suggestion {
  id: number;                                      // Egyedi azonosító (AUTO_INCREMENT)
  type: 'shop' | 'hours' | 'address' | 'name';    // Javaslat típusa
  shopId?: number;                                 // Opcionális: meglévő bolt ID (ha módosítás)
  shopName?: string;                               // Opcionális: bolt neve (display célra)
  shopAddress?: string;                            // Opcionális: bolt címe
  city?: string;                                   // Opcionális: város
  proposedValue: string;                           // Javasolt érték (pl. új bolt neve)
  latitude?: number;                               // Opcionális: szélességi koordináta (új boltnál)
  longitude?: number;                              // Opcionális: hosszúsági koordináta (új boltnál)
  authorId: number;                                // Javaslattevő felhasználó ID
  author: string;                                  // Javaslattevő felhasználó neve (display)
  createdAt: Date;                                 // Javaslat létrehozási időpontja
  likes: number;                                   // Like-ok száma (számított vagy tárolt)
  dislikes: number;                                // Dislike-ok száma (számított vagy tárolt)
  userVote?: 'like' | 'dislike' | null;           // Aktuális felhasználó szavazata (JOIN-olt)
  status: 'pending' | 'approved' | 'rejected';    // Javaslat állapota
  openingHours?: OpeningHour[];                    // Opcionális: nyitvatartási adatok
}

interface OpeningHour {
  id?: number;                                     // Egyedi azonosító
  suggestionId: number;                            // Melyik javaslathoz tartozik
  dayOfWeek: string;                               // Nap neve (Hétfő, Kedd, stb.)
  openHour: string;                                // Nyitás időpontja (HH:MM) vagy "Zárva"
  closeHour: string;                               // Zárás időpontja (HH:MM) vagy üres
}

interface SuggestionVote {
  id: number;                                      // Egyedi azonosító
  suggestionId: number;                            // Melyik javaslatra szavazott
  userId: number;                                  // Melyik felhasználó szavazott
  voteType: 'like' | 'dislike';                   // Szavazat típusa
  createdAt: Date;                                 // Szavazás időpontja
}
```

## Adatbázis séma

### `suggestions` tábla

```sql
CREATE TABLE suggestions (
  id INT AUTO_INCREMENT PRIMARY KEY,
  type ENUM('shop', 'hours', 'address', 'name') NOT NULL,
  shop_id INT NULL,                               -- Meglévő bolt ID (ha módosítás)
  shop_name VARCHAR(255) NULL,                    -- Bolt neve (display)
  shop_address VARCHAR(500) NULL,                 -- Bolt címe
  city VARCHAR(100) NULL,                         -- Város
  proposed_value TEXT NOT NULL,                   -- Javasolt érték
  latitude DECIMAL(10, 8) NULL,                   -- Szélességi koordináta
  longitude DECIMAL(11, 8) NULL,                  -- Hosszúsági koordináta
  author_id INT NOT NULL,                         -- FK users.id
  status ENUM('pending', 'approved', 'rejected') DEFAULT 'pending',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (author_id) REFERENCES users(id) ON DELETE CASCADE,
  FOREIGN KEY (shop_id) REFERENCES shops(id) ON DELETE CASCADE
);
```

### `suggestion_opening_hours` tábla

```sql
CREATE TABLE suggestion_opening_hours (
  id INT AUTO_INCREMENT PRIMARY KEY,
  suggestion_id INT NOT NULL,
  day_of_week VARCHAR(20) NOT NULL,               -- Hétfő, Kedd, stb.
  open_hour VARCHAR(10) NOT NULL,                 -- HH:MM vagy "Zárva"
  close_hour VARCHAR(10) NULL,                    -- HH:MM vagy NULL/üres
  FOREIGN KEY (suggestion_id) REFERENCES suggestions(id) ON DELETE CASCADE
);
```

### `suggestion_votes` tábla

```sql
CREATE TABLE suggestion_votes (
  id INT AUTO_INCREMENT PRIMARY KEY,
  suggestion_id INT NOT NULL,
  user_id INT NOT NULL,
  vote_type ENUM('like', 'dislike') NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY unique_vote (suggestion_id, user_id),  -- Egy user csak egyszer szavazhat
  FOREIGN KEY (suggestion_id) REFERENCES suggestions(id) ON DELETE CASCADE,
  FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
```

## API Endpointok

### 1. Javaslatok lekérdezése

**GET** `/api/suggestions`

**Query paraméterek:**
- `filter` (optional): `"all"` | `"community"` | `"own"` - Szűrés típus szerint
- `status` (optional): `"pending"` | `"approved"` | `"rejected"` - Szűrés státusz szerint
- `type` (optional): `"shop"` | `"hours"` | `"address"` | `"name"` - Szűrés javaslat típus szerint

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "suggestions": [
    {
      "id": 1,
      "type": "shop",
      "proposedValue": "Új Szivar Bolt",
      "shopAddress": "1051 Budapest, Nádor utca 12.",
      "city": "Budapest",
      "latitude": 47.5016,
      "longitude": 19.0486,
      "author": "user123",
      "authorId": 5,
      "createdAt": "2025-11-10T10:30:00Z",
      "likes": 3,
      "dislikes": 1,
      "userVote": null,
      "status": "pending",
      "openingHours": [
        {
          "dayOfWeek": "Hétfő",
          "openHour": "09:00",
          "closeHour": "18:00"
        },
        {
          "dayOfWeek": "Vasárnap",
          "openHour": "Zárva",
          "closeHour": ""
        }
      ]
    }
  ]
}
```

**Üzleti logika:**
- Ha `filter=own`, csak az aktuális felhasználó javaslatait adja vissza
- A `likes` és `dislikes` számokat az `suggestion_votes` táblából kell összesíteni
- A `userVote` mezőt az aktuális felhasználó szavazata alapján kell kitölteni (JOIN)
- Az `author` nevet a `users` táblából kell JOIN-olni
- Ha `type=shop`, akkor az `openingHours` tömbnek is benne kell lennie (JOIN `suggestion_opening_hours`)

---

### 2. Új javaslat létrehozása

**POST** `/api/suggestions`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "type": "shop",
  "proposedValue": "Új Szivar Bolt",
  "shopAddress": "1051 Budapest, Nádor utca 12.",
  "city": "Budapest",
  "latitude": 47.5016,
  "longitude": 19.0486,
  "openingHours": [
    {
      "dayOfWeek": "Hétfő",
      "openHour": "09:00",
      "closeHour": "18:00"
    },
    {
      "dayOfWeek": "Vasárnap",
      "openHour": "Zárva",
      "closeHour": ""
    }
  ]
}
```

**Mezők típusonként:**

**Új bolt javaslat (`type: "shop"`):**
- `proposedValue` (kötelező): Bolt neve
- `shopAddress` (kötelező): Cím
- `city` (kötelező): Város
- `latitude` (kötelező): Szélességi koordináta
- `longitude` (kötelező): Hosszúsági koordináta
- `openingHours` (opcionális): Nyitvatartási adatok tömb

**Bolt adatainak módosítása (`type: "hours"` | `"address"` | `"name"`):**
- `shopId` (kötelező): Melyik bolthoz tartozik
- `proposedValue` (kötelező): Javasolt új érték
- `openingHours` (csak `type: "hours"` esetén): Új nyitvatartási adatok

**Response:** `201 Created`
```json
{
  "suggestion": {
    "id": 3,
    "type": "shop",
    "proposedValue": "Új Szivar Bolt",
    "shopAddress": "1051 Budapest, Nádor utca 12.",
    "city": "Budapest",
    "latitude": 47.5016,
    "longitude": 19.0486,
    "author": "currentUser",
    "authorId": 42,
    "createdAt": "2025-12-04T14:20:00Z",
    "likes": 0,
    "dislikes": 0,
    "userVote": null,
    "status": "pending",
    "openingHours": [...]
  }
}
```

**Hibák:**
- `400 Bad Request`: Hiányzó kötelező mezők
- `401 Unauthorized`: Nincs bejelentkezve
- `404 Not Found`: `shopId` nem létezik (módosítás esetén)

---

### 3. Szavazás egy javaslatra

**POST** `/api/suggestions/{suggestionId}/vote`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "voteType": "like"
}
```

**Response:** `200 OK`
```json
{
  "suggestion": {
    "id": 1,
    "likes": 4,
    "dislikes": 1,
    "userVote": "like",
    "status": "pending"
  }
}
```

**Üzleti logika:**
- Ha a felhasználó még nem szavazott: új szavazat létrehozása
- Ha már szavazott ugyanarra: szavazat törlése (visszavonás)
- Ha már szavazott másra: meglévő szavazat frissítése
- Ha `likes - dislikes >= 5`: `status` automatikusan `'approved'` lesz
- Automatikus jóváhagyás esetén az `updated_at` is frissül

**Hibák:**
- `401 Unauthorized`: Nincs bejelentkezve
- `404 Not Found`: Javaslat nem létezik

---

### 4. Szavazat visszavonása

**DELETE** `/api/suggestions/{suggestionId}/vote`

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "suggestion": {
    "id": 1,
    "likes": 3,
    "dislikes": 1,
    "userVote": null,
    "status": "pending"
  }
}
```

**Üzleti logika:**
- Törli a felhasználó szavazatát az adott javaslatra
- Ha nincs szavazata, nem történik semmi (nem hiba)

**Hibák:**
- `401 Unauthorized`: Nincs bejelentkezve
- `404 Not Found`: Javaslat nem létezik

---

### 5. Javaslat törlése (saját)

**DELETE** `/api/suggestions/{suggestionId}`

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `204 No Content`

**Üzleti logika:**
- Csak a saját javaslatait törölheti a felhasználó
- Cascade törli a kapcsolódó `suggestion_votes` és `suggestion_opening_hours` rekordokat

**Hibák:**
- `401 Unauthorized`: Nincs bejelentkezve
- `403 Forbidden`: Nem a saját javaslata
- `404 Not Found`: Javaslat nem létezik

---

### 6. Jóváhagyott javaslatok alkalmazása (Admin funkció - opcionális)

**POST** `/api/suggestions/{suggestionId}/apply`

**Headers:**
```
Authorization: Bearer {admin_token}
Content-Type: application/json
```

**Response:** `200 OK`

**Üzleti logika:**
- Csak `status: 'approved'` javaslatokat lehet alkalmazni
- `type: 'shop'`: új bolt létrehozása a `shops` táblában
- `type: 'hours'`: meglévő bolt nyitvatartásának frissítése
- `type: 'address'`: meglévő bolt címének frissítése
- `type: 'name'`: meglévő bolt nevének frissítése
- Alkalmazás után a javaslat `status` maradhat `'approved'` vagy válthat `'applied'` státuszra (új enum érték)

---

## Koordináták kezelése (címből)

### Geocoding API használata

Amikor új bolt kerül hozzáadásra, a felhasználó címet ad meg, amiből koordinátákat kell generálni.

**Ajánlott megoldások:**

1. **OpenStreetMap Nominatim API** (Ingyenes)
   - Endpoint: `https://nominatim.openstreetmap.org/search`
   - Query: `?q={address}&format=json&limit=1`
   - Nincs API kulcs szükséges, de rate limit van (1 req/sec)

2. **Google Maps Geocoding API** (Fizetős, de pontos)
   - Endpoint: `https://maps.googleapis.com/maps/api/geocode/json`
   - Query: `?address={address}&key={API_KEY}`
   - API kulcs szükséges

3. **Mapbox Geocoding API** (Freemium)
   - Endpoint: `https://api.mapbox.com/geocoding/v5/mapbox.places/{address}.json`
   - Query: `?access_token={ACCESS_TOKEN}`
   - Havi 100,000 ingyenes request

**Backend implementáció példa (Node.js):**

```typescript
async function geocodeAddress(address: string, city: string): Promise<{lat: number, lon: number}> {
  const fullAddress = `${address}, ${city}, Hungary`;
  const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(fullAddress)}&format=json&limit=1`;
  
  const response = await fetch(url, {
    headers: {
      'User-Agent': 'SzivarClubApp/1.0'  // Nominatim megköveteli
    }
  });
  
  const data = await response.json();
  
  if (data.length === 0) {
    throw new Error('Address not found');
  }
  
  return {
    lat: parseFloat(data[0].lat),
    lon: parseFloat(data[0].lon)
  };
}
```

**Frontend integráció:**
- A felhasználó beírja a címet
- Frontend elküldi a backend-nek
- Backend geocoding-ol és elmenti a koordinátákkal együtt
- VAGY: Frontend geocoding-ol közvetlenül (de kevésbé biztonságos API kulcs miatt)

---

## Jóváhagyási folyamat

### Automatikus jóváhagyás

Ha egy javaslat eléri a **+5 nettó szavazatot** (`likes - dislikes >= 5`):
1. A `status` automatikusan `'approved'` lesz
2. A `updated_at` frissül
3. Értesítés küldése az `author`-nak (opcionális)

### Manuális moderáció (opcionális)

Admin dashboard-on:
- Megtekintheti az összes javaslat
- Manuálisan jóváhagyhatja/elutasíthatja őket
- Alkalmazhatja a jóváhagyott javaslatokat

---

## Technológiai stack javaslat

**Backend:**
- Node.js + Express
- TypeScript
- MySQL/MariaDB
- JWT autentikáció

**Geocoding:**
- OpenStreetMap Nominatim (ingyenes induláshoz)
- Később átállás Mapbox-ra (jobb pontosság, több funkció)

---

## Példa flow: Új bolt hozzáadása

1. **Felhasználó kitölti a formot:**
   - Bolt neve: "Havana Cigar Lounge"
   - Cím: "Erzsébet körút 43."
   - Város: "Budapest"
   - Nyitvatartás: [...]

2. **Frontend elküldi POST /api/suggestions-nak**

3. **Backend feldolgozza:**
   - Geocoding: `"Erzsébet körút 43., Budapest, Hungary"` → `{lat: 47.4983, lon: 19.0704}`
   - Suggestions rekord létrehozása
   - Opening hours rekordok létrehozása (ha van)
   - Visszaadja a teljes Suggestion objektumot

4. **Közösség szavaz:**
   - User1: like (+1)
   - User2: like (+2)
   - User3: like (+3)
   - User4: dislike (+2)
   - User5: like (+3)
   - User6: like (+4)
   - User7: like (+5) → **Automatikus jóváhagyás!**

5. **Admin alkalmazza** (vagy automatikus):
   - Új rekord a `shops` táblában
   - Új rekordok az `opening_hours` táblában
   - Javaslat státusza: `'applied'` (opcionális új státusz)

---

## Adatbázis indexek

```sql
-- Gyorsabb keresés
CREATE INDEX idx_suggestions_status ON suggestions(status);
CREATE INDEX idx_suggestions_author ON suggestions(author_id);
CREATE INDEX idx_suggestions_type ON suggestions(type);
CREATE INDEX idx_suggestions_created ON suggestions(created_at DESC);

-- Gyorsabb szavazat keresés
CREATE INDEX idx_votes_user ON suggestion_votes(user_id);
CREATE INDEX idx_votes_suggestion ON suggestion_votes(suggestion_id);
```

---

## Következő lépések

1. ✅ Frontend Community page elkészült
2. ⏳ Backend API implementálása
3. ⏳ Adatbázis táblák létrehozása
4. ⏳ Geocoding integráció
5. ⏳ Add New Shop modal/page frontend
6. ⏳ Admin dashboard (opcionális)


