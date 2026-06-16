# Dokumentacja techniczna projektu

Projekt: Automatyczne parkowanie w Unity 3D  
Wersja dokumentacji: 2026-06-15  
Charakter projektu: deterministyczna symulacja parkowania, bez machine learningu

## 1. Cel projektu

Celem projektu jest pokazanie prostego systemu automatycznego parkowania samochodu w Unity. Program ma:

- uruchamiac sie w Unity jako zwykla scena 3D,
- miec trzy scenariusze testowe,
- uzywac czujnikow opartych o `Raycast` i `CheckBox`,
- sterowac autem przez maszyne stanow FSM,
- reagowac na przeszkode dynamiczna,
- pokazac wynik w czytelny sposob na ekranie,
- byc mozliwy do wytlumaczenia bez znajomosci ML-Agents i bez trenowania modelu.

Projekt nie jest symulatorem profesjonalnej fizyki pojazdow. To wersja zaliczeniowa: stabilna, powtarzalna i prosta do omowienia. Najwazniejsze jest pokazanie algorytmu, podzialu na stany, sensorow i reakcji na sytuacje na parkingu.

## 2. Glowna idea dzialania

Auto nie uczy sie samo. Dziala wedlug deterministycznego scenariusza:

1. Auto jedzie powoli wzdluz drogi.
2. Sensory mierza odleglosci od przeszkod.
3. Program publikuje informacje o znalezionym miejscu parkingowym.
4. Maszyna stanow przechodzi przez kolejne etapy parkowania.
5. W mapach demonstracyjnych finalny manewr jest wykonywany po wyznaczonych punktach, jak po szynie.
6. Po dojechaniu do ostatniego punktu auto zatrzymuje sie w stanie `Parked`.

Finalna wersja celowo uzywa trybu "po szynach" dla manewru parkowania. Powod: w Unity prosta, niekonfigurowana fizyka pojazdu potrafi dawac niestabilne wyniki, a projekt ma dzialac przewidywalnie podczas prezentacji.

## 3. Struktura folderow

Po zainstalowaniu paczki w projekcie Unity wazne foldery sa takie:

```text
Assets/
  Editor/
    ParkingDemoBuilder.cs
  Scripts/
    Parking/
      CameraFollow.cs
      DebugHud.cs
      ImmediateMapUi.cs
      MapManager.cs
      MovingObstacle.cs
      ParkingCarController.cs
      ParkingManeuverPlanner.cs
      ParkingScenario.cs
      ParkingSensors.cs
      ParkingStateMachine.cs
      ParkingTypes.cs
      VehicleVisualRig.cs
  Scenes/
    MainMenu.unity
    Map_01_Perpendicular.unity
    Map_02_Parallel.unity
    Map_03_Dynamic.unity
  Prefabs/
  Materials/
```

Folder `Documentation` znajduje sie w paczce poza `Assets`, zeby nie mieszac dokumentow z logika Unity.

## 4. Najwazniejsze klasy

| Plik | Rola |
| --- | --- |
| `ParkingTypes.cs` | Definicje typow danych: stany FSM, typ parkowania, odczyty sensorow, kandydat miejsca. |
| `ParkingCarController.cs` | Sterowanie pojazdem: predkosc, skret, hamowanie, ruch kinematyczny i ruch po punktach. |
| `ParkingSensors.cs` | Czujniki Raycast, wykrywanie luk i walidacja wolnej przestrzeni. |
| `ParkingStateMachine.cs` | Glowna logika FSM: skanowanie, walidacja, manewr, stop awaryjny, zaparkowanie. |
| `ParkingScenario.cs` | Dane scenariusza: punkt akceptacji miejsca, lista punktow manewru, opis wykrytej luki. |
| `MovingObstacle.cs` | Ruch czerwonego auta i wymuszenie zatrzymania niebieskiego auta. |
| `ParkingDemoBuilder.cs` | Generator materialow, prefabu auta, UI i trzech scen testowych. |
| `VehicleVisualRig.cs` | Wizualny obrot kol i skret przednich kol. |
| `DebugHud.cs` | Tekst debug w lewym gornym rogu. |
| `MapManager.cs` | Ladowanie scen i restart. |
| `ImmediateMapUi.cs` | Zapasowe przyciski OnGUI do zmiany map. |
| `CameraFollow.cs` | Kamera sledzaca auto. |

