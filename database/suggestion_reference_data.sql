-- ============================================================================
-- KÖZÖSSÉGI JAVASLATOK RENDSZER - REFERENCIA ADATOK
-- ============================================================================
-- Verzió: 2.0
-- Dátum: 2026-02-24
-- Leírás: Suggestion types és statuses referencia adatok
-- ============================================================================

-- Javaslat típusok
-- ============================================================================

-- Meglévő adatok törlése (csak ha újra futtatod)
-- DELETE FROM public.suggestion_types;

INSERT INTO public.suggestion_types (id, code, name, description) VALUES
    (1, 'new_shop', 'Új dohánybolt', 'Teljesen új dohánybolt hozzáadása az adatbázishoz'),
    (2, 'edit_name', 'Név módosítása', 'Meglévő bolt nevének módosítása'),
    (3, 'edit_address', 'Cím módosítása', 'Meglévő bolt címének és koordinátáinak módosítása'),
    (4, 'edit_hours', 'Nyitvatartás módosítása', 'Meglévő bolt nyitvatartási idejének módosítása')
ON CONFLICT (code) DO UPDATE SET
    name = EXCLUDED.name,
    description = EXCLUDED.description;

-- Sequence resetelése
SELECT setval('public.suggestion_types_id_seq', (SELECT MAX(id) FROM public.suggestion_types));

-- ============================================================================

-- Javaslat státuszok
-- ============================================================================

-- Meglévő adatok törlése (csak ha újra futtatod)
-- DELETE FROM public.suggestion_statuses;

INSERT INTO public.suggestion_statuses (id, code, name, description) VALUES
    (1, 'pending', 'Függőben', 'Javaslat várakozik szavazatokra'),
    (2, 'approved', 'Jóváhagyva', 'Javaslat elérte a +5 szavazatot, alkalmazható'),
    (3, 'rejected', 'Elutasítva', 'Javaslat elutasításra került (admin döntés)'),
    (4, 'applied', 'Alkalmazva', 'Javaslat alkalmazva, adatbázis frissítve')
ON CONFLICT (code) DO UPDATE SET
    name = EXCLUDED.name,
    description = EXCLUDED.description;

-- Sequence resetelése
SELECT setval('public.suggestion_statuses_id_seq', (SELECT MAX(id) FROM public.suggestion_statuses));

-- ============================================================================

-- Ellenőrzés
-- ============================================================================

SELECT 'Suggestion Types:' as info;
SELECT * FROM public.suggestion_types ORDER BY id;

SELECT 'Suggestion Statuses:' as info;
SELECT * FROM public.suggestion_statuses ORDER BY id;

-- ============================================================================
-- VÉGÉ
-- ============================================================================

