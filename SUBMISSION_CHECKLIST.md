# Checklist oddania projektu

## Repozytorium GitHub

- [x] Repozytorium publiczne: https://github.com/Skkpl/Final-Unity-Parkowanie.git
- [x] Kod zrodlowy znajduje sie w folderze `Assets`.
- [x] Dokumentacja znajduje sie w `README.md`, `README.pdf` oraz folderze `Documentation`.
- [x] Plik `.gitignore` jest przygotowany pod Unity i ignoruje foldery generowane przez edytor.
- [x] Metryczka zespolu jest w `README.md`, `CONTRIBUTORS.md` i dokumentacji technicznej.

## Paczka Unity

- [ ] W Unity zaznaczyc foldery z wlasnymi skryptami/prefabami.
- [ ] Prawy przycisk myszy -> `Export Package`.
- [ ] Eksportowac tylko wlasne elementy projektu, bez standardowych assetow Unity.
- [ ] Nazwac paczke np. `Final-Unity-Parkowanie.unitypackage`.

## Build

- [ ] Otworzyc scene `Assets/Scenes/MainMenu.unity`.
- [ ] Sprawdzic, czy dzialaja przyciski `Map 1`, `Map 2`, `Map 3`.
- [ ] W `File -> Build Settings` dodac sceny:
  - `MainMenu`
  - `Map_01_Perpendicular`
  - `Map_02_Parallel`
  - `Map_03_Dynamic`
- [ ] Zbudowac wersje Windows `.exe`.

## Film 2-3 min

- [ ] Pokazac uruchomienie menu.
- [ ] Pokazac mape 1: ominiecie za waskiej luki i parkowanie prostopadle.
- [ ] Pokazac mape 2: parkowanie rownolegle.
- [ ] Pokazac mape 3: czerwone auto z naprzeciwka, zatrzymanie i parkowanie po lewej stronie.
- [ ] Pokazac przez chwile Scene View z promieniami `Debug.DrawRay`.

## Dokumentacja

- [x] Metryczka zespolu i procentowy podzial prac.
- [x] Diagram FSM.
- [x] Opis sensorow i decyzji projektowych.
- [x] Instrukcja uruchomienia.
- [x] Znane problemy i ograniczenia modelu.
