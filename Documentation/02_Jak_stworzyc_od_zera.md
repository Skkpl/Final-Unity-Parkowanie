# Jak stworzyc taki projekt od zera

Ten dokument opisuje proces stworzenia podobnego projektu w Unity krok po kroku. Nie jest to tylko instrukcja uruchomienia gotowej paczki, ale opis jak samodzielnie zbudowac podobna symulacje.

## 1. Przygotowanie projektu Unity

1. Otworz Unity Hub.
2. Utworz nowy projekt typu `Universal 3D`.
3. Nazwij projekt np. `Automatic parking`.
4. Po otwarciu Unity utworz foldery:

```text
Assets/Scripts/Parking
Assets/Editor
Assets/Scenes
Assets/Prefabs
Assets/Materials/ParkingDemo
Assets/ImportedVisuals
```

Minimalnie potrzebne sa:

- `Scripts/Parking` na skrypty runtime,
- `Editor` na generator scen,
- `Scenes` na sceny,
- `Materials` na materialy.

## 2. Podstawowe obiekty w Unity

Projekt mozna zrobic bez gotowych assetow, tylko z prymitywow Unity:

- `Cube` jako droga,
- `Cube` jako samochod,
- `Cube` jako kabina,
- `Cylinder` jako kola,
- `Cube` jako linie parkingowe,
- `Cube` jako przeszkody i sciany.

To wystarczy do zaliczenia, bo najwazniejsza jest logika.

## 3. Stworzenie typow danych

Pierwszy plik to `ParkingTypes.cs`.

Nalezy zdefiniowac:

- enum stanow FSM,
- enum typu parkowania,
- strukture odczytow sensorow,
- strukture kandydata miejsca.

Przyklad stanow:

```text
Scan -> ValidateSpot -> Positioning -> ReverseTurn -> CounterTurn -> Straighten -> Parked
```

Dodatkowy stan:

```text
EmergencyStop
```

Ta separacja jest wazna, bo latwiej potem tlumaczyc kod.

## 4. Stworzenie kontrolera auta

Plik: `ParkingCarController.cs`

Kontroler powinien miec pola:

- `wheelBase`,
- `carLength`,
- `carWidth`,
- `maxForwardSpeed`,
- `maxReverseSpeed`,
- `acceleration`,
- `brakeAcceleration`,
- `maxSteeringAngle`,
- `steeringRate`.

Minimalne metody:

```csharp
SetControls(float throttle, float steering, float brake)
StopImmediately()
MoveToPose(...)
SetPoseAlongPath(...)
```

### Tryb pierwszy: kinematyczny

W prostym trybie auto jedzie tak:

1. Oblicz predkosc docelowa z gazu.
2. Plynnie zmieniaj aktualna predkosc.
3. Plynnie zmieniaj kat skretu.
4. Oblicz predkosc obrotowa:

```csharp
yawRate = speed / wheelBase * tan(steeringAngle)
```

5. Przesun auto do przodu:

```csharp
transform.position += transform.forward * speed * dt;
```

### Tryb drugi: ruch po punktach

Do prezentacji najlepiej uzyc ruchu po punktach:

1. Zapamietaj pozycje startowa odcinka.
2. Zapamietaj kat startowy.
3. Oblicz roznice kata przez `Mathf.DeltaAngle`.
4. Przesuwaj auto przez `Vector3.Lerp`.
5. Obracaj auto przez lagodne przejscie `SmoothStep`.

To daje stabilny pokaz bez losowej fizyki.

## 5. Stworzenie sensorow

Plik: `ParkingSensors.cs`

Sensory powinny mierzyc:

- przod,
- tyl,
- bok prawy: przod/srodek/tyl,
- bok lewy: przod/srodek/tyl.

W Unity robi sie to przez:

```csharp
Physics.Raycast(origin, direction, out hit, range)
```

Warto rysowac promienie:

```csharp
Debug.DrawRay(origin, direction * distance, color)
```

Kolory:

- zielony: wolne,
- czerwony: przeszkoda.

## 6. Wykrywanie miejsca

Prosta logika:

1. Jesli trzy sensory boczne pokazuja duza odleglosc, zaczyna sie luka.
2. Zapamietaj pozycje poczatku luki.
3. Gdy auto jedzie dalej, aktualizuj koniec luki.
4. Jesli luka jest dluzsza niz wymagane minimum, sprawdz objetosc `CheckBox`.
5. Jesli `CheckBox` nie wykrywa przeszkody, miejsce jest poprawne.

Dla parkowania rownoleglego wymagaj dluzszej luki, np.:

```text
dlugosc auta * 1.4
```

Dla prostopadlego:

```text
szerokosc auta + margines
```

## 7. Stworzenie FSM

Plik: `ParkingStateMachine.cs`

Najprostszy wzorzec:

```csharp
switch (CurrentState)
{
    case Scan:
        ...
        break;
    case ValidateSpot:
        ...
        break;
}
```

W kazdym stanie:

- wykonaj logike,
- sprawdz warunek przejscia,
- jesli warunek spelniony, wywolaj `ChangeState(next)`.

Metoda `ChangeState` powinna:

- ustawic nowy stan,
- wyzerowac licznik czasu stanu,
- wykonac akcje specjalne, np. zatrzymanie w `Parked`.

## 8. Dodanie EmergencyStop

Stan awaryjny powinien dzialac z kazdego etapu manewru.

Przyklad:

```text
Jesli czujnik z przodu lub tylu wykryje przeszkode zbyt blisko:
    zapamietaj aktualny stan
    przejdz do EmergencyStop
    zatrzymaj auto
Gdy droga wolna:
    wroc do przerwanego stanu
```