## 5. Dane publiczne i struktury

### `ParkingState`

Enum opisuje aktualny stan automatu:

```csharp
public enum ParkingState
{
    Scan,
    ValidateSpot,
    Positioning,
    ReverseTurn,
    CounterTurn,
    Straighten,
    Parked,
    EmergencyStop
}
```

Znaczenie stanow:

- `Scan` - auto jedzie i szuka miejsca.
- `ValidateSpot` - program potwierdza znalezione miejsce.
- `Positioning` - auto ustawia sie do manewru.
- `ReverseTurn` - pierwszy skret przy wjezdzie w miejsce.
- `CounterTurn` - kontra, czyli drugi etap ustawiania auta.
- `Straighten` - prostowanie auta w miejscu.
- `Parked` - koniec.
- `EmergencyStop` - zatrzymanie awaryjne, np. przy przeszkodzie dynamicznej.

### `ParkingType`

```csharp
public enum ParkingType
{
    Perpendicular,
    Parallel
}
```

`Perpendicular` oznacza parkowanie prostopadle, `Parallel` oznacza parkowanie rownolegle.

### `SensorReadings`

Struktura przechowuje odleglosci z sensorow:

```csharp
public struct SensorReadings
{
    public float front;
    public float rear;
    public float rightFront;
    public float rightMiddle;
    public float rightRear;
    public float leftFront;
    public float leftMiddle;
    public float leftRear;
}
```

Przyklad: `rightFront` to odleglosc po prawej stronie z przodu pojazdu, `front` to odleglosc przed pojazdem.

### `ParkingSpotCandidate`

Opisuje kandydat na miejsce parkingowe:

```csharp
public struct ParkingSpotCandidate
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float length;
    public float width;
    public bool isValid;
    public ParkingType parkingType;
    public Vector3 Center => (startPosition + endPosition) * 0.5f;
}
```

W scenach demonstracyjnych kandydat jest czesciowo publikowany z `ParkingScenario`, zeby prezentacja byla powtarzalna.

## 6. Maszyna stanow FSM

FSM znajduje sie w `ParkingStateMachine.cs`. To najwazniejszy skrypt projektu.

Schemat:

```text
Scan
  -> ValidateSpot
  -> Positioning
  -> ReverseTurn
  -> CounterTurn
  -> Straighten
  -> Parked

Z Positioning / ReverseTurn / CounterTurn / Straighten mozliwe:
  -> EmergencyStop
  -> powrot do przerwanego stanu
```

### `Awake`

W `Awake` pobierane sa komponenty:

- `ParkingCarController`,
- `ParkingSensors`,
- `ParkingManeuverPlanner`,
- opcjonalnie `ParkingScenario`.

Jezeli obiekt ma `ParkingScenario`, to:

```csharp
car.UseExternalPoseControl = scenario != null;
```

Oznacza to, ze ruch auta bedzie sterowany przez punkty scenariusza, a nie przez swobodna fizyke.

### `FixedUpdate`

`FixedUpdate` dziala w stalym kroku fizycznym Unity. W projekcie jest to dobre miejsce na logike ruchu auta.

Najpierw aktualizowany jest czas stanu:

```csharp
StateElapsed += Time.fixedDeltaTime;
```

Potem sprawdzany jest stop awaryjny:

- `raycastEmergency` - przeszkoda z sensorow,
- `scriptedEmergency` - przeszkoda dynamiczna wymuszona przez `MovingObstacle`.

Jesli auto jest w trakcie manewru i pojawia sie przeszkoda, FSM przechodzi do `EmergencyStop`.

### `UpdateScenarioScan`

W scenach demonstracyjnych auto nie czeka na przypadkowy wynik sensorow, tylko dojezdza do punktu `scanAcceptPoint`.

Kiedy dojedzie:

1. budowany jest `ParkingSpotCandidate`,
2. kandydat jest wyswietlany w HUD,
3. FSM przechodzi do `ValidateSpot`.

### `UpdateScenarioValidation`

Auto zatrzymuje sie na krotka pauze:

```csharp
if (StateElapsed < scenario.validatePause)
{
    return;
}
```

Potem rozpoczyna pierwszy krok scenariusza.

### `UpdateScenarioManeuverState`

To metoda, ktora wykonuje punkty manewru:

