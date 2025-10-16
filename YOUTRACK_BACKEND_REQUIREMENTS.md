# Backend API Követelmények - Szivar Club App

## Áttekintés
A frontend Ionic Angular alkalmazás (standalone komponensek) JWT alapú autentikációt használ. A backend API-nak Node.js/Express alapon kell működnie, PostgreSQL adatbázissal.

**Backend URL:** `http://localhost:3000/api`

---

## 1. Autentikáció Endpointok (`/api/auth`)

### 1.1 POST `/api/auth/register`
**Leírás:** Új felhasználó regisztrálása

**Request Body:**
```json
{
  "email": "string (kötelező, unique)",
  "password": "string (kötelező, min 6 karakter)",
  "name": "string (kötelező)"
}
```

**Response (201 Created):**
```json
{
  "message": "Registration successful",
  "userId": "number"
}
```

**Hibakezelés:**
- 400: Hiányzó vagy érvénytelen mezők
- 409: Email már létezik
- 500: Szerver hiba

**Backend feladatok:**
- Email validálás és uniqueness ellenőrzés
- Jelszó hash-elés (bcrypt vagy argon2)
- `users` táblába beszúrás (role: 'user', created_at: timestamp)

---

### 1.2 POST `/api/auth/login`
**Leírás:** Felhasználó bejelentkeztetése

**Request Body:**
```json
{
  "email": "string (kötelező)",
  "password": "string (kötelező)"
}
```

**Response (200 OK):**
```json
{
  "token": "string (JWT token)",
  "user": {
    "id": "number",
    "email": "string",
    "name": "string",
    "role": "string (user/admin)"
  }
}
```

**Hibakezelés:**
- 400: Hiányzó mezők
- 401: Érvénytelen email vagy jelszó
- 500: Szerver hiba

**JWT token tartalma (payload):**
```json
{
  "userId": "number",
  "email": "string",
  "name": "string",
  "role": "string",
  "exp": "number (lejárati időpont)"
}
```

**Token lejárat:** 7 nap (vagy konfigurálható)

---

## 2. JWT Token Kezelés

### Frontend működése:
- **Token tárolás:** `localStorage` (`auth_token` kulccsal)
- **HTTP Interceptor:** Minden API kéréshez automatikusan hozzáadja a tokent:
  ```
  Authorization: Bearer <token>
  ```
  Kivéve: `/api/auth/login` és `/api/auth/register`

### Backend követelmények:
- **Middleware:** Token validálás védett endpointoknál
- Token dekódolás és ellenőrzés (lejárat, signature)
- `req.user` objektum beállítása a dekódolt adatokkal
- 401 Unauthorized válasz érvénytelen/lejárt token esetén

---

## 3. Frontend Route Struktúra

### Publikus route-ok (nincs védelem):
- `/login` - Bejelentkezés oldal
- `/register` - Regisztráció oldal

### Védett route-ok (authGuard - token szükséges):
- `/tabs/home` - Főoldal (dashboard)
- `/tabs/map` - Térkép (dohányboltok)
- `/tabs/settings` - Beállítások

**AuthGuard működése:**
- Ellenőrzi a token létezését és érvényességét
- Lejárt/hiányzó token esetén átirányít `/login`-ra

---

## 4. Szerepkör-alapú Jogosultságok

### Frontend role ellenőrzés:
```typescript
authService.getUserRole() // 'user' vagy 'admin'
authService.isAdmin() // boolean
```

### Backend követelmények:
- Admin szerepkörű felhasználók számára extra jogosultságok:
  - Boltok módosítása/törlése
  - Szivar elérhetőség megerősítése (`confirmed` flag)
  - Felhasználói feltöltések moderálása (fotók, review-k)
  - Statisztikák megtekintése

**Middleware:** `adminOnly` middleware létrehozása védett admin endpointokhoz

---

## 5. Adatbázis Séma (PostgreSQL)

### Főbb táblák a `szivar.sql` alapján:

#### `users`
```sql
- id (INT, PRIMARY KEY, AUTO INCREMENT)
- name (varchar)
- email (varchar, UNIQUE)
- password_hash (text) -- bcrypt hash
- role (varchar) -- 'user' vagy 'admin'
- created_at (timestamp)
```

#### `shops`
```sql
- id (INT, PRIMARY KEY)
- name (varchar)
- address (text)
- city (varchar)
- latitude (decimal)
- longitude (decimal)
- created_at, updated_at (timestamp)
```

