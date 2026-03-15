-- ============================================================================
-- KÖZÖSSÉGI JAVASLATOK RENDSZER - ADATBÁZIS MIGRÁCIÓ
-- ============================================================================
-- Verzió: 2.0
-- Dátum: 2026-02-24
-- Leírás: Egyszerűsített és hatékony suggestion rendszer
-- ============================================================================

-- 1. BACKUP - Régi táblák átnevezése (ha vannak adatok benne)
-- ============================================================================

-- Csak akkor futtatd, ha már vannak adatok a régi táblákban!
-- ALTER TABLE IF EXISTS public.suggestion RENAME TO suggestion_old;
-- ALTER TABLE IF EXISTS public.suggestion_ratings RENAME TO suggestion_ratings_old;
-- ALTER TABLE IF EXISTS public.suggestion_user_rating RENAME TO suggestion_user_rating_old;
-- ALTER TABLE IF EXISTS public.suggestion_type RENAME TO suggestion_type_old;

-- 2. RÉGI TÁBLÁK TÖRLÉSE (ha nincsenek adatok vagy backup után)
-- ============================================================================

DROP TABLE IF EXISTS public.suggestion_ratings CASCADE;
DROP TABLE IF EXISTS public.suggestion_user_rating CASCADE;
DROP TABLE IF EXISTS public.suggestion CASCADE;
DROP TABLE IF EXISTS public.suggestion_type CASCADE;

-- 3. ÚJ TÁBLÁK LÉTREHOZÁSA
-- ============================================================================

-- 3.1 Javaslat típusok referencia tábla
CREATE TABLE public.suggestion_types (
    id SERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT NOW()
);

COMMENT ON TABLE public.suggestion_types IS 'Javaslat típusok referencia adatok';
COMMENT ON COLUMN public.suggestion_types.code IS 'Típus kód: new_shop, edit_name, edit_address, edit_hours';

ALTER TABLE public.suggestion_types OWNER TO develop;

-- 3.2 Javaslat státuszok referencia tábla
CREATE TABLE public.suggestion_statuses (
    id SERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT NOW()
);

COMMENT ON TABLE public.suggestion_statuses IS 'Javaslat státuszok referencia adatok';
COMMENT ON COLUMN public.suggestion_statuses.code IS 'Státusz kód: pending, approved, rejected, applied';

ALTER TABLE public.suggestion_statuses OWNER TO develop;

-- 3.3 Referencia adatok feltöltése
INSERT INTO public.suggestion_types (code, name, description) VALUES
    ('new_shop', 'Új dohánybolt', 'Teljesen új dohánybolt hozzáadása az adatbázishoz'),
    ('edit_name', 'Név módosítása', 'Meglévő bolt nevének módosítása'),
    ('edit_address', 'Cím módosítása', 'Meglévő bolt címének és koordinátáinak módosítása'),
    ('edit_hours', 'Nyitvatartás módosítása', 'Meglévő bolt nyitvatartási idejének módosítása');

INSERT INTO public.suggestion_statuses (code, name, description) VALUES
    ('pending', 'Függőben', 'Javaslat várakozik szavazatokra'),
    ('approved', 'Jóváhagyva', 'Javaslat elérte a +5 szavazatot, alkalmazható'),
    ('rejected', 'Elutasítva', 'Javaslat elutasításra került (admin döntés)'),
    ('applied', 'Alkalmazva', 'Javaslat alkalmazva, adatbázis frissítve');

