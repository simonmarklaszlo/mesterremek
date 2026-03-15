# Változások Log - Javaslatok Rendszer v2.0

## Dátum: 2026-02-24

## 🔄 Fő Változtatás

A `type` és `status` mezők **normalizálva** lettek külön referencia táblákba. Ez jobb adatbázis dizájn és könnyebb karbantartást tesz lehetővé.

---

## 📋 Változtatások Részletesen

### 1. Új Táblák

#### `suggestion_types` (Javaslat típusok referencia tábla)
```sql
CREATE TABLE public.suggestion_types (
    id SERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT NOW()
);
```

**Adatok:**
| id | code | name | description |
|----|------|------|-------------|
| 1 | new_shop | Új dohánybolt | Teljesen új dohánybolt hozzáadása |
| 2 | edit_name | Név módosítása | Meglévő bolt nevének módosítása |
| 3 | edit_address | Cím módosítása | Meglévő bolt címének módosítása |
| 4 | edit_hours | Nyitvatartás módosítása | Nyitvatartási idő módosítása |

#### `suggestion_statuses` (Javaslat státuszok referencia tábla)
```sql
CREATE TABLE public.suggestion_statuses (
    id SERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT NOW()
);
```

**Adatok:**
| id | code | name | description |
|----|------|------|-------------|
| 1 | pending | Függőben | Várakozik szavazatokra |
| 2 | approved | Jóváhagyva | Elérte a +5 szavazatot |
| 3 | rejected | Elutasítva | Admin elutasította |
| 4 | applied | Alkalmazva | Adatbázis frissítve |

### 2. Módosított Tábla: `suggestions`

**ELŐTTE:**
```sql
CREATE TABLE suggestions (
    type VARCHAR(50) NOT NULL,
    status VARCHAR(20) DEFAULT 'pending',
    ...
);
```

**UTÁNA:**
```sql
CREATE TABLE suggestions (
    type_id INTEGER NOT NULL REFERENCES suggestion_types(id),
    status_id INTEGER NOT NULL DEFAULT 1 REFERENCES suggestion_statuses(id),
    ...
);
```

### 3. Módosított Indexek

**ELŐTTE:**
```sql
CREATE INDEX idx_suggestions_status ON suggestions(status);
CREATE INDEX idx_suggestions_type ON suggestions(type);
```

**UTÁNA:**
```sql
CREATE INDEX idx_suggestions_status_id ON suggestions(status_id);
CREATE INDEX idx_suggestions_type_id ON suggestions(type_id);
```

### 4. Módosított VIEW: `suggestions_with_votes`

Most már JOIN-olja a `suggestion_types` és `suggestion_statuses` táblákat:

```sql
SELECT 
    s.id,
    s.type_id,
    st.code as type_code,
    st.name as type_name,
    s.status_id,
    ss.code as status_code,
    ss.name as status_name,
    ...
FROM suggestions s
JOIN suggestion_types st ON s.type_id = st.id
JOIN suggestion_statuses ss ON s.status_id = ss.id
...
```

### 5. Módosított Trigger: `check_suggestion_auto_approval()`

Most ID-kkel dolgozik VARCHAR helyett:

```sql
DECLARE
    pending_status_id INTEGER;
    approved_status_id INTEGER;
BEGIN
    SELECT id INTO pending_status_id 
    FROM suggestion_statuses WHERE code = 'pending';
    
    SELECT id INTO approved_status_id 
    FROM suggestion_statuses WHERE code = 'approved';
    
    IF current_status_id = pending_status_id THEN
        ...
        IF net_votes >= 5 THEN
            UPDATE suggestions 
            SET status_id = approved_status_id
            WHERE id = ... AND status_id = pending_status_id;
        END IF;
    END IF;
END;
```

---

## 💾 Módosított Fájlok

### 1. `SUGGESTION_SYSTEM_PLAN.md`
- ✅ Adatbázis struktúra frissítve
- ✅ TypeScript típusok frissítve
- ✅ `SuggestionType` és `SuggestionStatus` típusok hozzáadva
- ✅ `Suggestion` típus kiegészítve `typeId`, `statusId` mezőkkel

### 2. `database/migration_suggestions_v2.sql`
- ✅ `suggestion_types` tábla létrehozása
- ✅ `suggestion_statuses` tábla létrehozása
- ✅ Referencia adatok INSERT-je
- ✅ `suggestions` tábla módosítva (type_id, status_id)
- ✅ Indexek módosítva
- ✅ Trigger frissítve ID-kkal való munkához
- ✅ VIEW frissítve JOIN-okkal
- ✅ GRANT jogosultságok kiegészítve

### 3. `SUGGESTION_SYSTEM_SUMMARY.md`
- ✅ Adatbázis diagram frissítve
- ✅ Példa JSON frissítve

### 4. `database/suggestion_reference_data.sql` (ÚJ)
- ✅ Külön fájl a referencia adatok kezeléséhez
- ✅ ON CONFLICT DO UPDATE támogatás (újrafuttatható)

