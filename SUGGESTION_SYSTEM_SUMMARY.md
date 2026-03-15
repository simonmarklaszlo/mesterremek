# Közösségi Javaslatok Rendszer - Rövid Összefoglaló

## 🎯 Cél
A felhasználók közösségi módon tartják karban az adatbázist. Ha találnak hibát vagy hiányosságot, javaslatot tehetnek, amit a közösség +5 upvote-nál automatikusan jóváhagy.

---

## 📊 Rendszer Működése

### 1️⃣ Javaslat Típusok

| Típus | Leírás | Példa |
|-------|--------|-------|
| **new_shop** | Új dohánybolt hozzáadása | "Havana Cigar Lounge, Budapest, Erzsébet krt. 43." |
| **edit_name** | Meglévő bolt nevének javítása | "Dohánybolt Újpest" → "Dohánybolt Újpest Központ" |
| **edit_address** | Bolt címének javítása | "Kossuth utca 12" → "Kossuth Lajos utca 12" |
| **edit_hours** | Nyitvatartás frissítése | Hétköznap 9-18h → Hétköznap 8-20h |

---

### 2️⃣ Javaslat Életciklus

```
┌─────────────────┐
│ User létrehoz   │
│  egy javaslatot │
└────────┬────────┘
         │
         ▼
    ┌─────────┐
    │ PENDING │ ◄───── Kezdeti státusz
    └────┬────┘
         │
         │ Közösség szavaz (👍 / 👎)
         │
         ▼
   Nettó szavazat >= +5?
         │
    ┌────┴────┐
    │   NEM   │   IGEN
    │         ├──────────────┐
    └─────────┘              ▼
                        ┌──────────┐
                        │ APPROVED │ ◄─── Automatikus jóváhagyás
                        └────┬─────┘
                             │
                             │ Backend/Admin alkalmazza
                             ▼
                        ┌─────────┐
                        │ APPLIED │ ◄─── Adatbázis frissítve
                        └─────────┘
```

**Státuszok:**
- 🟡 **PENDING**: Várakozik szavazatokra
- 🟢 **APPROVED**: Elérte a +5 szavazatot, alkalmazható
- 🔵 **APPLIED**: Alkalmazva, az adatbázis frissült
- 🔴 **REJECTED**: Admin elutasította (opcionális)

---

### 3️⃣ Szavazási Rendszer

**Szabályok:**
- Minden bejelentkezett user szavazhat
- Egy javaslatra **csak 1 szavazat** (👍 VAGY 👎)
- Átszavazás lehetséges
- Szavazat visszavonható

**Nettó szavazat számítás:**
```
Nettó szavazat = 👍 (likes) - 👎 (dislikes)
```

**Példa:**
```
Javaslat: "Új bolt: Havana Cigar Lounge"

User1: 👍
User2: 👍
User3: 👎
User4: 👍
User5: 👍
User6: 👍
User7: 👍

Nettó = 6 likes - 1 dislike = +5 ✅ → AUTOMATIKUS JÓVÁHAGYÁS
```

---

### 4️⃣ Adatbázis Struktúra (Normalizált)

```sql
┌─────────────────────────────────────┐
│     SUGGESTION_TYPES                │
├─────────────────────────────────────┤
│ id                                  │
│ code (new_shop/edit_name/...)       │
│ name (megjelenítendő név)           │
│ description                         │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│     SUGGESTION_STATUSES             │
├─────────────────────────────────────┤
│ id                                  │
│ code (pending/approved/...)         │
│ name (megjelenítendő név)           │
│ description                         │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│        SUGGESTIONS                  │
├─────────────────────────────────────┤
│ id                                  │
│ type_id → suggestion_types(id)      │
│ shop_id (NULL ha új, ID ha módosít) │
│ proposed_value (javasolt érték)     │
│ additional_data (JSON: cím, GPS)    │
│ user_id (ki javasolta)              │
│ status_id → suggestion_statuses(id) │
│ created_at                          │
│ updated_at                          │
└──────────┬──────────────────────────┘
           │
           │ 1:N
           ▼
┌─────────────────────────────────────┐
│    SUGGESTION_OPENING_HOURS         │
├─────────────────────────────────────┤
│ id                                  │
│ suggestion_id                       │
│ day_id (1=Hétfő, 2=Kedd...)         │
│ open_hour                           │
│ close_hour                          │
└─────────────────────────────────────┘

           │ 1:N
           ▼
┌─────────────────────────────────────┐
│      SUGGESTION_VOTES               │
├─────────────────────────────────────┤
│ id                                  │
│ suggestion_id                       │
│ user_id                             │
│ vote_type (like/dislike)            │
│ created_at                          │
│ UNIQUE(suggestion_id, user_id)      │
└─────────────────────────────────────┘
```

---

### 5️⃣ API Végpontok

| Metódus | Végpont | Leírás |
|---------|---------|--------|
| GET | `/api/suggestions` | Javaslatok listázása (szűrés: status, type, filter) |
| POST | `/api/suggestions` | Új javaslat létrehozása |
| POST | `/api/suggestions/:id/vote` | Szavazás (body: `{voteType: "like"}`) |
| DELETE | `/api/suggestions/:id/vote` | Szavazat visszavonása |
| DELETE | `/api/suggestions/:id` | Saját javaslat törlése |
| POST | `/api/suggestions/:id/apply` | Javaslat alkalmazása (admin/auto) |

**Autentikáció:** Minden endpoint JWT token-t igényel
```
Authorization: Bearer {token}
```

---

### 6️⃣ Frontend Flow - Új Bolt Hozzáadása

