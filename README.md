# System zgłaszania i przeglądania zdarzeń drogowych

Aplikacja backendowa (REST API) w technologii **ASP.NET Core 8 Web API**, zbudowana
w **architekturze warstwowej** (Controllers → Services → Data). Umożliwia zgłaszanie
i przeglądanie zdarzeń drogowych: wypadków, robót drogowych, przeszkód i awarii.

---

## 1. Domena

| Element | Opis |
|---|---|
| **Encja główna** | `IncidentReport` — zgłoszenie zdarzenia drogowego (tytuł, opis, typ, status, lokalizacja, współrzędne, data wystąpienia). |
| **Użytkownik** | `Reporter` — osoba zgłaszająca (rola Owner/Client). |
| **Relacja logiczna** | 1:N — jeden zgłaszający może mieć wiele zgłoszeń (`Reporter` 1 — N `IncidentReport`). |

### Reguły biznesowe (zaimplementowane w warstwie serwisów)

1. **Data wystąpienia nie z przyszłości** — nie można utworzyć zgłoszenia, którego
   `OccurredAt` jest późniejsze niż bieżąca chwila (z 5-minutową tolerancją na różnice zegarów).
2. **Walidacja przejść statusu** — status może zmieniać się wyłącznie zgodnie z dozwolonym
   przepływem pracy:
   `Nowe → Zweryfikowane → WTrakcie → Zakonczone`, z możliwością `Odrzucone`
   na wcześniejszych etapach. Statusy `Zakonczone` i `Odrzucone` są końcowe — nie można ich cofnąć.
3. *(dodatkowa)* **Brak duplikatów** — nie można utworzyć drugiego aktywnego zgłoszenia
   tego samego typu w promieniu ok. 100 m od istniejącego (wzór haversine).

---

## 2. Architektura

```
RoadIncidentApi/
├── Controllers/      # cienkie kontrolery (tylko odbiór żądań)
├── Services/         # logika biznesowa + interfejsy (Service Pattern + DI)
├── Models/           # encje domenowe i enumy
├── DTOs/             # obiekty transferu danych (separacja od encji)
├── Data/             # AppDbContext (EF Core) + seeder danych
├── Mapping/          # ręczne mapowanie encja <-> DTO
├── Exceptions/       # DomainException, NotFoundException
├── Middleware/       # globalna obsługa błędów -> JSON
└── Migrations/       # migracje EF Core
```

**Wykorzystane mechanizmy:** Dependency Injection (`AddScoped`), Service Pattern
(interfejsy `IReportService`/`IReporterService`/`ICsvExportService`), własne wyjątki
domenowe przechwytywane przez middleware, walidacja atrybutami `[Required]`,
`[StringLength]`, `[Range]` w klasach DTO.

**Funkcjonalności dodatkowe:** paginacja + filtrowanie listy zgłoszeń oraz eksport do CSV
(`CsvHelper`).

---

## 3. Wymagania

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- (opcjonalnie) Visual Studio 2022 lub VS Code z rozszerzeniem **C# Dev Kit**

---

## 4. Uruchomienie (2 minuty)

```bash
# 1. Wejdź do katalogu projektu
cd RoadIncidentApi

# 2. Pobierz zależności
dotnet restore

# 3. Uruchom aplikację (baza SQLite i tabele utworzą się automatycznie)
dotnet run
```

Aplikacja przy starcie automatycznie wykonuje migracje (`Database.Migrate()`) i wypełnia
bazę przykładowymi danymi. Po uruchomieniu otwórz:

```
http://localhost:5080/swagger
```

> Port może się różnić — sprawdź komunikat w konsoli lub `Properties/launchSettings.json`.

---

## 5. Baza danych (EF Core + SQLite)

Connection string znajduje się w `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=roadincidents.db"
}
```

Plik bazy `roadincidents.db` tworzy się w katalogu projektu przy pierwszym uruchomieniu.

### Praca z migracjami (opcjonalnie)

Wymaga narzędzia `dotnet-ef` (`dotnet tool install --global dotnet-ef`):

```bash
# dodanie nowej migracji
dotnet ef migrations add NazwaMigracji

# ręczne zastosowanie migracji do bazy
dotnet ef database update
```

W projekcie znajduje się gotowa migracja `InitialCreate`.

---

## 6. Najważniejsze endpointy

| Metoda | Ścieżka | Opis |
|---|---|---|
| GET | `/api/reports` | Lista zgłoszeń (paginacja + filtry `status`, `type`, `city`) |
| GET | `/api/reports/{id}` | Szczegóły zgłoszenia |
| POST | `/api/reports` | Utworzenie zgłoszenia |
| PATCH | `/api/reports/{id}/status` | Zmiana statusu (z walidacją przejścia) |
| DELETE | `/api/reports/{id}` | Usunięcie zgłoszenia |
| GET | `/api/reports/export` | Eksport wszystkich zgłoszeń do CSV |
| GET | `/api/reporters` | Lista zgłaszających |
| POST | `/api/reporters` | Utworzenie zgłaszającego |
| GET | `/api/reporters/{id}` | Szczegóły zgłaszającego |
| DELETE | `/api/reporters/{id}` | Usunięcie zgłaszającego (kaskadowo) |

Przykładowe żądania znajdują się w pliku `RoadIncidentApi.http`.

### Przykład — utworzenie zgłoszenia

```http
POST /api/reports
Content-Type: application/json

{
  "title": "Kolizja na rondzie",
  "description": "Dwa pojazdy, brak rannych.",
  "type": "Wypadek",
  "city": "Gdynia",
  "street": "ul. Świętojańska",
  "latitude": 54.5189,
  "longitude": 18.5305,
  "occurredAt": "2026-06-17T07:45:00Z",
  "reporterId": 1
}
```

### Format błędu (przykład naruszenia reguły biznesowej)

```json
{
  "status": 400,
  "title": "Naruszenie reguły biznesowej",
  "detail": "Data wystąpienia zdarzenia nie może być z przyszłości."
}
```

---

## 7. Słowniki wartości (enumy)

**`type` (IncidentType):** `Wypadek`, `RobotyDrogowe`, `Przeszkoda`, `Awaria`, `Inne`

**`status` (ReportStatus):** `Nowe`, `Zweryfikowane`, `WTrakcie`, `Zakonczone`, `Odrzucone`

Enumy w JSON są przekazywane jako tekst (np. `"type": "Wypadek"`).
