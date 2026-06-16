# Contributors / Metryczka zespolu

Projekt: Automatyczne parkowanie w Unity 3D  
Repozytorium: https://github.com/Skkpl/Final-Unity-Parkowanie.git  
Data: 2026-06-16

## Sklad zespolu

| Osoba | Nr albumu | Udzial procentowy | Glowne zadania |
| --- | ---: | ---: | --- |
| Szymon Karamon | 91859 | 40% | Integracja projektu w Unity, generator scen, konfiguracja repozytorium GitHub, testy koncowe, dokumentacja oddania. |
| Bartosz Stolarczyk | 91742 | 30% | Logika automatu stanow FSM, sensory Raycast/CheckBox, walidacja miejsc parkingowych, HUD debug. |
| Antoni Krakowiak | 88437 | 30% | Uklad map testowych, trajektorie manewrow, scenariusz przeszkody dynamicznej, poprawki wizualne pojazdow. |

Suma udzialow: 100%.

## Podzial prac wedlug modulow

| Modul / element | Odpowiedzialna osoba | Opis |
| --- | --- | --- |
| `ParkingDemoBuilder.cs` | Szymon Karamon | Automatyczne budowanie scen, prefabow, materialow, UI oraz konfiguracji kamer. |
| `ParkingStateMachine.cs` | Bartosz Stolarczyk | Glowne przejscia stanow: skanowanie, walidacja miejsca, parkowanie i zatrzymanie awaryjne. |
| `ParkingSensors.cs` | Bartosz Stolarczyk | Czujniki Raycast, pomiar odleglosci, sprawdzenie wolnej przestrzeni przez `Physics.CheckBox`. |
| `ParkingCarController.cs` | Szymon Karamon | Uproszczona kinematyka pojazdu, ruch po punktach, ograniczenie obrotu i predkosci. |
| `ParkingScenario.cs` | Antoni Krakowiak | Dane map: punkty skanowania, kandydaci miejsc i trajektorie parkowania. |
| `MovingObstacle.cs` | Antoni Krakowiak | Czerwone auto w scenie dynamicznej i wymuszenie zatrzymania niebieskiego pojazdu. |
| Dokumentacja i testy | Caly zespol | README, opis techniczny, instrukcja uruchomienia, test plan i opis ograniczen. |

## Oswiadczenie

Powyzszy podzial opisuje deklarowany zakres wykonanych prac w ramach projektu. Projekt wykorzystuje wlasna logike parkowania oparta o FSM, sensory Raycast/CheckBox i trajektorie demonstracyjne. W projekcie nie zastosowano ML-Agents ani zewnetrznego modelu uczenia maszynowego.
