# Elon: The Life Simulator

**Working title** · 2D pixel-art narrative adventure / life simulator following Elon Musk’s life with a respectful, inspiring, lightly humorous tone.

Unity 6 (6000.1.x) project with modular **Act 1 (South Africa)**, **company stats**, and **Zip2 / X.com** inbox problems.

---

## Open the project

1. Install **Unity Hub** + Editor **6000.1.3f1** (or compatible 6000.1.x).
2. Open folder `ElonLifeSimulator`.
3. Confirm **Build Settings** scenes:
   - `MainMenu`
   - `SouthAfrica_Pretoria`
   - `Canada_Toronto`
   - `SiliconValley_PaloAlto`
4. Press **Play** on **MainMenu**.

### Play path (current)

1. **New Game** → Pretoria. **Story** auto-starts Act 1 (or click **Story**).
2. Finish beats in order: **Home → School (staircase) → Library (encyclopedia) → Rockets/computers/physics → Leave for Canada**.
3. Canada unlocks. **Inbox** gets *North to Canada* → accept → **Map** → **Travel** (ticket completes on arrival).
4. **Companies** → **Found Zip2** → Zip2 tickets appear in the Inbox. Use **Prev/Next** if needed to select a Pending Zip2 ticket (completed guidance tickets no longer stick).
5. Accept a Zip2 problem → **Map** (**Next Loc** cycles unlocked destinations) → **Travel** to Palo Alto → **Resolve** → pick a choice (stats change).
6. Complete *The Acquisition Offer* with **Sell** → **Found X.com** → X.com problems arrive.

Placeholder UI/sprites are labeled `PLACEHOLDER` in hierarchy and logs.

---

## Architecture

```
Assets/Scripts/
  Core/                      # PURE C# (no UnityEngine) — unit-testable
    Models/                  # Ticket, Location, Company, Problem, Dialogue
    Services/                # Inbox, Travel, Dialogue, CompanyManager, Act1, GameSession
    Content/                 # Act1Content, CompanyContent, CompanyProblemsContent, PrototypeContent
  Data/                      # ScriptableObject wrappers (Company, Problem, Ticket, Location)
  Unity/                     # UI, bootstrap, scene flow, player
Core.Tests/                  # Compiles shipped Core/** and runs session-level tests
```

| System | Class | Role |
|--------|--------|------|
| **Act 1** | `Act1Progression` + `Act1Content` | Ordered narrative beats; unlocks Canada + Zip2 founding |
| **Companies** | `CompanyManager` + `CompanyState` | Money, Progress, Engineering, Public Opinion, Status |
| **Problems** | `ProblemDefinition` + `CompanyProblemsContent` | Inbox tickets with 2–3 choices → stat deltas |
| **Inbox** | `InboxService` | Receive / list / accept / complete tickets |
| **Travel** | `TravelService` | Locations + **unlock** gates + scene names |
| **Session** | `GameSession` | Wires progression: Act1 → Canada → Found Zip2 → tickets → resolve |

Unity UI only **displays and calls** session APIs — no parallel game rules.

---

## Act 1 flow (South Africa)

Beats (dialogue; Home / School / Library labeled in text — not full multi-room tilemaps):

1. `act1_home_intro` — Pretoria home  
2. `act1_school_staircase` — bullying / staircase incident (respectful framing)  
3. `act1_library_encyclopedia` — obsessive encyclopedia reading  
4. `act1_rockets_computers_physics` — early interest in rockets, computers, physics  
5. `act1_leave_for_canada` — decision to leave → **Complete**

On complete: `Travel.Unlock(toronto)`, guidance ticket `act1_travel_canada`, `Zip2FoundingUnlocked = true`.

---

## Company system

Each company tracks:

| Field | Meaning |
|--------|---------|
| `DisplayName` | e.g. Zip2, X.com |
| `Money` | Resources / cash |
| `Progress` | Product / business progress |
| `EngineeringLevel` | Technical strength |
| `PublicOpinion` | Reputation (0–100) |
| `Status` | `NotFounded` / `Active` / `Sold` / `Merged` / `Inactive` |

**Starting roster:** Zip2 + X.com (`NotFounded`).

```csharp
session.FoundZip2();   // after Act 1 → Active + seed money + Zip2 tickets
session.FoundXCom();   // after Zip2 Sold → Active + X.com tickets
session.ResolveProblem(ticketId, choiceIndex); // applies ProblemChoice deltas
```

### Status changes via problems

- Zip2 **sale** choice can set `Sold` and enable **Found X.com**.
- X.com **merger** choices can set `Merged`.

---

## Zip2 / X.com inbox problems (6)

| Id | Company | Theme |
|----|---------|--------|
| `zip2_first_big_customer` | Zip2 | Yellow pages / first big customer |
| `zip2_maps_directories_tech` | Zip2 | Maps & directories engineering |
| `zip2_sale_to_compaq` | Zip2 | Acquisition decision |
| `xcom_fraud_banking_pressure` | X.com | Fraud / banking controls |
| `xcom_confinity_merger` | X.com | Merger with Confinity (PayPal) |
| `xcom_internal_leadership` | X.com | Leadership / product direction |

Each has a real-event-inspired description, target location (`palo_alto`), and **2–3 choices** with distinct money/progress/opinion/engineering (and optional status) deltas.

Tickets are delivered by `GameSession.DeliverZip2Tickets()` / `DeliverXComTickets()` when the company is founded — not hard-coded only in a scene button.

---

## How to expand

### Add a company

1. Add id + `CompanyState` in `CompanyContent.CreateStartingCompanies()` (or SO `Company Definition` + loader).
2. Register is automatic via `StartNewGame`.
3. Add `FoundX()` on `GameSession` if founding is gated.
4. Author problems in `CompanyProblemsContent` (or `Problem Definition` SO).

### Add an inbox problem

1. New `ProblemDefinition` in `CompanyProblemsContent` with **2–3** `ProblemChoice`s and deltas.
2. Include it in `CreateZip2Problems` / `CreateXComProblems` filters (by `companyId`) or a new pack.
3. Ensure founding path calls `Inbox.ReceiveTicket(problem.ToTicket())`.
4. Keep tone respectful / non-mocking.

### Add an Act 1 (or later) beat

1. New beat enum value in `Act1Progression` (or a new act class).
2. Dialogue in `Act1Content`.
3. Wire `Advance()` chain and unlocks in `GameSession`.

### Add a location

1. Scene under `Assets/Scenes/` + Build Settings.  
2. `GameLocation` in `PrototypeContent.CreateLocations()`.  
3. Unlock when progression allows (`Travel.Unlock`).

---

## Tests

```bash
dotnet run --project Core.Tests -c Release
```

Exercises **shipped** Core: Act1 unlock gate, company found/stats, distinct choice deltas, Zip2/X.com ticket delivery, full session path. Run twice for consistency.

---

## Placeholder art

- `Assets/Art/Placeholders/` + runtime names like `Player_YoungElon_PLACEHOLDER`, `HUD_Canvas_PLACEHOLDER`.
- Replace with 16-bit / SNES-style art; era palettes: muted early years → vibrant modern → Mars later.

---

## Roadmap (later)

- Tesla, SpaceX, and remaining companies  
- Timed inbox spawns, minigames, save/load  
- Full tilemap school/home/library and production pixel art  

---

## Contribution

Prefer new **content** (problems, companies, dialogues, scenes) over editing core services. Keep historical portrayal careful and non-mocking.