1. Pobiera aktualny `ParkingScenarioStep`.
2. Jesli krok ma `smoothArc = true`, uzywa `UpdateSmoothScenarioStep`.
3. Jesli nie, uzywa prostego `MoveToPose`.
4. Po zakonczeniu kroku przechodzi do kolejnego.

Nazwa `smoothArc` zostala historycznie, ale w finalnej wersji ruch jest celowo zrobiony "po szynach": prosta interpolacja pozycji miedzy punktami i lagodna interpolacja kata.

### `UpdateSmoothScenarioStep`

Finalna wersja dziala tak:

```csharp
stepStartPosition = transform.position;
stepStartYaw = transform.eulerAngles.y;
stepYawDelta = Mathf.DeltaAngle(stepStartYaw, step.target.rotation.eulerAngles.y);
stepPathLength = Mathf.Max(0.1f, Vector3.Distance(stepStartPosition, step.target.position));
```

Na starcie kroku zapamietujemy:

- pozycje startowa,
- kat startowy,
- roznice kata do punktu docelowego,
- dlugosc odcinka.

Potem w kazdym kroku:

```csharp
stepProgress = Mathf.Clamp01(stepProgress + speed * Time.fixedDeltaTime / stepPathLength);
Vector3 position = Vector3.Lerp(stepStartPosition, step.target.position, stepProgress);
float easedProgress = Mathf.SmoothStep(0.0f, 1.0f, stepProgress);
float yaw = stepStartYaw + stepYawDelta * easedProgress;
```

To znaczy:

- `stepProgress` rosnie od 0 do 1,
- `Vector3.Lerp` przesuwa auto po prostej szynie,
- `Mathf.SmoothStep` lagodzi start i koniec obrotu,
- `Mathf.DeltaAngle` gwarantuje najkrotszy obrot, bez 360 stopni.

Krok konczy sie, gdy:

```csharp
return stepProgress >= 0.999f;
```

Nie ma juz dokrecania w miejscu. To wazne, bo wczesniejsze wersje potrafily stac i obracac samochod.

## 7. Sterowanie autem

Sterowanie znajduje sie w `ParkingCarController.cs`.

### Dwa tryby pracy

1. Tryb kinematyczny:
   - uzywany, gdy nie ma `ParkingScenario`,
   - auto reaguje na `SetControls(throttle, steering, brake)`,
   - obrot liczony jest uproszczonym modelem rowerowym.

2. Tryb po punktach:
   - uzywany w scenach demonstracyjnych,
   - `UseExternalPoseControl = true`,
   - FSM ustawia pozycje przez `SetPoseAlongPath`.

### `SetControls`

```csharp
public void SetControls(float throttle, float steering, float brake)
```

Parametry:

- `throttle` od `-1` do `1`, ujemny oznacza cofanie,
- `steering` od `-1` do `1`, kierunek skretu,
- `brake` od `0` do `1`.

### Kinematyka w `FixedUpdate`

Predkosc nie zmienia sie natychmiast. Uzywany jest `Mathf.MoveTowards`, czyli plynne dojscie do predkosci docelowej.

Skret tez nie zmienia sie od razu:

```csharp
CurrentSteeringAngle = Mathf.MoveTowards(
    CurrentSteeringAngle,
    desiredSteeringAngle,
    steeringRate * dt);
```

Obrot auta w trybie kinematycznym:

```csharp
float yawRate = CurrentSpeed / wheelBase * Mathf.Tan(steeringRadians);
```

To uproszczony model rowerowy. Zaklada, ze pojazd ma jeden wirtualny przod i jeden wirtualny tyl.

### `SetPoseAlongPath`

Ta metoda jest kluczowa dla finalnego trybu:

```csharp
public void SetPoseAlongPath(Vector3 position, Quaternion rotation, float signedSpeed, float steeringHint)
```

Ustawia:

- pozycje auta,
- rotacje tylko po osi Y,
- aktualna predkosc do HUD i kol,
- podpowiedz skretu dla wizualnych kol.

Funkcja `FlattenYaw` zeruje niepotrzebne obroty w osiach X i Z, zeby auto nie przechylalo sie ani nie obracalo przypadkowo.

## 8. Sensory

Sensory sa w `ParkingSensors.cs`.

### Odczyty Raycast

Metoda `GetReadings` wykonuje kilka promieni:

