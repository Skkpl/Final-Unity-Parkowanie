# Opis do obrony projektu

Ten dokument jest krotka sciaga do rozmowy z prowadzacym.

## Metryczka zespolu

| Osoba | Nr albumu | Udzial | Skrot zadan |
| --- | ---: | ---: | --- |
| Szymon Karamon | 91859 | 40% | Integracja Unity, sceny, repozytorium GitHub, testy koncowe. |
| Bartosz Stolarczyk | 91742 | 30% | FSM, sensory, walidacja miejsc, HUD debug. |
| Antoni Krakowiak | 88437 | 30% | Uklady map, trajektorie parkowania, przeszkoda dynamiczna, wyglad pojazdow. |

## 1. Jednozdaniowy opis projektu

Projekt przedstawia deterministyczna symulacje automatycznego parkowania w Unity, w ktorej auto wykrywa miejsce parkingowe sensorami Raycast, przechodzi przez maszyne stanow FSM i wykonuje zaprogramowany manewr parkowania.

## 2. Co jest najwazniejsze technicznie

Najwazniejsze elementy:

- Unity 3D jako srodowisko symulacyjne,
- C# i `MonoBehaviour`,
- sensory `Physics.Raycast`,
- walidacja miejsca przez `Physics.CheckBox`,
- FSM jako glowny algorytm decyzyjny,
- deterministyczny tor parkowania po punktach,
- przeszkoda dynamiczna wymuszajaca `EmergencyStop`,
- HUD debug pokazujacy stan algorytmu.

## 3. Dlaczego projekt nie uzywa ML

Mozna powiedziec:

"Nie zastosowalem ML-Agents, poniewaz celem projektu bylo pokazanie algorytmu automatycznego parkowania, a nie procesu uczenia modelu. Rozwiazanie deterministyczne jest latwiejsze do wytlumaczenia, powtarzalne i stabilne podczas prezentacji. W projekcie widac sensory, walidacje miejsca, automat stanow i reakcje na przeszkode."

## 4. Jak dziala auto

Auto ma dwa poziomy sterowania:

1. Podczas dojazdu jedzie kinematycznie.
2. Po znalezieniu miejsca wykonuje manewr po punktach trajektorii.

Powod:

- pelna fizyka auta wymagalaby strojenia zawieszenia, tarcia i masy,
- w projekcie zaliczeniowym wazniejszy jest algorytm i powtarzalny pokaz.

## 5. Jak dzialaja sensory

Sensory sa promieniami:

- jeden z przodu,
- jeden z tylu,
- trzy z prawej,
- trzy z lewej.

Promienie zwracaja odleglosc do najblizszej przeszkody. Jezeli z boku jest wolna przestrzen przez odpowiedni dystans, program uznaje, ze moze tam byc luka parkingowa.

Dodatkowo `CheckBox` sprawdza, czy w obszarze miejsca nie ma przeszkody.

## 6. Jak dziala FSM

FSM przechodzi przez stany:

```text
Scan -> ValidateSpot -> Positioning -> ReverseTurn -> CounterTurn -> Straighten -> Parked
```

Znaczenie:

- `Scan` - szukanie miejsca,
- `ValidateSpot` - potwierdzenie miejsca,
- `Positioning` - ustawienie auta,
- `ReverseTurn` - pierwszy skret,
- `CounterTurn` - kontra,
- `Straighten` - wyprostowanie,
- `Parked` - koniec.

Jest tez `EmergencyStop`, gdy pojawia sie przeszkoda.

## 7. Co pokazac na mapie 1

Powiedz:

"Mapa 1 pokazuje parkowanie prostopadle. Wiekszosc miejsc jest zajeta, jest tez za waska luka. Auto finalnie parkuje w poprawnym miejscu."

Pokaz:

- HUD,
- przejscia stanow,
- finalne zaparkowanie.

## 8. Co pokazac na mapie 2

Powiedz:

"Mapa 2 pokazuje parkowanie rownolegle. Auto wykonuje cofanie, kontre i prostowanie."

Pokaz:

- cofanie,
- ustawienie rownolegle,
- stan `Parked`.

## 9. Co pokazac na mapie 3

Powiedz:

"Mapa 3 pokazuje reakcje na ruchoma przeszkode. Czerwone auto jedzie z naprzeciwka, dlatego niebieskie auto zatrzymuje sie w `EmergencyStop`. Po przejechaniu przeszkody kontynuuje i parkuje po lewej stronie."

Pokaz:

- czerwone auto,
- zatrzymanie niebieskiego,
- kontynuacje manewru,
- finalne parkowanie po lewej.

## 10. Pytania i odpowiedzi

### Czy to jest prawdziwa fizyka auta?

Nie w pelnym sensie. To uproszczony model kinematyczny i manewr po punktach. Pelna fizyka auta w Unity wymagalaby dokladnego strojenia `WheelCollider`, masy, tarcia i zawieszenia.

### Czy auto samo znajduje miejsce?

W projekcie sa sensory i struktura kandydata miejsca. W scenach demonstracyjnych miejsce jest dodatkowo opisane w `ParkingScenario`, zeby pokaz byl powtarzalny.

### Czy mozna dodac inne mapy?

Tak. Trzeba dodac nowa scene albo rozbudowac `ParkingDemoBuilder`, ustawic auta, linie i nowe punkty `ParkingScenarioStep`.

### Co jest najwiekszym uproszczeniem?

Najwieksze uproszczenie to finalny manewr parkowania po zaprogramowanych punktach, zamiast dynamicznego wyznaczania trajektorii w czasie rzeczywistym.

### Co jest najwazniejszym elementem algorytmu?

Maszyna stanow FSM i sensory. FSM decyduje, co auto robi w danym momencie, a sensory dostarczaja informacji o przeszkodach i miejscu.

## 11. Co powiedziec o wlasnym wkladzie

Mozna powiedziec:

"Projekt zostal zrobiony jako indywidualna implementacja w Unity. Logika sterowania, FSM, sensory, scenariusze, UI i dokumentacja zostaly przygotowane od zera. Nie korzystam z ML-Agents ani gotowego modelu uczonego."

## 12. Krotki opis do sprawozdania

Projekt przedstawia symulacje automatycznego parkowania samochodu w Unity 3D. System opiera sie na maszynie stanow FSM oraz czujnikach wirtualnych wykonanych za pomoca `Physics.Raycast`. Czujniki mierza odleglosci od przeszkod z przodu, z tylu oraz po bokach pojazdu. Po wykryciu poprawnego miejsca parkingowego auto przechodzi przez kolejne stany manewru: ustawienie, skret, kontre, prostowanie i zatrzymanie. W scenie dynamicznej czerwony pojazd nadjezdzajacy z naprzeciwka wymusza zatrzymanie awaryjne. Dla stabilnosci demonstracji finalny manewr parkowania jest realizowany po wyznaczonych punktach trajektorii, co zapewnia powtarzalny wynik i ulatwia prezentacje algorytmu.