-- 3.4 Javaslatok fő tábla
CREATE TABLE public.suggestions (
    id SERIAL PRIMARY KEY,
    type_id INTEGER NOT NULL REFERENCES public.suggestion_types(id),
    shop_id INTEGER REFERENCES public.shops(id) ON DELETE CASCADE,
    proposed_value TEXT NOT NULL,
    additional_data JSONB,
    user_id INTEGER NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    status_id INTEGER NOT NULL DEFAULT 1 REFERENCES public.suggestion_statuses(id),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

COMMENT ON TABLE public.suggestions IS 'Közösségi javaslatok új boltokra vagy módosításokra';
COMMENT ON COLUMN public.suggestions.type_id IS 'Javaslat típusa (FK suggestion_types)';
COMMENT ON COLUMN public.suggestions.shop_id IS 'NULL ha új bolt, kitöltve ha meglévő bolt módosítása';
COMMENT ON COLUMN public.suggestions.proposed_value IS 'Javasolt főérték (bolt név, új cím, stb.)';
COMMENT ON COLUMN public.suggestions.additional_data IS 'JSON: {address, city, latitude, longitude}';
COMMENT ON COLUMN public.suggestions.status_id IS 'Javaslat státusza (FK suggestion_statuses), default: 1 = pending';

ALTER TABLE public.suggestions OWNER TO develop;

-- 3.5 Javaslatok nyitvatartási adatai
CREATE TABLE public.suggestion_opening_hours (
    id SERIAL PRIMARY KEY,
    suggestion_id INTEGER NOT NULL REFERENCES public.suggestions(id) ON DELETE CASCADE,
    day_id INTEGER NOT NULL REFERENCES public.days_of_week(id),
    open_hour TIME,
    close_hour TIME
);

COMMENT ON TABLE public.suggestion_opening_hours IS 'Javasolt nyitvatartási adatok (új boltnál vagy edit_hours típusnál)';

ALTER TABLE public.suggestion_opening_hours OWNER TO develop;

-- 3.6 Szavazatok tábla
CREATE TABLE public.suggestion_votes (
    id SERIAL PRIMARY KEY,
    suggestion_id INTEGER NOT NULL REFERENCES public.suggestions(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    vote_type VARCHAR(10) NOT NULL CHECK (vote_type IN ('like', 'dislike')),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT unique_vote UNIQUE (suggestion_id, user_id)
);

COMMENT ON TABLE public.suggestion_votes IS 'Felhasználói szavazatok javaslatokra (like/dislike)';
COMMENT ON CONSTRAINT unique_vote ON public.suggestion_votes IS 'Egy felhasználó egy javaslatra csak egyszer szavazhat';

ALTER TABLE public.suggestion_votes OWNER TO develop;

-- 4. INDEXEK - Gyorsabb lekérdezésekhez
-- ============================================================================

CREATE INDEX idx_suggestions_status_id ON public.suggestions(status_id);
CREATE INDEX idx_suggestions_type_id ON public.suggestions(type_id);
CREATE INDEX idx_suggestions_user_id ON public.suggestions(user_id);
CREATE INDEX idx_suggestions_shop_id ON public.suggestions(shop_id) WHERE shop_id IS NOT NULL;
CREATE INDEX idx_suggestions_created_at ON public.suggestions(created_at DESC);

CREATE INDEX idx_suggestion_votes_suggestion_id ON public.suggestion_votes(suggestion_id);
CREATE INDEX idx_suggestion_votes_user_id ON public.suggestion_votes(user_id);

CREATE INDEX idx_suggestion_opening_hours_suggestion_id ON public.suggestion_opening_hours(suggestion_id);

-- 5. TRIGGER - Automatikus updated_at frissítés
-- ============================================================================

CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_suggestions_updated_at
    BEFORE UPDATE ON public.suggestions
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trigger_suggestion_votes_updated_at
    BEFORE UPDATE ON public.suggestion_votes
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- 6. TRIGGER - Automatikus jóváhagyás +5 nettó szavazatnál
-- ============================================================================

CREATE OR REPLACE FUNCTION check_suggestion_auto_approval()
RETURNS TRIGGER AS $$
DECLARE
    net_votes INTEGER;
    current_status_id INTEGER;
    pending_status_id INTEGER;
    approved_status_id INTEGER;
BEGIN
    -- Lekérdezzük a státusz ID-kat
    SELECT id INTO pending_status_id FROM public.suggestion_statuses WHERE code = 'pending';
    SELECT id INTO approved_status_id FROM public.suggestion_statuses WHERE code = 'approved';

    -- Lekérdezzük a javaslat jelenlegi státuszát
    SELECT status_id INTO current_status_id
    FROM public.suggestions
    WHERE id = COALESCE(NEW.suggestion_id, OLD.suggestion_id);

    -- Csak pending javaslatokat ellenőrzünk
    IF current_status_id = pending_status_id THEN
        -- Nettó szavazat számítás
        SELECT
            COUNT(CASE WHEN vote_type = 'like' THEN 1 END) -
            COUNT(CASE WHEN vote_type = 'dislike' THEN 1 END)
        INTO net_votes
        FROM public.suggestion_votes
        WHERE suggestion_id = COALESCE(NEW.suggestion_id, OLD.suggestion_id);

        -- Ha eléri a +5-öt, automatikus jóváhagyás
        IF net_votes >= 5 THEN
            UPDATE public.suggestions
            SET status_id = approved_status_id, updated_at = NOW()
            WHERE id = COALESCE(NEW.suggestion_id, OLD.suggestion_id)
            AND status_id = pending_status_id;

            -- TODO: Értesítés küldése a szerzőnek (opcionális)
            -- INSERT INTO notifications (user_id, message, ...) VALUES (...);
        END IF;
    END IF;

    IF TG_OP = 'DELETE' THEN
        RETURN OLD;
    ELSE
        RETURN NEW;
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_suggestion_auto_approval
    AFTER INSERT OR UPDATE OR DELETE ON public.suggestion_votes
    FOR EACH ROW
    EXECUTE FUNCTION check_suggestion_auto_approval();

COMMENT ON FUNCTION check_suggestion_auto_approval() IS 'Automatikusan approved státuszra állítja a javaslatot, ha eléri a +5 nettó szavazatot';

-- 7. VIEW - Javaslatok kiterjesztett nézettel (számított mezőkkel)
-- ============================================================================

CREATE OR REPLACE VIEW public.suggestions_with_votes AS
SELECT
    s.id,
    s.type_id,
    st.code as type_code,
    st.name as type_name,
    s.shop_id,
    s.proposed_value,
    s.additional_data,
    s.user_id,
    u.name as user_name,
    s.status_id,
    ss.code as status_code,
    ss.name as status_name,
    s.created_at,
    s.updated_at,
    sh.name as shop_name,
    sh.address as shop_address,
    sh.city as shop_city,
    COALESCE(
        (SELECT COUNT(*) FROM public.suggestion_votes sv
         WHERE sv.suggestion_id = s.id AND sv.vote_type = 'like'),
        0
    ) as likes,
    COALESCE(
        (SELECT COUNT(*) FROM public.suggestion_votes sv
         WHERE sv.suggestion_id = s.id AND sv.vote_type = 'dislike'),
        0
    ) as dislikes,
    COALESCE(
        (SELECT COUNT(*) FROM public.suggestion_votes sv
         WHERE sv.suggestion_id = s.id AND sv.vote_type = 'like'),
        0
    ) - COALESCE(
        (SELECT COUNT(*) FROM public.suggestion_votes sv
         WHERE sv.suggestion_id = s.id AND sv.vote_type = 'dislike'),
        0
    ) as net_votes
FROM public.suggestions s
JOIN public.users u ON s.user_id = u.id
JOIN public.suggestion_types st ON s.type_id = st.id
JOIN public.suggestion_statuses ss ON s.status_id = ss.id
LEFT JOIN public.shops sh ON s.shop_id = sh.id;

COMMENT ON VIEW public.suggestions_with_votes IS 'Javaslatok kiterjesztett nézet szavazatokkal, felhasználó adatokkal, típussal és státusszal';

-- 8. TESZT ADATOK (opcionális - csak fejlesztéshez)
-- ============================================================================

-- Feltételezzük, hogy van user_id = 1 és shop_id = 1
-- Kommenteld ki éles környezetben!

/*
-- Teszt javaslat: új bolt
-- type_id = 1 (new_shop)
INSERT INTO public.suggestions (type_id, proposed_value, additional_data, user_id, status_id)
VALUES (
    1,
    'Havana Cigar Lounge',
    '{"address": "Budapest, Erzsébet körút 43.", "city": "Budapest", "latitude": 47.4983, "longitude": 19.0704}'::jsonb,
    1,
    1  -- pending
);

-- Teszt javaslat: név módosítás
-- type_id = 2 (edit_name)
INSERT INTO public.suggestions (type_id, shop_id, proposed_value, user_id, status_id)
VALUES (
    2,
    1,
    'Dohánybolt Újpest - Megújult név',
    1,
    1  -- pending
);

-- Teszt szavazatok
INSERT INTO public.suggestion_votes (suggestion_id, user_id, vote_type)
VALUES
    (1, 1, 'like'),
    (1, 2, 'like'),
    (1, 3, 'dislike');
*/

-- 9. GRANT JOGOSULTSÁGOK
-- ============================================================================

GRANT SELECT ON public.suggestion_types TO develop;
GRANT SELECT ON public.suggestion_statuses TO develop;

GRANT SELECT, INSERT, UPDATE, DELETE ON public.suggestions TO develop;
GRANT SELECT, INSERT, UPDATE, DELETE ON public.suggestion_opening_hours TO develop;
GRANT SELECT, INSERT, UPDATE, DELETE ON public.suggestion_votes TO develop;
GRANT SELECT ON public.suggestions_with_votes TO develop;

GRANT USAGE, SELECT ON SEQUENCE public.suggestion_types_id_seq TO develop;
GRANT USAGE, SELECT ON SEQUENCE public.suggestion_statuses_id_seq TO develop;
GRANT USAGE, SELECT ON SEQUENCE public.suggestions_id_seq TO develop;
GRANT USAGE, SELECT ON SEQUENCE public.suggestion_opening_hours_id_seq TO develop;
GRANT USAGE, SELECT ON SEQUENCE public.suggestion_votes_id_seq TO develop;

-- ============================================================================
-- MIGRÁCIÓ VÉGE
-- ============================================================================

-- Ellenőrzés:
-- SELECT * FROM public.suggestions_with_votes;
-- SELECT tablename, schemaname FROM pg_tables WHERE schemaname = 'public' AND tablename LIKE 'suggest%';