- przod,
- tyl,
- prawa strona: przod, srodek, tyl,
- lewa strona: przod, srodek, tyl.

Promienie sa rysowane w `Scene View`:

- czerwony oznacza trafienie w przeszkode,
- zielony oznacza brak przeszkody.

Najwazniejsza metoda pomocnicza:

```csharp
private float CastDistance(Vector3 origin, Vector3 direction, float range)
```

Zwraca odleglosc do przeszkody albo maksymalny zasieg.

### Wykrywanie luki

`TryFindSpot` sprawdza, czy po prawej stronie jest wolna przestrzen:

```csharp
bool rightSideLooksFree = readings.rightFront > freeSideThreshold
    && readings.rightMiddle > freeSideThreshold
    && readings.rightRear > freeSideThreshold;
```

Jesli wolna przestrzen trwa odpowiednio dlugo, powstaje kandydat na miejsce.

### Walidacja objetosci

Sama odleglosc z promieni nie wystarczy. Dlatego jest dodatkowy test:

```csharp
Physics.CheckBox(center, halfExtents, rotation, obstacleMask, QueryTriggerInteraction.Ignore);
```

`CheckBox` sprawdza, czy w przewidywanym miejscu parkingowym nie ma obiektu. To zabezpiecza przed falszywa luka.

## 9. Scenariusze parkowania

Scenariusz jest w `ParkingScenario.cs`.

Najwazniejsze pola:

- `parkingType` - prostopadle albo rownolegle,
- `scanAcceptPoint` - gdzie auto uznaje miejsce za znalezione,
- `scanSpeed` - predkosc podczas dojazdu,
- `validatePause` - pauza po znalezieniu miejsca,
- `spotStart`, `spotEnd`, `spotWidth` - opis miejsca do HUD,
- `steps` - lista punktow manewru.

Pojedynczy krok:

```csharp
public sealed class ParkingScenarioStep
{
    public ParkingState state;
    public Transform target;
    public bool reverse;
    public float speed;
    public float turnSpeed;
    public float arriveDistance;
    public float angleTolerance;
    public float timeout;
    public bool smoothArc;
    public float curveHandle;
}
```

W finalnej wersji najwazniejsze sa:

- `state`,
- `target`,
- `reverse`,
- `speed`,
- `timeout`.

`curveHandle` zostal jako pole kompatybilne z poprzednimi wersjami, ale aktualny ruch po szynach nie korzysta z krzywych Beziera.

## 10. Generator scen

`ParkingDemoBuilder.cs` jest skryptem edytorowym. Nie dziala w buildzie jako logika gry, tylko generuje zasoby w Unity.

Menu:

```text
Tools -> Parking Project -> Build Demo Scenes
```

Po kliknieciu wykonywane jest:

```csharp
BuildDemoScenes()
```

Ta metoda:

1. tworzy foldery,
2. tworzy materialy,
3. tworzy prefaby,
4. generuje menu glowne,
5. generuje mape 1,
6. generuje mape 2,
7. generuje mape 3,
8. ustawia sceny w Build Settings.

### Materialy

`CreateMaterials` tworzy materialy:

- asfalt,
- beton,
- linie parkingowe,
- niebieski samochod,
- szare samochody,
- czerwony samochod dynamiczny,
- sciany,
- opony,
- szyby.

### Model auta

`AddVehicleVisual` tworzy auto z prymitywow Unity:

- `Body` - glowna bryla auta,
- `Cabin` - kabina/szyby,
- `YellowFrontMarker` albo `FrontLights` - oznaczenie przodu,
- 4 walce jako kola.

To nie jest realistyczny model 3D, ale pozwala czytelnie pokazac przod, tyl i skret.

## 11. Mapa 1

Nazwa: `Map_01_Perpendicular`

Zalozenie:

- parking prostopadly,
- wiekszosc miejsc zajeta,
- jedna luka jest za waska,
- druga luka jest miejscem docelowym.

Auto startuje:

```csharp
new Vector3(0.0f, 0.0f, -27.0f)
```

Miejsca i auta sa po prawej stronie. Auto ma dojechac do punktu skanowania, zignorowac zla luke i zaparkowac w miejscu docelowym.

Tor manewru jest zapisany w `ConfigurePerpendicularScenario`.

## 12. Mapa 2

Nazwa: `Map_02_Parallel`

