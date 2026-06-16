# Testy, uruchomienie i kryteria zaliczenia

## 1. Instalacja paczki

Zalecana metoda:

1. Zamknij Unity.
2. Wypakuj paczke.
3. Uruchom `Install-ToUnityProject.ps1`.
4. Otworz Unity.
5. Poczekaj na kompilacje.
6. Kliknij:

```text
Tools -> Parking Project -> Build Demo Scenes
```

7. Otworz:

```text
Assets/Scenes/MainMenu.unity
```

8. Uruchom Play.

## 2. Dlaczego trzeba kliknac Build Demo Scenes

Skrypty same nie zmieniaja istniejacych scen. Generator musi:

- odbudowac pozycje aut,
- odbudowac punkty trajektorii,
- odbudowac UI,
- zapisac sceny.

Jesli po instalacji nie klikniesz `Build Demo Scenes`, Unity moze nadal uruchamiac stara wersje mapy.

## 3. Test ogolny

Po starcie projektu sprawdz:

- czy menu ma trzy przyciski,
- czy kazda mapa sie laduje,
- czy widac HUD w lewym gornym rogu,
- czy auto nie wypada poza droge,
- czy po zaparkowaniu HUD pokazuje `Parked`.

## 4. Test mapy 1

Nazwa sceny:

```text
Map_01_Perpendicular
```

Cel:

- parking prostopadly,
- auto ma zaparkowac w miejscu docelowym,
- za waska luka ma byc tylko elementem demonstracyjnym.

Oczekiwany przebieg:

1. Auto startuje z dolu mapy.
2. Jedzie wzdluz drogi.
3. HUD pokazuje `Scan`.
4. Po dojechaniu do punktu akceptacji przechodzi do `ValidateSpot`.
5. Potem przechodzi przez:
   - `Positioning`,
   - `ReverseTurn`,
   - `CounterTurn`,
   - `Straighten`.
6. Konczy w `Parked`.

Co sprawdzic wizualnie:

- auto stoi w miejscu docelowym,
- nie zatrzymuje sie w za waskiej luce,
- nie obraca sie w miejscu,
- nie zahacza zaparkowanych aut.

## 5. Test mapy 2

Nazwa sceny:

```text
Map_02_Parallel
```

Cel:

- parkowanie rownolegle,
- manewr cofania i kontry.

Oczekiwany przebieg:

1. Auto jedzie ulica.
2. Dojezdza do punktu akceptacji.
3. Cofa po kilku punktach.
4. Wykonuje kontre.
5. Prostuje sie.
6. Konczy w `Parked`.

Co sprawdzic:

- przod auta nie uderza w auto przed nim,
- auto nie konczy krzywo,
- finalnie stoi rownolegle do drogi.

## 6. Test mapy 3

Nazwa sceny:

```text
Map_03_Dynamic
```

Cel:

- parking po lewej stronie,
- czerwone auto jedzie z naprzeciwka,
- niebieskie auto ma zaczekac,
- potem ma zaparkowac po lewej.

Oczekiwany przebieg:

1. Niebieskie auto rusza.
2. Czerwone auto jedzie z naprzeciwka.
3. Niebieskie auto przechodzi do `EmergencyStop`.
4. Czerwone auto przejezdza.
5. Niebieskie auto kontynuuje.
6. Niebieskie auto parkuje po lewej.
7. Konczy w `Parked`.

Finalne miejsce:

- linie miejsca sa przy `z = -10.1` oraz `z = -6.7`,
- srodek miejsca to `z = -8.4`,
- finalny punkt auta to `(-5.62, 0, -8.4)`, kat `90`.

Co sprawdzic:

- niebieskie auto nie stoi na linii,
- niebieskie auto nie zahacza auta zaparkowanego obok,
- czerwone auto nie zderza sie z niebieskim,
- po zatrzymaniu auto kontynuuje manewr.

## 7. Test sensorow

W `Scene View` powinno byc widac promienie:

- zielone, gdy nie ma przeszkody,
- czerwone, gdy sensor trafia przeszkode.

Sprawdz:

- sensor przedni,
- sensor tylny,
- trzy sensory po prawej,
- trzy sensory po lewej.

## 8. Test HUD

HUD powinien pokazywac:

```text
FSM: ...
Speed: ...
Steering: ...
Front: ...
Rear: ...
Right F/M/R: ...
Gap tracking: ...
Candidate: ...
```

Jesli HUD jest pusty:

- sprawdz, czy `DebugHud` ma przypisane referencje,
- wygeneruj sceny jeszcze raz,
- sprawdz, czy auto ma komponenty `ParkingStateMachine`, `ParkingCarController`, `ParkingSensors`.

## 9. Typowe problemy i naprawy

### Auto dalej zachowuje sie jak w starej wersji

Przyczyna:

- scena nie zostala przebudowana.

Naprawa:

```text
Tools -> Parking Project -> Build Demo Scenes
```

### Nie ma menu Tools

Mozliwe przyczyny:

- plik `ParkingDemoBuilder.cs` nie jest w `Assets/Editor`,
- Unity jeszcze kompiluje skrypty,
- w konsoli sa bledy kompilacji.

### Przyciski UI nie dzialaja

Projekt ma zapasowe UI `OnGUI`. W lewym gornym rogu powinny byc przyciski `Menu`, `Restart`, `Map 1`, `Map 2`, `Map 3`.

### Auto stoi na linii

Trzeba przesunac finalny punkt scenariusza. Dla miejsca ograniczonego liniami:

```text
finalZ = (lineA + lineB) / 2
```

### Auto zahacza inne auto

Trzeba:

- odsunac punkty posrednie od przeszkody,
- dodac wiecej malych krokow,
- zmniejszyc zmiane kata miedzy punktami,
- sprawdzic finalny punkt.

## 10. Kryteria zaliczenia

Projekt spelnia zalozenia, jezeli:

- ma 3 sceny,
- auto porusza sie automatycznie,
- jest FSM,
- sa czujniki Raycast,
- widac reakcje na przeszkode dynamiczna,
- jest UI do zmiany map,
- jest dokumentacja,
- da sie zbudowac `.exe`,
- logika nie korzysta z ML-Agents.

## 11. Lista kontrolna przed oddaniem

Przed oddaniem sprawdz:

- [ ] Wpisane imie i nazwisko w README.
- [ ] Wpisany numer albumu.
- [ ] Sceny wygenerowane przez `Build Demo Scenes`.
- [ ] Mapa 1 dziala.
- [ ] Mapa 2 dziala.
- [ ] Mapa 3 dziala.
- [ ] Zrobiony build Windows.
- [ ] Nagrany film 2-3 min.
- [ ] W filmie widac HUD.
- [ ] W filmie widac trzy mapy.

