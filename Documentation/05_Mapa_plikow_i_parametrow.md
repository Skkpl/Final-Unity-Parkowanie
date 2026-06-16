# Mapa plikow i parametrow

Ten dokument pomaga szybko znalezc, gdzie w kodzie zmienia sie konkretne rzeczy.

## 1. Gdzie zmienic predkosc auta

Plik:

```text
Assets/Scripts/Parking/ParkingCarController.cs
```

Pola:

```csharp
public float maxForwardSpeed = 4.0f;
public float maxReverseSpeed = 2.8f;
public float acceleration = 4.0f;
public float brakeAcceleration = 9.0f;
```

W scenach generator ustawia:

```csharp
controller.maxForwardSpeed = 3.8f;
controller.maxReverseSpeed = 2.5f;
```

## 2. Gdzie zmienic skret auta

Plik:

```text
ParkingCarController.cs
```

Pola:

```csharp
public float maxSteeringAngle = 35.0f;
public float steeringRate = 90.0f;
```

`maxSteeringAngle` to maksymalny kat skretu. `steeringRate` to szybkosc zmiany skretu.

## 3. Gdzie zmienic sensory

Plik:

```text
ParkingSensors.cs
```

Pola:

```csharp
public float rayHeight = 0.55f;
public float forwardRange = 5.0f;
public float sideRange = 7.0f;
public float parallelRequiredLength = 6.3f;
public float perpendicularRequiredWidth = 2.8f;
public float freeSideThreshold = 4.2f;
```

Najwazniejsze:

- `forwardRange` - zasieg przod/tyl,
- `sideRange` - zasieg boczny,
- `freeSideThreshold` - odleglosc uznawana za wolna przestrzen,
- `parallelRequiredLength` - minimalna dlugosc luki rownoleglej,
- `perpendicularRequiredWidth` - minimalna szerokosc luki prostopadlej.

## 4. Gdzie zmienic stany FSM

Plik:

```text
ParkingTypes.cs
```

Enum:

```csharp
public enum ParkingState
```

Logika przejsc:

```text
ParkingStateMachine.cs
```

## 5. Gdzie zmienic tor parkowania mapy 1

Plik:

```text
Assets/Editor/ParkingDemoBuilder.cs
```

Metoda:

```csharp
ConfigurePerpendicularScenario(GameObject car)
```

Lista:

```csharp
scenario.steps = new[]
{
    Step(...),
    Step(...),
}
```

Kazdy `Step` ma:

```text
stan, punkt, cofanie/tak-nie, predkosc, tolerancja, uchwyt
```

## 6. Gdzie zmienic tor parkowania mapy 2

Plik:

```text
ParkingDemoBuilder.cs
```

Metoda:

```csharp
ConfigureParallelScenario(GameObject car)
```

## 7. Gdzie zmienic tor parkowania mapy 3

Plik:

```text
ParkingDemoBuilder.cs
```

Metoda:

```csharp
ConfigureDynamicLeftScenario(GameObject car)
```

Finalny punkt aktualnie:

```csharp
CreateWaypoint("Map3_FinalLeftSpot", new Vector3(-5.62f, 0.0f, -8.4f), 90.0f)
```

Znaczenie:

- `x = -5.62` - glebokosc w miejscu po lewej stronie,
- `z = -8.4` - srodek miejsca miedzy liniami,
- `yaw = 90` - auto stoi prostopadle do drogi.

## 8. Gdzie zmienic czerwone auto w mapie 3

Plik:

```text
ParkingDemoBuilder.cs
```

Metoda:

```csharp
CreateOncomingTrafficCar(ParkingStateMachine observedCar)
```

Punkty:

```csharp
pointA.transform.position = new Vector3(-1.35f, 0.0f, 6.5f);
pointB.transform.position = new Vector3(-1.35f, 0.0f, -27.5f);
```

Predkosc:

```csharp
script.speed = 2.8f;
```

## 9. Gdzie zmienic wyglad auta

Plik:

```text
ParkingDemoBuilder.cs
```

Metoda:

```csharp
AddVehicleVisual(GameObject root, Material bodyMaterial, bool isEgo)
```

Tam sa:

- `Body`,
- `Cabin`,
- `FrontLights`,
- kola.

## 10. Gdzie zmienic kolory

Plik:

```text
ParkingDemoBuilder.cs
```

Metoda:

```csharp
CreateMaterials()
```

Przyklady:

```csharp
egoCar = CreateMaterial("EgoCarBlue", new Color(0.10f, 0.45f, 0.95f));
obstacle = CreateMaterial("DynamicObstacleRed", new Color(0.62f, 0.08f, 0.08f));
```

## 11. Gdzie zmienic kamere

Plik:

```text
CameraFollow.cs
```

Pola:

```csharp
public Vector3 offset = new Vector3(-7.5f, 8.5f, -9.0f);
public float followSharpness = 6.0f;
```

## 12. Gdzie zmienic UI

Pliki:

```text
MapManager.cs
ImmediateMapUi.cs
ParkingDemoBuilder.cs
```

`MapManager` laduje sceny. `ImmediateMapUi` rysuje awaryjne przyciski. `ParkingDemoBuilder` tworzy Canvas.

## 13. Gdzie zmienic HUD

Plik:

```text
DebugHud.cs
```

Tekst HUD powstaje w:

```csharp
label.text = ...
```

Mozna tam dodac np. aktualny indeks punktu, ale teraz jest prywatny w `ParkingStateMachine`.

## 14. Najwazniejsze parametry mapy 3

Zaparkowane auta po lewej:

```text
z = -23.0
z = -20.1
z = -17.2
z = -13.3
z = -5.0
z = -1.6
z = 1.8
```

Miejsce docelowe:

```text
miedzy liniami z = -10.1 i z = -6.7
srodek z = -8.4
```

Finalny punkt auta:

```text
x = -5.62
z = -8.4
yaw = 90
```

## 15. Szybka sciaga: co zmienic gdy cos wyglada zle

| Problem | Co zmienic |
| --- | --- |
| Auto stoi na linii | finalne `z` w ostatnim punkcie scenariusza |
| Auto zahacza inne auto | punkty posrednie przed finalnym punktem |
| Auto obraca sie za mocno | zmniejszyc roznice katow miedzy punktami |
| Auto jedzie za szybko | `speed` w `Step(...)` |
| Czerwone auto za pozno wyjezdza | `triggerDelay` albo `speed` w `CreateOncomingTrafficCar` |
| HUD zaslania ekran | pozycje `CreateText` w `CreateHud` |

