# 📚 Közösségi Javaslatok Rendszer - Dokumentáció

## 📖 Dokumentumok Áttekintése

### 🎯 Főbb Dokumentumok

#### 1. **SUGGESTION_SYSTEM_SUMMARY.md** ⭐ KEZDD EZZEL!
- **Célja:** Gyors áttekintés a rendszer működéséről
- **Tartalom:** 
  - Rendszer működése vizuálisan
  - Javaslat típusok
  - Életciklus diagram
  - Szavazási rendszer
  - API végpontok gyors referencia
- **Kinek:** Mindenki (projekt manager, fejlesztő, érdeklődő)
- **Olvasási idő:** 5-10 perc

#### 2. **SUGGESTION_SYSTEM_PLAN.md** 📋 IMPLEMENTÁCIÓHOZ
- **Célja:** Teljes körű implementációs terv
- **Tartalom:**
  - Jelenlegi állapot felmérése
  - Részletes adatbázis terv
  - Backend implementáció (model, routes, db access)
  - Frontend implementáció (services, components)
  - Apply logika
  - Sprint bontás
  - Kockázatok és megoldások
- **Kinek:** Fejlesztők (backend, frontend)
- **Olvasási idő:** 30-45 perc

#### 3. **SUGGESTION_SYSTEM_CHANGELOG.md** 🔄 VÁLTOZÁSOK
- **Célja:** v2.0 változtatások dokumentálása
- **Tartalom:**
  - Type és Status normalizálás részletei
  - Adatbázis változások
  - TypeScript típus változások
  - Breaking changes
  - Előnyök
- **Kinek:** Fejlesztők (aki migrálja a rendszert)
- **Olvasási idő:** 10-15 perc

### 🗄️ SQL Fájlok

#### 1. **database/migration_suggestions_v2.sql** 🚀 FŐ MIGRÁCIÓ
- **Célja:** Teljes suggestion rendszer létrehozása
- **Tartalom:**
  - Referencia táblák (suggestion_types, suggestion_statuses)
  - Fő táblák (suggestions, suggestion_opening_hours, suggestion_votes)
  - Indexek
  - Triggerek (auto-approval)
  - VIEW (suggestions_with_votes)
  - Grant jogosultságok
  - Teszt adatok (kommentelt)
- **Mikor futtasd:** Egyszer, az első telepítéskor
- **Futtatás:**
  ```bash
  psql -U develop -d szivar -f database/migration_suggestions_v2.sql
  ```

#### 2. **database/suggestion_reference_data.sql** 📝 REFERENCIA ADATOK
- **Célja:** Suggestion types és statuses feltöltése/frissítése
- **Tartalom:**
  - suggestion_types INSERT (4 típus)
  - suggestion_statuses INSERT (4 státusz)
  - ON CONFLICT DO UPDATE (újrafuttatható)
- **Mikor futtasd:** 
  - Ha új típust/státuszt adsz hozzá
  - Ha módosítod a meglévő neveket/leírásokat
- **Futtatás:**
  ```bash
  psql -U develop -d szivar -f database/suggestion_reference_data.sql
  ```

---

## 🚀 Gyors Start

### Új Projekt (nincs még suggestion rendszer)

1. **Olvasd el a SUMMARY-t**
   ```bash
   cat SUGGESTION_SYSTEM_SUMMARY.md
   ```

2. **Futtasd a migrációt**
   ```bash
   psql -U develop -d szivar -f database/migration_suggestions_v2.sql
   ```

3. **Ellenőrzés**
   ```sql
   SELECT * FROM suggestion_types;
   SELECT * FROM suggestion_statuses;
   ```

4. **Backend implementáció** (lásd PLAN.md Sprint 1-2)
5. **Frontend integráció** (lásd PLAN.md Sprint 3-4)

### Meglévő Projekt (van suggestion, de régi)