```
1. User megnyitja a Community oldalt
   └─> Floating Action Button: "+" gomb
       └─> Modal: "Új javaslat"

2. Típus kiválasztása: "Új dohánybolt"

3. Űrlap kitöltése:
   ├─ Bolt neve: "Havana Cigar Lounge"
   ├─ Cím: "Erzsébet körút 43."
   ├─ Város: "Budapest"
   └─ Nyitvatartás: [Hétfő: 9:00-18:00, ...]

4. "Cím geocoding..."
   └─> Nominatim API hívás
       └─> lat: 47.4983, lon: 19.0704

5. Térkép előnézet (Leaflet)
   └─> Pin a térképen: "Rendben van?"

6. "Javaslat beküldése"
   └─> POST /api/suggestions
       └─> Backend elmenti
           └─> Modal bezárul
               └─> Lista frissül

7. Közösség látja és szavaz
   └─> User2: 👍 (+1)
   └─> User3: 👍 (+2)
   └─> ...
   └─> User7: 👍 (+5) ✅ JÓVÁHAGYVA!

8. Backend automatikusan alkalmazza
   └─> Új rekord a `shops` táblában
   └─> Új rekordok `shop_opening_hours` táblában
   └─> Javaslat status → "applied"
```

---

### 7️⃣ Automatikus Jóváhagyás - PostgreSQL Trigger

```sql
CREATE TRIGGER trigger_suggestion_auto_approval
    AFTER INSERT OR UPDATE OR DELETE ON suggestion_votes
    FOR EACH ROW
    EXECUTE FUNCTION check_suggestion_auto_approval();
```

**Mit csinál?**
1. Minden szavazat után ellenőrzi a nettó szavazatot
2. Ha >= +5 és status = 'pending':
   - `UPDATE suggestions SET status = 'approved'`
3. Opcionálisan értesítést küld a szerzőnek

---

### 8️⃣ Javaslat Alkalmazása (Apply)

**Opció A: Automatikus (ajánlott)**
```typescript
// Backend: vote endpoint után
if (netVotes >= 5) {
  await updateSuggestionStatus(id, 'approved');
  await applySuggestion(id); // Azonnal alkalmazza
}
```

**Opció B: Manuális (admin dashboard)**
```
Admin dashboard → "Jóváhagyott javaslatok" lista
→ "Alkalmazás" gomb → POST /api/suggestions/:id/apply
```

**Apply logika típusonként:**

```typescript
switch (suggestion.type) {
  case 'new_shop':
    // 1. INSERT INTO shops (name, address, city, location)
    // 2. INSERT INTO shop_opening_hours (shop_id, day_id, ...)
    break;
    
  case 'edit_name':
    // UPDATE shops SET name = proposed_value WHERE id = shop_id
    break;
    
  case 'edit_address':
    // UPDATE shops SET address = ..., city = ..., location = ST_MakePoint(...)
    break;
    
  case 'edit_hours':
    // DELETE FROM shop_opening_hours WHERE shop_id = ...
    // INSERT INTO shop_opening_hours (új adatok)
    break;
}
```

---

## 🔒 Biztonság

### Authentication
- JWT token minden API híváshoz
- Token ellenőrzés middleware
- User ID kinyerése a token-ből

### Authorization
- Csak saját javaslat törölhető
- Admin szerepkör a manuális apply-hoz
- Rate limiting (max 10 javaslat/nap/user)

### Validáció
- Backend input validáció
- SQL injection védelem (parameterized queries)
- UNIQUE constraint (suggestion_id, user_id)

---

## 📊 Példa Adatok

### Javaslat Objektum (JSON)
```json
{
  "id": 1,
  "typeId": 1,
  "typeCode": "new_shop",
  "typeName": "Új dohánybolt",
  "proposedValue": "Havana Cigar Lounge",
  "additionalData": {
    "address": "Budapest, Erzsébet körút 43.",
    "city": "Budapest",
    "latitude": 47.4983,
    "longitude": 19.0704
  },
  "userId": 5,
  "userName": "cigar_lover_42",
  "statusId": 1,
  "statusCode": "pending",
  "statusName": "Függőben",
  "createdAt": "2026-02-24T10:30:00Z",
  "likes": 3,
  "dislikes": 1,
  "userVote": "like",
  "openingHours": [
    {
      "dayId": 1,
      "dayName": "Hétfő",
      "openHour": "09:00",
      "closeHour": "18:00"
    },
    {
      "dayId": 7,
      "dayName": "Vasárnap",
      "openHour": null,
      "closeHour": null
    }
  ]
}
```

---

## 🚀 Implementációs Sorrend

1. ✅ **Adatbázis migráció** (migration_suggestions_v2.sql futtatása)
2. ⏳ **Backend model** (suggestion.ts típusok)
3. ⏳ **Backend DB access** (suggestionAccess.ts)
4. ⏳ **Backend API** (suggestionRoutes.ts + handlers)
5. ⏳ **Frontend service** (suggestion.service.ts)
6. ⏳ **Frontend integráció** (community.page.ts API bekötés)
7. ⏳ **Create modal bővítés** (geocoding, térkép)
8. ⏳ **Apply logika** (automatikus alkalmazás)
9. ⏳ **Tesztelés** (E2E flow végig)

**Becsült idő:** 3-4 hét (1 full-stack fejlesztő)

---

## 📚 Kapcsolódó Dokumentumok

- 📄 **SUGGESTION_SYSTEM_PLAN.md** - Részletes implementációs terv
- 📄 **migration_suggestions_v2.sql** - Adatbázis migráció
- 📄 **BACKEND_COMMUNITY_API.md** - API specifikáció (régebbi verzió)

---

**Készült:** 2026-02-24  
**Verzió:** 2.0  
**Projekt:** SzivarClub - Közösségi Adatbázis Karban Tartás

