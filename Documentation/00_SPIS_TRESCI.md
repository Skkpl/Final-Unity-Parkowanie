# Spis tresci dokumentacji

Projekt: Automatyczne parkowanie w Unity 3D

Data aktualizacji: 2026-06-16

## Metryczka

| Osoba | Nr albumu | Udzial |
| --- | ---: | ---: |
| Szymon Karamon | 91859 | 40% |
| Bartosz Stolarczyk | 91742 | 30% |
| Antoni Krakowiak | 88437 | 30% |

Pelny podzial zadan znajduje sie w `CONTRIBUTORS.md` oraz w `01_Dokumentacja_techniczna.md`.

## Pliki dokumentacji

1. `01_Dokumentacja_techniczna.md`
   - pelny opis techniczny projektu,
   - architektura,
   - klasy,
   - FSM,
   - sensory,
   - scenariusze,
   - generator scen,
   - ograniczenia.

2. `02_Jak_stworzyc_od_zera.md`
   - instrukcja stworzenia podobnego projektu krok po kroku,
   - konfiguracja Unity,
   - tworzenie auta,
   - tworzenie sensorow,
   - tworzenie FSM,
   - tworzenie map,
   - strojenie punktow parkowania.

3. `03_Testy_i_uruchomienie.md`
   - instalacja,
   - uruchomienie,
   - testy map 1, 2 i 3,
   - kryteria zaliczenia,
   - typowe problemy.

4. `04_Opis_do_obrony.md`
   - krotka sciaga do rozmowy z prowadzacym,
   - odpowiedzi na pytania,
   - opis map,
   - opis uproszczen.

5. `05_Mapa_plikow_i_parametrow.md`
   - gdzie w kodzie zmienic predkosc,
   - gdzie zmienic sensory,
   - gdzie zmienic tor parkowania,
   - gdzie zmienic czerwone auto,
   - gdzie zmienic UI i HUD.

## Najwazniejsze do przeczytania przed obrona

Jesli masz malo czasu:

1. Przeczytaj `04_Opis_do_obrony.md`.
2. Przeczytaj sekcje o FSM w `01_Dokumentacja_techniczna.md`.
3. Przeczytaj test mapy 3 w `03_Testy_i_uruchomienie.md`.
4. Zapamietaj, ze finalny manewr dziala deterministycznie "po szynach", a sensory i FSM sluza do pokazania logiki automatycznego parkowania.