1. **Olvasd el a CHANGELOG-ot**
   ```bash
   cat SUGGESTION_SYSTEM_CHANGELOG.md
   ```

2. **Backup készítése**
   ```bash
   pg_dump -U develop -t suggestion* > backup_suggestions.sql
   ```

3. **Futtasd a migrációt**
   ```bash
   psql -U develop -d szivar -f database/migration_suggestions_v2.sql
   ```

4. **Frissítsd a backend kódot** (lásd CHANGELOG breaking changes)

---

## 📂 Fájl Struktúra

```
mesterremek/
├── SUGGESTION_SYSTEM_SUMMARY.md          ⭐ Gyors áttekintés
├── SUGGESTION_SYSTEM_PLAN.md             📋 Implementációs terv
├── SUGGESTION_SYSTEM_CHANGELOG.md        🔄 v2.0 változások
├── README_SUGGESTION_DOCS.md             📚 Ez a fájl
│
├── database/
│   ├── migration_suggestions_v2.sql      🚀 Fő migráció
│   └── suggestion_reference_data.sql     📝 Referencia adatok
│
├── backend/
│   └── src/
│       ├── model/
│       │   └── suggestion.ts             🔧 TypeScript típusok
│       ├── db/
│       │   └── suggestionAccess.ts       🔧 DB műveletek
│       └── routes/
│           ├── suggestionRoutes.ts       🔧 API routes
│           └── func/
│               └── suggestion.ts         🔧 Route handlers
│
└── frontend/
    └── SzivarClubApp/
        └── src/app/
            ├── services/
            │   ├── suggestion.service.ts  🔧 API service
            │   └── geocoding.service.ts   🔧 Geocoding
            └── pages/
                └── community/             🔧 Community page
```

---

## 🎯 Funkció Összefoglaló

### Mi a Suggestion Rendszer?

A közösségi javaslatok rendszer lehetővé teszi a felhasználók számára:
- ✅ Új dohányboltok hozzáadása
- ✅ Meglévő boltok adatainak javítása (név, cím, nyitvatartás)
- ✅ Szavazás javaslatokra (👍 like / 👎 dislike)
- ✅ **Automatikus jóváhagyás +5 nettó szavazatnál**
- ✅ Alkalmazás: adatbázis automatikus frissítése

### Javaslat Típusok

| Kód | Név | Leírás |
|-----|-----|--------|
| `new_shop` | Új dohánybolt | Új bolt hozzáadása |
| `edit_name` | Név módosítása | Bolt nevének javítása |
| `edit_address` | Cím módosítása | Bolt címének javítása |
| `edit_hours` | Nyitvatartás módosítása | Nyitvatartás frissítése |

### Javaslat Státuszok

| Kód | Név | Leírás |
|-----|-----|--------|
| `pending` | Függőben | Várakozik szavazatokra |
| `approved` | Jóváhagyva | Elérte a +5 szavazatot ✅ |
| `rejected` | Elutasítva | Admin elutasította ❌ |
| `applied` | Alkalmazva | Adatbázis frissítve 🎉 |

---

## 🔧 Technológiai Stack

- **Backend:** Node.js, Express, TypeScript
- **Database:** PostgreSQL (PostGIS)
- **Frontend:** Angular, Ionic
- **Geocoding:** OpenStreetMap Nominatim
- **Auth:** JWT

---

## 📞 Kapcsolat & Támogatás

Ha kérdésed van a rendszerrel kapcsolatban:
1. Olvasd el a **SUMMARY.md**-t (gyors válaszok)
2. Nézd meg a **PLAN.md**-t (részletes infók)
3. Ellenőrizd a **CHANGELOG.md**-t (változások)

---

## 📅 Verzió Információk

- **Verzió:** 2.0
- **Utolsó frissítés:** 2026-02-24
- **Fő változás:** Type és Status normalizálás
- **Készítette:** GitHub Copilot
- **Projekt:** SzivarClub App

---

**Happy Coding! 🚀**