W projekcie przeszkoda dynamiczna moze tez wywolac:

```csharp
RequestEmergencyStop(seconds)
```

To pozwala pokazac reakcje nawet wtedy, gdy fizyczne kolizje sa uproszczone.

## 9. Stworzenie scenariusza manewru

Plik: `ParkingScenario.cs`

Najprostszy sposob:

1. Utworz liste punktow docelowych.
2. Kazdy punkt ma:
   - pozycje,
   - kat,
   - predkosc,
   - informacje czy jedziemy do przodu czy cofamy,
   - stan FSM, ktory ma byc wyswietlany.
3. FSM wykonuje punkty po kolei.

Przyklad:

```text
Punkt 1: ustawienie przy miejscu
Punkt 2: pierwszy skret
Punkt 3: kontra
Punkt 4: prostowanie
Punkt 5: koniec
```

Im wiecej malych punktow, tym plynniej wyglada ruch.

## 10. Jak stroic punkty parkowania

Najpraktyczniejsza metoda:

1. Ustaw auto przed miejscem.
2. Dodaj punkt pozycjonowania.
3. Dodaj punkt wejscia w miejsce.
4. Dodaj punkt polowy.
5. Dodaj punkt koncowy.
6. Uruchom Play.
7. Jesli auto zahacza:
   - odsun punkty od przeszkody,
   - zmniejsz skok kata miedzy punktami,
   - dodaj punkt posredni.
8. Jesli auto stoi na linii:
   - przesun finalny punkt do srodka miejsca.

Dla miejsca miedzy liniami `zA` i `zB` srodek to:

```text
zCenter = (zA + zB) / 2
```

Przyklad z mapy 3:

```text
linie: -10.1 i -6.7
srodek: -8.4
```

## 11. Generator scen

Zamiast recznie klikac wszystko w Unity, mozna napisac skrypt edytorowy:

```csharp
[MenuItem("Tools/Parking Project/Build Demo Scenes")]
public static void BuildDemoScenes()
```

Taki generator:

- tworzy materialy,
- tworzy auta,
- tworzy drogi,
- tworzy linie,
- ustawia UI,
- ustawia kamere,
- zapisuje sceny.

Zaleta: jesli cos sie zepsuje, mozna jednym kliknieciem odbudowac caly projekt.

## 12. Stworzenie modelu auta

Minimalny model:

1. `Body` - cube, skala np. `2.0 x 0.72 x 4.45`.
2. `Cabin` - mniejszy cube na gorze.
3. `FrontMarker` - maly pasek z przodu.
4. Cztery kola - cylindry.

Do auta dodaj:

- `Rigidbody` jako kinematic,
- `ParkingCarController`,
- `ParkingSensors`,
- `ParkingStateMachine`,
- `ParkingManeuverPlanner`,
- `VehicleVisualRig`.

## 13. Stworzenie mapy 1

Mapa 1:

- droga na srodku,
- parking po prawej,
- wiekszosc miejsc zajeta,
- jedna luka za waska,
- jedna luka docelowa.

Cel: pokazac, ze auto nie parkuje w pierwszym lepszym miejscu.

## 14. Stworzenie mapy 2

Mapa 2:

- ulica,
- auta zaparkowane rownolegle,
- miejsce docelowe,
- manewr cofania.

Cel: pokazac parkowanie rownolegle.

## 15. Stworzenie mapy 3

Mapa 3:

- parking po lewej,
- czerwone auto z naprzeciwka,
- niebieskie auto czeka,
- po przejechaniu czerwonego auta parkuje.

Cel: pokazac stan `EmergencyStop`.

## 16. Dodanie HUD

HUD powinien pokazywac:

- aktualny stan,
- predkosc,
- kat skretu,
- odczyty sensorow,
- czy jest kandydat miejsca.

To bardzo pomaga podczas obrony.

## 17. Testowanie

Testuj w tej kolejnosci:

1. Czy sceny sie generuja.
2. Czy menu laduje mapy.
3. Czy auto rusza.
4. Czy HUD pokazuje zmiane stanow.
5. Czy auto konczy w `Parked`.
6. Czy mapa 3 przechodzi przez `EmergencyStop`.
7. Czy finalne pozycje aut nie zahaczaja innych aut.

## 18. Build

Po wygenerowaniu scen:

1. Otworz `File -> Build Profiles` albo `File -> Build Settings`.
2. Sprawdz, czy sceny sa w liscie:
   - `MainMenu`,
   - `Map_01_Perpendicular`,
   - `Map_02_Parallel`,
   - `Map_03_Dynamic`.
3. Wybierz Windows.
4. Zbuduj `.exe`.

## 19. Co nagrac na filmie

Film 2-3 min:

1. Menu.
2. Mapa 1: auto parkuje prostopadle.
3. Mapa 2: auto parkuje rownolegle.
4. Mapa 3: czerwone auto wymusza zatrzymanie, potem niebieskie parkuje po lewej.
5. Krotko pokaz `Scene View` z promieniami sensorow.

## 20. Jak tlumaczyc uproszczenia

Najlepsze wyjasnienie:

"Projekt skupia sie na algorytmie sterowania, a nie na zaawansowanej fizyce pojazdu. Dlatego zastosowano uproszczony model kinematyczny i deterministyczny tor parkowania po punktach. Dzieki temu wynik jest powtarzalny, a w projekcie widac sensory, FSM i reakcje na przeszkody."

