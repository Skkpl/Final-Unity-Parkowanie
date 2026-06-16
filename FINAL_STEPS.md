# Finalne kroki w Unity

Repozytorium GitHub: https://github.com/Skkpl/Final-Unity-Parkowanie.git

## Metryczka do oddania

- Szymon Karamon, 91859, 40% - integracja Unity, sceny, repozytorium GitHub, testy koncowe.
- Bartosz Stolarczyk, 91742, 30% - FSM, sensory, walidacja miejsc, HUD debug.
- Antoni Krakowiak, 88437, 30% - uklady map, trajektorie, przeszkoda dynamiczna, wyglad pojazdow.

1. Zamknij Unity.
2. Uruchom `Install-ToUnityProject.ps1` z tej paczki albo recznie skopiuj:
   - `Assets/Scripts` do `Automatic parking/Assets/Scripts`
   - `Assets/Editor` do `Automatic parking/Assets/Editor`
3. Otworz Unity.
4. Poczekaj az kompilacja skryptow sie zakonczy.
5. Wybierz `Tools -> Parking Project -> Build Demo Scenes`.
6. Otworz `Assets/Scenes/MainMenu.unity`.
7. Uruchom Play.

## Co sprawdzic przed nagraniem

- Mapa 1: auto ignoruje falszywa luke i parkuje w nastepnej.
- Mapa 2: auto szuka luki rownoleglej i wykonuje manewr tylem.
- Mapa 3: scena jak mapa 1, ale miejsca sa po lewej; czerwony pojazd jedzie z naprzeciwka, nasze auto czeka, potem podjezdza do lewej krawedzi i cofa w miejsce.
- W lewym gornym rogu widac HUD ze stanem FSM.
- W Scene View widac promienie czujnikow.

## Jesli widzisz folder `Assets/Assets`

To efekt wklejenia calego folderu `Assets` do srodka istniejacego `Assets`. Nie jest potrzebny do finalnej wersji.
Najbezpieczniej zostawic go, jesli projekt dziala. Do porzadkowania mozna go usunac dopiero po zrobieniu kopii projektu.