#### További táblák:
- `cigar_availability` - Szivar elérhetőség jelölése
- `user_actions` - Felhasználói aktivitások
- `shop_photos` - Bolt fotók
- `cigar_brands` - Szivar márkák
- `shop_cigar_brands` - Kapcsolótábla
- `reviews` - Értékelések (1-5 csillag)
- `cigars` - Szivar részletes adatok
- `shop_opening_hours` - Nyitvatartási idők

---

## 6. Környezeti Változók

**Backend `.env` fájl szükséges:**
```env
PORT=3000
DATABASE_URL=postgresql://user:password@localhost:5432/szivar_db
JWT_SECRET=<biztonságos random string>
JWT_EXPIRATION=7d
```

---

## 7. CORS Beállítások

**Frontend origin:** `http://localhost:8100` (Ionic dev server)

**Backend CORS konfiguráció:**
```javascript
app.use(cors({
  origin: 'http://localhost:8100',
  credentials: true
}));
```

**Production:** Konfigurálható origin lista (később)

---

## 8. Hibaüzenet Formátum

**Egységes error response:**
```json
{
  "error": "string (rövid leírás)",
  "message": "string (részletes üzenet)",
  "statusCode": "number"
}
```

---

## 9. Jelenlegi Backend Státusz

**Meglévő fájlok:**
- `backend/src/main.ts` - Express szerver alapvázat
- `backend/src/routes/userRoutes.ts` - Csak egy teszt route (`GET /users`)

**Hiányzó komponensek:**
- ❌ `/api/auth/register` endpoint
- ❌ `/api/auth/login` endpoint
- ❌ JWT token generálás és validálás
- ❌ Auth middleware
- ❌ PostgreSQL kapcsolat és modellek
- ❌ Jelszó hash-elés
- ❌ CORS konfiguráció
- ❌ Környezeti változók kezelés (.env)
- ❌ Error handling middleware

---

## 10. Szükséges NPM Csomagok

**Backend dependencies:**
```bash
npm install express
npm install pg          # PostgreSQL driver
npm install bcrypt      # Jelszó hash-elés
npm install jsonwebtoken # JWT token kezelés
npm install dotenv      # Környezeti változók
npm install cors        # CORS middleware
npm install express-validator # Input validálás
```

**DevDependencies:**
```bash
npm install --save-dev @types/express
npm install --save-dev @types/node
npm install --save-dev @types/bcrypt
npm install --save-dev @types/jsonwebtoken
npm install --save-dev @types/cors
npm install --save-dev typescript
npm install --save-dev ts-node
npm install --save-dev nodemon
```

---

## 11. Fejlesztési Prioritások

### Phase 1 - Autentikáció (KRITIKUS)
1. PostgreSQL kapcsolat beállítása
2. User model és database helper függvények
3. POST `/api/auth/register` implementálása
4. POST `/api/auth/login` implementálása
5. JWT middleware (token validálás)
6. CORS és error handling

### Phase 2 - További API-k
- Boltok CRUD műveletek (`/api/shops`)
- Térkép adatok lekérése (koordináták alapján)
- User settings módosítás
- Admin funkciók

---

## 12. Tesztelési Jegyzet

**Tesztelendő eszközökkel (Postman/Thunder Client):**
- ✅ Sikeres regisztráció
- ✅ Duplikált email hibakezelés
- ✅ Sikeres login és token visszaadás
- ✅ Helytelen jelszó hibakezelés
- ✅ Token validálás védett endpointokon
- ✅ Lejárt token elutasítása
- ✅ Admin/user role ellenőrzés

---

## 13. Frontend-Backend Kapcsolat Összefoglalás

| Frontend Funkció | Backend Endpoint | Metódus | Auth |
|-----------------|------------------|---------|------|
| Regisztráció | `/api/auth/register` | POST | ❌ |
| Bejelentkezés | `/api/auth/login` | POST | ❌ |
| Főoldal betöltés | `/api/user/profile` | GET | ✅ |
| Térkép boltok | `/api/shops` | GET | ✅ |
| Beállítások | `/api/user/settings` | GET/PUT | ✅ |

**Auth = ✅:** JWT token szükséges a kéréshez (Authorization header)

---

## Kapcsolattartó Információk
- **Frontend repo:** `frontend/SzivarClubApp`
- **Backend repo:** `backend`
- **Database schema:** `database/szivar.sql`

**Kérdések esetén konzultálj a frontend AuthService implementációjával!**