---

## 🎯 Előnyök

### 1. **Adatintegritás**
- Nem lehet elírni a type vagy status értékeket
- Foreign key constraintek biztosítják a konzisztenciát
- Centralizált adat menedzsment

### 2. **Könnyebb Karbantartás**
- Új típus/státusz hozzáadása egyszerű (csak INSERT)
- Névmódosítás egy helyen történik
- Lefordítás/lokalizáció egyszerű

### 3. **Jobb Teljesítmény**
- Integer JOIN gyorsabb mint VARCHAR
- Indexek hatékonyabbak
- Kisebb storage igény

### 4. **Bővíthetőség**
- Könnyen hozzáadhatók új mezők (ikon, szín, sorrend)
- Státusz gép (workflow) implementálható
- Jogosultság-kezelés típusonként

---

## 📝 Backend TypeScript Változások

### Model: `backend/src/model/suggestion.ts`

```typescript
// ÚJ típusok
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

// FRISSÍTETT Suggestion típus
export type Suggestion = {
  id: number;
  typeId: number;              // ÚJ
  type?: SuggestionType;       // ÚJ (JOIN-olt)
  typeCode?: string;           // ÚJ (gyors hozzáférés)
  statusId: number;            // ÚJ
  status?: SuggestionStatus;   // ÚJ (JOIN-olt)
  statusCode?: string;         // ÚJ (gyors hozzáférés)
  // ... többi mező változatlan
};
```

### Database Access példa

```typescript
// Javaslat létrehozása - type code-ot kell type_id-ra konvertálni
async function createSuggestion(data: CreateSuggestionRequest, userId: number) {
  // 1. Type code -> type_id konverzió
  const typeResult = await pool.query(
    'SELECT id FROM suggestion_types WHERE code = $1',
    [data.type]
  );
  const typeId = typeResult.rows[0].id;
  
  // 2. Javaslat beszúrása
  const result = await pool.query(
    `INSERT INTO suggestions (type_id, shop_id, proposed_value, additional_data, user_id)
     VALUES ($1, $2, $3, $4, $5) RETURNING id`,
    [typeId, data.shopId, data.proposedValue, data.additionalData, userId]
  );
  
  return result.rows[0].id;
}

// Javaslatok lekérdezése - JOIN-olja a referencia táblákat
async function getAllSuggestions(userId: number) {
  const result = await pool.query(`
    SELECT 
      s.*,
      st.code as type_code,
      st.name as type_name,
      ss.code as status_code,
      ss.name as status_name,
      u.name as user_name
    FROM suggestions s
    JOIN suggestion_types st ON s.type_id = st.id
    JOIN suggestion_statuses ss ON s.status_id = ss.id
    JOIN users u ON s.user_id = u.id
    ORDER BY s.created_at DESC
  `);
  
  return result.rows;
}
```

---

## 🚀 Telepítési Lépések

### 1. Backup
```sql
-- Backup készítése (ha van adat)
pg_dump -U develop -t suggestions* > backup_suggestions.sql
```

### 2. Migráció futtatása
```bash
psql -U develop -d szivar -f database/migration_suggestions_v2.sql
```

### 3. Ellenőrzés
```sql
-- Táblák ellenőrzése
SELECT tablename FROM pg_tables 
WHERE schemaname = 'public' AND tablename LIKE 'suggest%';

-- Referencia adatok ellenőrzése
SELECT * FROM suggestion_types;
SELECT * FROM suggestion_statuses;

-- VIEW ellenőrzése
SELECT * FROM suggestions_with_votes LIMIT 1;
```

---

## ⚠️ Breaking Changes

### API Request Body
**ELŐTTE:**
```json
{
  "type": "new_shop",
  "status": "pending"
}
```

**UTÁNA:** (maradt ugyanaz, a backend konvertál)
```json
{
  "type": "new_shop"  // Backend: code -> type_id konverzió
}
```

### API Response
**ELŐTTE:**
```json
{
  "id": 1,
  "type": "new_shop",
  "status": "pending"
}
```

**UTÁNA:**
```json
{
  "id": 1,
  "typeId": 1,
  "typeCode": "new_shop",
  "typeName": "Új dohánybolt",
  "statusId": 1,
  "statusCode": "pending",
  "statusName": "Függőben"
}
```

---

## 📚 Referencia

- **Migration SQL:** `database/migration_suggestions_v2.sql`
- **Reference Data SQL:** `database/suggestion_reference_data.sql`
- **Részletes terv:** `SUGGESTION_SYSTEM_PLAN.md`
- **Rövid összefoglaló:** `SUGGESTION_SYSTEM_SUMMARY.md`

---

**Verzió:** 2.0  
**Dátum:** 2026-02-24  
**Készítette:** GitHub Copilot  
**Projekt:** SzivarClub - Közösségi Javaslatok Rendszer