Zalozenie:

- waska ulica,
- parkowanie rownolegle,
- auto ma wykonac cofanie i kontre,
- finalnie ma stac rownolegle do drogi.

Tor jest zapisany w `ConfigureParallelScenario`.

## 13. Mapa 3

Nazwa: `Map_03_Dynamic`

Zalozenie:

- scena podobna do mapy 1,
- miejsca parkingowe sa po lewej stronie,
- czerwone auto jedzie z naprzeciwka,
- niebieskie auto musi zaczekac,
- po przejechaniu czerwonego auta niebieskie parkuje po lewej.

Najwazniejsze punkty:

```csharp
CreateOncomingTrafficCar(...)
ConfigureDynamicLeftScenario(...)
```

Finalne miejsce jest miedzy liniami:

- `z = -10.1`,
- `z = -6.7`.

Finalny punkt niebieskiego auta:

```csharp
new Vector3(-5.62f, 0.0f, -8.4f), yaw 90
```

To srodek miejsca parkingowego.

## 14. Przeszkoda dynamiczna

`MovingObstacle.cs` przesuwa czerwone auto miedzy `pointA` i `pointB`.

W mapie 3:

- czerwone auto startuje z przodu,
- rusza, gdy niebieskie jest w stanie `Scan`,
- wymusza `EmergencyStop` przez `RequestEmergencyStop`,
- po dojechaniu do konca moze sie zatrzymac.

Klucz:

```csharp
observedCar.RequestEmergencyStop(0.75f);
```

To nie jest fizyczne zderzenie. To celowe wymuszenie reakcji FSM.

## 15. HUD i debug

`DebugHud.cs` pokazuje:

- aktualny stan FSM,
- predkosc auta,
- kat skretu,
- odczyty sensorow,
- czy trwa sledzenie luki,
- ostatniego kandydata miejsca parkingowego.

To pomaga podczas prezentacji, bo widac, ze system faktycznie przechodzi przez stany.

## 16. Kamera

`CameraFollow.cs` sledzi auto:

```csharp
transform.position = Vector3.Lerp(...);
transform.LookAt(...);
```

Kamera nie jest elementem algorytmu, tylko ulatwia obserwacje.

## 17. UI

Projekt ma dwa typy UI:

1. Canvas z przyciskami tworzony przez `ParkingDemoBuilder`.
2. Awaryjne UI `OnGUI` z `ImmediateMapUi`.

`OnGUI` jest malo eleganckie, ale praktyczne: nawet jesli Canvas nie zadzialalby poprawnie, dalej mozna zmieniac mapy.

## 18. Dlaczego nie ML-Agents

Projekt nie uzywa ML-Agents, bo:

- uczenie modelu zajmuje duzo czasu,
- wynik uczenia nie jest deterministyczny,
- trudniej wyjasnic decyzje auta,
- na projekt zaliczeniowy wystarcza FSM + sensory + scenariusze.

To jest zaleta w dokumentacji: prowadzacy moze zapytac "dlaczego bez ML?", a odpowiedz brzmi: wymagany byl algorytm automatycznego parkowania, niekoniecznie uczony model; zastosowano deterministyczny automat stanow i sensory.

## 19. Ograniczenia projektu

Najwazniejsze ograniczenia:

- auto nie ma pelnego modelu zawieszenia,
- finalny manewr nie jest obliczany dynamicznie z geometrii miejsca, tylko zapisany punktami,
- trasy sa dopasowane do przygotowanych scen,
- kolizje nie sa glownym mechanizmem sterowania,
- projekt pokazuje zasade dzialania, a nie gotowy system do prawdziwego auta.

Te ograniczenia sa akceptowalne, jesli sa uczciwie opisane.

## 20. Co mowic przy prezentacji

Krotkie wytlumaczenie:

"Projekt pokazuje automatyczne parkowanie zrealizowane deterministycznie. Auto ma sensory Raycast, ktore mierza odleglosci od przeszkod, a logika jest oparta o maszyne stanow. Po znalezieniu miejsca auto wykonuje manewr wedlug zaprogramowanej trajektorii punktowej. W trzeciej scenie pojawia sie auto z naprzeciwka, ktore wymusza stan EmergencyStop. Rozwiazanie nie uzywa ML, bo celem byla powtarzalna demonstracja algorytmu i reakcji na przeszkody."
