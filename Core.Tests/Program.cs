using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Models;
using ElonLifeSim.Core.Services;

namespace ElonLifeSim.Core.Tests
{
    /// <summary>
    /// Pure C# harness driving SHIPPED Core code (not a reimplementation).
    /// Run: dotnet run --project Core.Tests
    /// </summary>
    public static class Program
    {
        private static int _passed;
        private static int _failed;

        public static int Main(string[] args)
        {
            Console.WriteLine("=== ElonLifeSim Core + Act1 + Company Tests ===");
            Console.WriteLine($"Time: {DateTime.UtcNow:O}");
            Console.WriteLine();

            // Baseline systems
            Run("Inbox_ReceiveListAccept", TestInboxBasic);
            Run("Travel_UnlockGate", TestTravelUnlock);
            Run("Dialogue_ContinuePath", TestDialogueContinue);

            // Act 1
            Run("Act1_AllNamedBeats_UnlockCanada", TestAct1FullSequence);
            Run("Act1_CanadaLockedUntilComplete", TestCanadaLockedUntilAct1);

            // Companies
            Run("Companies_Zip2AndXCom_Registered", TestCompaniesRegistered);
            Run("Companies_FoundZip2_UpdatesStatusAndMoney", TestFoundZip2);
            Run("Companies_ChoiceDeltas_Distinct", TestChoiceDeltasDistinct);
            Run("Companies_Zip2Sale_ThenFoundXCom", TestZip2SaleThenXCom);

            // Inbox after founding
            Run("Zip2Founded_DeliversZip2Tickets", TestZip2TicketDelivery);
            Run("XComFounded_DeliversXComTickets", TestXComTicketDelivery);
            Run("ProblemResolve_AppliesStatsAndCompletesTicket", TestResolveProblem);

            // Content inventory
            Run("ContentInventory_Act1EventsAndProblems", TestContentInventory);
            Run("Tone_NoMockingKeywords", TestTone);

            // Full session path
            Run("Session_Act1_To_Zip2_To_Resolve", TestFullSessionPath);

            // Skeptic fixes: selection, guidance complete, free map travel
            Run("InboxSelection_SkipsCompleted_PicksPending", TestInboxSelectionSkipsCompleted);
            Run("InboxSelection_NextCyclesAllTickets", TestInboxSelectionNext);
            Run("GuidanceTicket_CompletesOnTravel", TestGuidanceTicketCompletesOnTravel);
            Run("AfterZip2Found_PendingZip2Selectable", TestAfterZip2PendingSelectable);
            Run("TravelMapSelection_FreeTravelAmongUnlocked", TestFreeMapTravelSelection);
            Run("Session_FreeTravel_AfterAct1WithoutTicket", TestSessionFreeTravelAfterAct1);

            // Elon era swap: shipped location → era (+ resource keys); act hook ignored
            Run("ElonEra_LocationMappingAndResourceKeys", TestElonEraLocationMappingAndResourceKeys);
            Run("ElonEra_SwapOnTravelLocationChanged", TestElonEraSwapOnTravelLocationChanged);
            Run("DebugJump_F1F2F3_MapsAndUnlockTravelWithoutAct1", TestDebugJumpF1F2F3);
            Run("DebugJump_F4F5_MissingAsPlaceOnCurrentRegistry", TestDebugJumpF4F5Missing);
            Run("HudExclusivity_ToggleOpenCloseAndDialogueClears", TestHudPanelExclusivity);
            Run("HudExclusivity_MenuAndStory", TestHudPanelMenuAndStory);
            Run("UiStyleTokens_HierarchyAndCopy", TestUiStyleTokens);
            Run("TopBarLayout_InboxFullyVisible_EqualButtons", TestTopBarLayoutNoClip);
            Run("HudStatusCopy_Act1Pretoria", TestHudStatusCopy);
            Run("WorldBackdrop_PretoriaNotEditorGray", TestWorldBackdropPretoria);
            Run("HudNavHighlight_AndSheetCloseToWorld", TestHudNavHighlightAndSheetClose);
            Run("DialogueStrip_StaysBelowTopBar", TestDialogueStripBelowTopBar);
            Run("HudSource_WiresTopBarAndBackdrop", TestHudSourceWiresLayout);
            Run("UiTokens_AaaPaletteTypeMotionDimFilter", TestAaaThemeTokens);
            Run("TitleScreen_TaglineAndCtas", TestTitleScreenCopy);
            Run("WorldBackdrop_HorizonLineAndVignette", TestWorldHorizonLine);

            Console.WriteLine();
            Console.WriteLine($"Results: {_passed} passed, {_failed} failed");
            if (_failed > 0)
            {
                Console.WriteLine("FAILED");
                return 1;
            }

            Console.WriteLine("ALL PASSED");
            return 0;
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                _passed++;
                Console.WriteLine($"[PASS] {name}");
            }
            catch (Exception ex)
            {
                _failed++;
                Console.WriteLine($"[FAIL] {name}");
                Console.WriteLine($"       {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new Exception(message);
        }

        private static void TestInboxBasic()
        {
            var inbox = new InboxService();
            var problem = CompanyProblemsContent.CreateZip2Problems()[0];
            var ticket = problem.ToTicket();
            Assert(inbox.ReceiveTicket(ticket), "receive");
            Assert(inbox.ListTickets().Count == 1, "count");
            Assert(inbox.AcceptTicket(ticket.Id), "accept");
            Assert(inbox.TryGet(ticket.Id, out var t) && t.Status == TicketStatus.Accepted, "status");
        }

        private static void TestTravelUnlock()
        {
            var travel = new TravelService();
            travel.RegisterLocations(PrototypeContent.CreateLocations());
            travel.SetStartingLocation(PrototypeContent.LocationPretoria);
            Assert(travel.IsUnlocked(PrototypeContent.LocationPretoria), "start unlocked");
            Assert(!travel.IsUnlocked(PrototypeContent.LocationToronto), "canada locked");
            Assert(!travel.TravelTo(PrototypeContent.LocationToronto), "cannot travel locked");
            Assert(travel.Unlock(PrototypeContent.LocationToronto), "unlock");
            Assert(travel.TravelTo(PrototypeContent.LocationToronto), "travel ok");
            Assert(travel.CurrentLocationId == PrototypeContent.LocationToronto, "arrived");
        }

        private static void TestDialogueContinue()
        {
            var node = new DialogueNode("only", new List<DialogueLine>
            {
                new DialogueLine("N", "A"),
                new DialogueLine("N", "B")
            });
            var def = new DialogueDefinition("d", "only", new[] { node });
            var runner = new DialogueRunner();
            Assert(runner.Start(def), "start");
            Assert(runner.Advance(), "adv1");
            Assert(runner.Advance(), "adv2");
            Assert(runner.IsComplete, "done");
        }

        private static void TestAct1FullSequence()
        {
            var act1 = new Act1Progression();
            act1.Begin();
            Assert(act1.CurrentBeat == Act1Progression.Beat.HomeIntro, "home");
            Assert(act1.GetCurrentDialogue() != null, "home dialogue");
            Assert(act1.GetCurrentDialogue().Id == "act1_home_intro", "home id");

            Assert(act1.Advance(), "to school");
            Assert(act1.CurrentBeat == Act1Progression.Beat.SchoolStaircase, "school");
            Assert(act1.GetCurrentDialogue().Id == "act1_school_staircase", "school id");

            Assert(act1.Advance(), "to library");
            Assert(act1.CurrentBeat == Act1Progression.Beat.LibraryEncyclopedia, "library");
            Assert(act1.GetCurrentDialogue().Id == "act1_library_encyclopedia", "lib id");

            Assert(act1.Advance(), "to tech");
            Assert(act1.CurrentBeat == Act1Progression.Beat.RocketsComputersPhysics, "tech");
            Assert(act1.GetCurrentDialogue().Id == "act1_rockets_computers_physics", "tech id");

            Assert(act1.Advance(), "to leave");
            Assert(act1.CurrentBeat == Act1Progression.Beat.LeaveForCanada, "leave");
            Assert(act1.GetCurrentDialogue().Id == "act1_leave_for_canada", "leave id");

            Assert(act1.Advance(), "complete");
            Assert(act1.IsComplete, "complete");
            Assert(act1.CanadaUnlocked, "canada flag");
            Assert(act1.Zip2FoundingUnlocked, "zip2 flag");
        }

        private static void TestCanadaLockedUntilAct1()
        {
            var session = new GameSession();
            session.StartNewGame();
            Assert(!session.Travel.IsUnlocked(PrototypeContent.LocationToronto), "locked");
            Assert(!session.Travel.TravelTo(PrototypeContent.LocationToronto), "no travel");

            while (!session.Act1.IsComplete)
                Assert(session.AdvanceAct1Beat(), "advance");

            Assert(session.Travel.IsUnlocked(PrototypeContent.LocationToronto), "unlocked");
            Assert(session.Inbox.ListTickets().Any(t => t.Id == "act1_travel_canada"), "travel ticket");
            Assert(session.Travel.TravelTo(PrototypeContent.LocationToronto), "travel canada");
        }

        private static void TestCompaniesRegistered()
        {
            var session = new GameSession();
            session.StartNewGame();
            Assert(session.Companies.Count == 2, "two companies");
            Assert(session.Companies.TryGet(CompanyContent.Zip2, out var z), "zip2");
            Assert(z.Status == CompanyStatus.NotFounded, "zip2 not founded");
            Assert(session.Companies.TryGet(CompanyContent.XCom, out var x), "xcom");
            Assert(x.Status == CompanyStatus.NotFounded, "xcom not founded");
        }

        private static void TestFoundZip2()
        {
            var session = new GameSession();
            session.StartNewGame();
            Assert(!session.FoundZip2(), "cannot found before act1");
            CompleteAct1(session);
            Assert(session.CanFoundZip2(), "can found");
            Assert(session.FoundZip2(), "found");
            Assert(session.Companies.TryGet(CompanyContent.Zip2, out var z), "get");
            Assert(z.Status == CompanyStatus.Active, "active");
            Assert(z.Money >= 25, "seed money");
            Assert(session.Travel.IsUnlocked(PrototypeContent.LocationPaloAlto), "sv unlocked");
        }

        private static void TestChoiceDeltasDistinct()
        {
            var problem = CompanyProblemsContent.CreateAll()
                .First(p => p.Id == CompanyProblemsContent.Zip2FirstCustomer);
            Assert(problem.Choices.Count >= 2 && problem.Choices.Count <= 3, "2-3 choices");

            var mgr = new CompanyManager();
            mgr.RegisterMany(CompanyContent.CreateStartingCompanies());
            mgr.Found(CompanyContent.Zip2, 0);

            mgr.TryGet(CompanyContent.Zip2, out var before0);
            mgr.ApplyChoice(CompanyContent.Zip2, problem.Choices[0]);
            mgr.TryGet(CompanyContent.Zip2, out var after0);

            // Reset second company path with fresh manager
            var mgr2 = new CompanyManager();
            mgr2.RegisterMany(CompanyContent.CreateStartingCompanies());
            mgr2.Found(CompanyContent.Zip2, 0);
            mgr2.ApplyChoice(CompanyContent.Zip2, problem.Choices[1]);
            mgr2.TryGet(CompanyContent.Zip2, out var after1);

            bool distinct =
                after0.Money != after1.Money ||
                after0.Progress != after1.Progress ||
                after0.PublicOpinion != after1.PublicOpinion ||
                after0.EngineeringLevel != after1.EngineeringLevel;
            Assert(distinct, "choices must produce different stats");
            Assert(after0.Money == before0.Money + problem.Choices[0].MoneyDelta, "choice0 money applied via shipped ApplyChoice");
        }

        private static void TestZip2SaleThenXCom()
        {
            var session = PrepareWithZip2();
            var sale = CompanyProblemsContent.Zip2Sale;
            Assert(session.AcceptTicket(sale), "accept sale");
            // Sell choice index 0
            Assert(session.ResolveProblem(sale, 0), "sell");
            Assert(session.Companies.TryGet(CompanyContent.Zip2, out var z), "zip2");
            Assert(z.Status == CompanyStatus.Sold, "sold");
            Assert(session.CanFoundXCom(), "can found xcom");
            Assert(session.FoundXCom(), "found xcom");
            Assert(session.Companies.TryGet(CompanyContent.XCom, out var x) && x.Status == CompanyStatus.Active, "xcom active");
        }

        private static void TestZip2TicketDelivery()
        {
            var session = new GameSession();
            session.StartNewGame();
            CompleteAct1(session);
            int before = session.Inbox.Count;
            Assert(session.FoundZip2(), "found");
            Assert(session.Zip2TicketsDelivered, "flag");
            var zip2Ids = CompanyProblemsContent.CreateZip2Problems().Select(p => p.Id).ToList();
            Assert(zip2Ids.Count >= 3, "at least 3 zip2 problems");
            foreach (var id in zip2Ids)
                Assert(session.Inbox.TryGet(id, out _), "ticket " + id);
            Assert(session.Inbox.Count > before, "inbox grew");
            Assert(session.DeliverZip2Tickets() == 0, "idempotent");
        }

        private static void TestXComTicketDelivery()
        {
            var session = PrepareWithZip2();
            session.ResolveProblem(CompanyProblemsContent.Zip2Sale, 0);
            session.FoundXCom();
            var ids = CompanyProblemsContent.CreateXComProblems().Select(p => p.Id).ToList();
            Assert(ids.Count >= 2, "xcom problems");
            foreach (var id in ids)
                Assert(session.Inbox.TryGet(id, out _), "xcom ticket " + id);
        }

        private static void TestResolveProblem()
        {
            var session = PrepareWithZip2();
            var id = CompanyProblemsContent.Zip2MapsTech;
            session.Companies.TryGet(CompanyContent.Zip2, out var before);
            Assert(session.ResolveProblem(id, 0), "resolve");
            session.Companies.TryGet(CompanyContent.Zip2, out var after);
            var problem = CompanyProblemsContent.CreateAll().First(p => p.Id == id);
            var c0 = problem.Choices[0];
            Assert(after.Money == before.Money + c0.MoneyDelta, "money delta");
            Assert(after.EngineeringLevel == before.EngineeringLevel + c0.EngineeringDelta, "eng delta");
            Assert(session.Inbox.TryGet(id, out var t) && t.Status == TicketStatus.Completed, "completed");
            Assert(!string.IsNullOrEmpty(session.LastResolutionNarration), "narration");
        }

        private static void TestContentInventory()
        {
            // Act 1 named events
            foreach (var id in Act1Progression.NamedEventIds)
            {
                bool found = false;
                foreach (Act1Progression.Beat b in Enum.GetValues(typeof(Act1Progression.Beat)))
                {
                    var d = Act1Content.GetDialogueForBeat(b);
                    if (d != null && d.Id == id) { found = true; break; }
                }
                Assert(found, "act1 event " + id);
            }

            var problems = CompanyProblemsContent.CreateAll();
            Assert(problems.Count >= 4, ">=4 problems, got " + problems.Count);
            Assert(problems.Count(p => p.CompanyId == CompanyContent.Zip2) >= 3, "zip2 problems");
            Assert(problems.Count(p => p.CompanyId == CompanyContent.XCom) >= 2, "xcom problems");
            foreach (var p in problems)
            {
                Assert(!string.IsNullOrWhiteSpace(p.Description), p.Id + " description");
                Assert(p.Choices.Count >= 2 && p.Choices.Count <= 3, p.Id + " choices 2-3");
                Assert(!string.IsNullOrWhiteSpace(p.LocationId), p.Id + " location");
            }
        }

        private static void TestTone()
        {
            var blobs = new List<string>();
            foreach (Act1Progression.Beat b in Enum.GetValues(typeof(Act1Progression.Beat)))
            {
                var d = Act1Content.GetDialogueForBeat(b);
                if (d == null) continue;
                foreach (var node in d.Nodes.Values)
                foreach (var line in node.Lines)
                    blobs.Add(line.Text);
            }
            foreach (var p in CompanyProblemsContent.CreateAll())
            {
                blobs.Add(p.Title);
                blobs.Add(p.Description);
            }
            var all = string.Join(" ", blobs).ToLowerInvariant();
            Assert(!all.Contains("idiot"), "no idiot");
            Assert(!all.Contains("loser"), "no loser");
            Assert(!all.Contains("moron"), "no moron");
        }

        private static void TestFullSessionPath()
        {
            var session = new GameSession();
            session.StartNewGame();
            Assert(session.HasStarted, "started");
            Assert(session.Act1.CurrentBeat == Act1Progression.Beat.HomeIntro, "act1 start");

            CompleteAct1(session);
            Assert(session.Act1.CanadaUnlocked || session.Travel.IsUnlocked(PrototypeContent.LocationToronto), "canada");
            Assert(session.TravelTo(PrototypeContent.LocationToronto), "to canada");

            Assert(session.FoundZip2(), "found zip2");
            Assert(session.Inbox.ListPending().Any(t => t.CompanyId == CompanyContent.Zip2), "zip2 pending");

            var firstZip = CompanyProblemsContent.Zip2FirstCustomer;
            Assert(session.AcceptTicket(firstZip), "accept");
            Assert(session.TravelToActiveTicketLocation(), "to palo alto");
            Assert(session.Travel.CurrentLocationId == PrototypeContent.LocationPaloAlto, "at SV");
            Assert(session.ResolveProblem(firstZip, 0), "resolve");
        }

        private static void CompleteAct1(GameSession session)
        {
            int guard = 0;
            while (!session.Act1.IsComplete && guard++ < 20)
                session.AdvanceAct1Beat();
            Assert(session.Act1.IsComplete, "act1 should complete");
        }

        private static GameSession PrepareWithZip2()
        {
            var session = new GameSession();
            session.StartNewGame();
            CompleteAct1(session);
            Assert(session.FoundZip2(), "found zip2");
            return session;
        }

        private static void TestInboxSelectionSkipsCompleted()
        {
            var inbox = new InboxService();
            inbox.ReceiveTicket(new InboxTicket(
                "guide", "personal", "Personal", "toronto", "Toronto",
                "Guide", "Go north", 1, "arrive"));
            var problem = CompanyProblemsContent.CreateZip2Problems()[0];
            inbox.ReceiveTicket(problem.ToTicket());

            inbox.AcceptTicket("guide");
            inbox.CompleteTicket("guide");

            var list = inbox.ListTickets();
            // Stale selection on completed guide must re-pick pending problem
            var selected = InboxSelection.EnsureSelected(list, "guide");
            Assert(selected == problem.Id, "should select pending problem, got " + selected);
            Assert(InboxSelection.IsActionable(list.First(t => t.Id == selected)), "actionable");
        }

        private static void TestInboxSelectionNext()
        {
            var inbox = new InboxService();
            var problems = CompanyProblemsContent.CreateZip2Problems();
            foreach (var p in problems)
                inbox.ReceiveTicket(p.ToTicket());
            var list = inbox.ListTickets();
            Assert(list.Count >= 3, "need several tickets");
            var a = InboxSelection.EnsureSelected(list, null);
            var b = InboxSelection.SelectNext(list, a);
            var c = InboxSelection.SelectNext(list, b);
            Assert(a != b || list.Count == 1, "next moves");
            Assert(b != c || list.Count <= 2, "next moves again");
            // Full cycle returns to a
            var cur = a;
            for (int i = 0; i < list.Count; i++)
                cur = InboxSelection.SelectNext(list, cur);
            Assert(cur == a, "full cycle wraps");
        }

        private static void TestGuidanceTicketCompletesOnTravel()
        {
            var session = new GameSession();
            session.StartNewGame();
            CompleteAct1(session);
            Assert(session.Inbox.TryGet("act1_travel_canada", out var guide), "guide exists");
            Assert(guide.Status == TicketStatus.Pending, "pending");
            Assert(session.AcceptTicket("act1_travel_canada"), "accept");
            Assert(session.TravelToActiveTicketLocation(), "travel");
            Assert(session.Travel.CurrentLocationId == PrototypeContent.LocationToronto, "at canada");
            Assert(session.Inbox.TryGet("act1_travel_canada", out var done), "still listed");
            Assert(done.Status == TicketStatus.Completed, "guidance completed on travel, was " + done.Status);
            Assert(string.IsNullOrEmpty(session.ActiveTicketId), "active cleared");
        }

        private static void TestAfterZip2PendingSelectable()
        {
            // Spot-check path: Act1 → travel completes guide → FoundZip2 → selection prefers Pending Zip2
            var session = new GameSession();
            session.StartNewGame();
            CompleteAct1(session);
            session.AcceptTicket("act1_travel_canada");
            session.TravelToActiveTicketLocation();
            Assert(session.FoundZip2(), "found zip2");

            var list = session.Inbox.ListTickets();
            // Simulate sticky selection on completed guide (the old bug)
            var selected = InboxSelection.EnsureSelected(list, "act1_travel_canada");
            Assert(selected != "act1_travel_canada", "must not stick on completed guide");
            Assert(session.Inbox.TryGet(selected, out var t), "selected exists");
            Assert(t.Status == TicketStatus.Pending, "selected pending");
            Assert(t.CompanyId == CompanyContent.Zip2, "zip2 problem");
            Assert(session.TryGetProblem(selected, out _), "is company problem");
        }

        private static void TestFreeMapTravelSelection()
        {
            var travel = new TravelService();
            travel.RegisterLocations(PrototypeContent.CreateLocations());
            travel.SetStartingLocation(PrototypeContent.LocationPretoria);
            travel.Unlock(PrototypeContent.LocationToronto);
            travel.Unlock(PrototypeContent.LocationPaloAlto);

            var unlocked = travel.GetUnlockedLocations().ToList();
            // Opening map with no target should pick a non-current unlocked location
            var target = TravelMapSelection.EnsureTarget(unlocked, travel.CurrentLocationId, null);
            Assert(!string.IsNullOrEmpty(target), "default target");
            Assert(target != PrototypeContent.LocationPretoria, "not current");
            Assert(travel.IsUnlocked(target), "unlocked");

            var next = TravelMapSelection.SelectNext(unlocked, travel.CurrentLocationId, target);
            Assert(!string.IsNullOrEmpty(next), "next target");
            var prev = TravelMapSelection.SelectPrevious(unlocked, travel.CurrentLocationId, next);
            Assert(prev == target, "previous returns to prior target");

            // Free travel without a ticket
            Assert(travel.TravelTo(target), "free travel");
            Assert(travel.CurrentLocationId == target, "arrived free");

            // Can return / go elsewhere
            var other = TravelMapSelection.EnsureTarget(
                travel.GetUnlockedLocations().ToList(), travel.CurrentLocationId, null);
            Assert(travel.TravelTo(other), "travel again");
        }

        private static void TestSessionFreeTravelAfterAct1()
        {
            var session = new GameSession();
            session.StartNewGame();
            CompleteAct1(session);
            // Free travel to Canada without using the guidance ticket
            Assert(session.Travel.IsUnlocked(PrototypeContent.LocationToronto), "unlocked");
            var unlocked = session.Travel.GetUnlockedLocations().ToList();
            var target = TravelMapSelection.EnsureTarget(
                unlocked, session.Travel.CurrentLocationId, PrototypeContent.LocationToronto);
            Assert(target == PrototypeContent.LocationToronto, "target canada");
            Assert(session.TravelTo(target), "session free travel");
            Assert(session.Travel.CurrentLocationId == PrototypeContent.LocationToronto, "at canada free");
        }

        private static void TestElonEraLocationMappingAndResourceKeys()
        {
            var pret = ElonEraResolver.EraFolderForLocation(PrototypeContent.LocationPretoria);
            var tor = ElonEraResolver.EraFolderForLocation(PrototypeContent.LocationToronto);
            var palo = ElonEraResolver.EraFolderForLocation(PrototypeContent.LocationPaloAlto);
            var unknown = ElonEraResolver.EraFolderForLocation("not_a_place");
            var missing = ElonEraResolver.EraFolderForLocation(null);

            Assert(pret == "01_young_sa", "pretoria → school era, got " + pret);
            Assert(tor == "02_young_adult_90s", "toronto → 90s era, got " + tor);
            Assert(palo == "03_early_2000s", "palo alto → early-2000s era, got " + palo);
            Assert(unknown == "04_modern", "unknown → modern, got " + unknown);
            Assert(missing == "04_modern", "null → modern, got " + missing);
            Assert(pret != tor && tor != palo && pret != palo, "named location eras must differ");

            Assert(ElonEraResolver.IdleResourceKey(PrototypeContent.LocationPretoria)
                   == "Characters/Elon/01_young_sa/elon_young_sa_idle", "pretoria idle key");
            Assert(ElonEraResolver.PortraitResourceKey(PrototypeContent.LocationPretoria)
                   == "Characters/Elon/01_young_sa/elon_young_sa_portrait", "pretoria portrait key");
            Assert(ElonEraResolver.WalkResourceKey(PrototypeContent.LocationPretoria, 0)
                   == "Characters/Elon/01_young_sa/walk/elon_young_sa_walk_00", "pretoria walk_00 key");

            Assert(ElonEraResolver.IdleResourceKey(PrototypeContent.LocationToronto)
                   == "Characters/Elon/02_young_adult_90s/elon_young_adult_idle", "toronto idle key");
            Assert(ElonEraResolver.PortraitResourceKey(PrototypeContent.LocationToronto)
                   == "Characters/Elon/02_young_adult_90s/elon_young_adult_portrait", "toronto portrait key");
            Assert(ElonEraResolver.WalkResourceKey(PrototypeContent.LocationToronto, 1)
                   == "Characters/Elon/02_young_adult_90s/walk/elon_young_adult_walk_01", "toronto walk_01 key");

            Assert(ElonEraResolver.IdleResourceKey(PrototypeContent.LocationPaloAlto)
                   == "Characters/Elon/03_early_2000s/elon_early2000s_idle", "palo idle key");
            Assert(ElonEraResolver.PortraitResourceKey(PrototypeContent.LocationPaloAlto)
                   == "Characters/Elon/03_early_2000s/elon_early2000s_portrait", "palo portrait key");
            Assert(ElonEraResolver.WalkResourceKey(PrototypeContent.LocationPaloAlto, 0)
                   == "Characters/Elon/03_early_2000s/walk/elon_early2000s_walk_00", "palo walk_00 key");

            Assert(ElonEraResolver.IdleResourceKey("somewhere_else")
                   == "Characters/Elon/04_modern/elon_modern_idle", "default idle key");
            Assert(ElonEraResolver.EraFolderForLocation("mars") == "05_mars", "mars location selects mars era");

            // Act hook exists but must not change Pretoria / Toronto / Palo Alto.
            Assert(ElonEraResolver.EraFolderForLocation(PrototypeContent.LocationPretoria, "act8_mars") == pret,
                "actId must not override Pretoria");
            Assert(ElonEraResolver.EraFolderForLocation(PrototypeContent.LocationToronto, "act3") == tor,
                "actId must not override Toronto");
            Assert(ElonEraResolver.EraFolderForLocation(PrototypeContent.LocationPaloAlto, "act8") == palo,
                "actId must not override Palo Alto");
            Assert(ElonEraResolver.IdleResourceKey(PrototypeContent.LocationPretoria, "act8")
                   == ElonEraResolver.IdleResourceKey(PrototypeContent.LocationPretoria),
                "actId must not change Pretoria idle key");
        }

        private static void TestElonEraSwapOnTravelLocationChanged()
        {
            var travel = new TravelService();
            travel.RegisterLocations(PrototypeContent.CreateLocations());
            travel.SetStartingLocation(PrototypeContent.LocationPretoria);
            travel.Unlock(PrototypeContent.LocationToronto);
            travel.Unlock(PrototypeContent.LocationPaloAlto);

            string eraFromEvent = null;
            string idleFromEvent = null;
            string portraitFromEvent = null;
            travel.LocationChanged += (_, newId) =>
            {
                eraFromEvent = ElonEraResolver.EraFolderForLocation(newId);
                idleFromEvent = ElonEraResolver.IdleResourceKey(newId);
                portraitFromEvent = ElonEraResolver.PortraitResourceKey(newId);
            };

            Assert(ElonEraResolver.EraFolderForLocation(travel.CurrentLocationId) == "01_young_sa",
                "start Pretoria school era");

            Assert(travel.TravelTo(PrototypeContent.LocationToronto), "travel toronto");
            Assert(eraFromEvent == "02_young_adult_90s", "LocationChanged era toronto, got " + eraFromEvent);
            Assert(idleFromEvent == "Characters/Elon/02_young_adult_90s/elon_young_adult_idle",
                "LocationChanged idle toronto");
            Assert(portraitFromEvent == "Characters/Elon/02_young_adult_90s/elon_young_adult_portrait",
                "LocationChanged portrait toronto");
            Assert(ElonEraResolver.EraFolderForLocation(travel.CurrentLocationId) == "02_young_adult_90s",
                "current after toronto");

            Assert(travel.TravelTo(PrototypeContent.LocationPaloAlto), "travel palo alto");
            Assert(eraFromEvent == "03_early_2000s", "LocationChanged era palo, got " + eraFromEvent);
            Assert(idleFromEvent == "Characters/Elon/03_early_2000s/elon_early2000s_idle",
                "LocationChanged idle palo");
            Assert(portraitFromEvent == "Characters/Elon/03_early_2000s/elon_early2000s_portrait",
                "LocationChanged portrait palo");
            Assert(ElonEraResolver.EraFolderForLocation(travel.CurrentLocationId) == "03_early_2000s",
                "current after palo");
        }

        private static void TestDebugJumpF1F2F3()
        {
            var f1 = DebugLocationJumpMap.ForKey(1);
            var f2 = DebugLocationJumpMap.ForKey(2);
            var f3 = DebugLocationJumpMap.ForKey(3);
            Assert(f1.LocationId == PrototypeContent.LocationPretoria, "F1 location");
            Assert(f2.LocationId == PrototypeContent.LocationToronto, "F2 location");
            Assert(f3.LocationId == PrototypeContent.LocationPaloAlto, "F3 location");
            Assert(f1.EraFolder == "01_young_sa", "F1 era " + f1.EraFolder);
            Assert(f2.EraFolder == "02_young_adult_90s", "F2 era " + f2.EraFolder);
            Assert(f3.EraFolder == "03_early_2000s", "F3 era " + f3.EraFolder);
            Assert(f1.PlaceExists && f2.PlaceExists && f3.PlaceExists, "F1–F3 places exist");
            Assert(f1.EraFolder != f2.EraFolder && f2.EraFolder != f3.EraFolder && f1.EraFolder != f3.EraFolder,
                "F1–F3 eras differ");

            Assert(ElonEraResolver.IdleResourceKey(f1.LocationId)
                   == "Characters/Elon/01_young_sa/elon_young_sa_idle", "F1 idle");
            Assert(ElonEraResolver.PortraitResourceKey(f1.LocationId)
                   == "Characters/Elon/01_young_sa/elon_young_sa_portrait", "F1 portrait");
            Assert(ElonEraResolver.IdleResourceKey(f2.LocationId)
                   == "Characters/Elon/02_young_adult_90s/elon_young_adult_idle", "F2 idle");
            Assert(ElonEraResolver.PortraitResourceKey(f2.LocationId)
                   == "Characters/Elon/02_young_adult_90s/elon_young_adult_portrait", "F2 portrait");
            Assert(ElonEraResolver.IdleResourceKey(f3.LocationId)
                   == "Characters/Elon/03_early_2000s/elon_early2000s_idle", "F3 idle");
            Assert(ElonEraResolver.PortraitResourceKey(f3.LocationId)
                   == "Characters/Elon/03_early_2000s/elon_early2000s_portrait", "F3 portrait");

            // Unlock + TravelTo on TravelService — no Act1 complete, no GameSession.TravelTo.
            var session = new GameSession();
            session.StartNewGame();
            Assert(!session.Act1.IsComplete, "act1 not complete");
            Assert(!session.Travel.IsUnlocked(PrototypeContent.LocationToronto), "toronto locked until jump");
            Assert(!session.Travel.IsUnlocked(PrototypeContent.LocationPaloAlto), "palo locked until jump");

            string eraFromEvent = null;
            session.Travel.LocationChanged += (_, id) =>
                eraFromEvent = ElonEraResolver.EraFolderForLocation(id);

            var r1 = DebugLocationJumpMap.TryJump(session.Travel, 1);
            Assert(!r1.PlaceMissing, "F1 not missing");
            Assert(r1.ToLocationId == PrototypeContent.LocationPretoria, "F1 stays/goes pretoria");
            Assert(r1.EraFolder == "01_young_sa", "F1 result era");
            Assert(r1.Log == DebugLocationJumpMap.FormatLog(r1.FromLocationId, r1.ToLocationId, r1.EraFolder, false),
                "F1 log");
            Assert(r1.Log.Contains("[DebugJump]") && r1.Log.Contains("era=01_young_sa"), "F1 log tags");
            Assert(!session.Act1.IsComplete, "F1 did not complete act1");

            var r2 = DebugLocationJumpMap.TryJump(session.Travel, 2);
            Assert(r2.Moved, "F2 moved");
            Assert(r2.ToLocationId == PrototypeContent.LocationToronto, "F2 toronto");
            Assert(r2.EraFolder == "02_young_adult_90s", "F2 era");
            Assert(eraFromEvent == "02_young_adult_90s", "LocationChanged era F2");
            Assert(session.Travel.CurrentLocationId == PrototypeContent.LocationToronto, "travel current F2");
            Assert(session.Travel.IsUnlocked(PrototypeContent.LocationToronto), "F2 unlocked toronto");
            Assert(!session.Act1.IsComplete, "F2 did not play act1");
            Assert(r2.Log == "[DebugJump] pretoria → toronto | era=02_young_adult_90s", "F2 log exact: " + r2.Log);

            var r3 = DebugLocationJumpMap.TryJump(session.Travel, 3);
            Assert(r3.Moved, "F3 moved");
            Assert(r3.ToLocationId == PrototypeContent.LocationPaloAlto, "F3 palo");
            Assert(r3.EraFolder == "03_early_2000s", "F3 era");
            Assert(eraFromEvent == "03_early_2000s", "LocationChanged era F3");
            Assert(!session.Act1.IsComplete, "F3 did not play act1");
            Assert(r3.Log == "[DebugJump] toronto → palo_alto | era=03_early_2000s", "F3 log exact: " + r3.Log);
        }

        private static void TestDebugJumpF4F5Missing()
        {
            var registry = PrototypeContent.CreateLocations();
            var f4 = DebugLocationJumpMap.ForKey(4, registry);
            var f5 = DebugLocationJumpMap.ForKey(5, registry);
            Assert(!f4.PlaceExists, "F4 modern not a registered place");
            Assert(f4.EraFolder == "04_modern", "F4 era target");
            Assert(!f5.PlaceExists, "F5 mars not a registered place");
            Assert(f5.EraFolder == "05_mars", "F5 era target");

            var travel = new TravelService();
            travel.RegisterLocations(registry);
            travel.SetStartingLocation(PrototypeContent.LocationPretoria);

            var r4 = DebugLocationJumpMap.TryJump(travel, 4);
            Assert(r4.PlaceMissing, "F4 missing as place");
            Assert(!r4.Moved, "F4 did not travel");
            Assert(travel.CurrentLocationId == PrototypeContent.LocationPretoria, "F4 stayed");
            Assert(r4.Log.Contains("[DebugJump]") && r4.Log.Contains("era=04_modern") && r4.Log.Contains("missing as place"),
                "F4 log: " + r4.Log);

            var r5 = DebugLocationJumpMap.TryJump(travel, 5);
            Assert(r5.PlaceMissing, "F5 missing as place");
            Assert(!r5.Moved, "F5 did not travel");
            Assert(travel.CurrentLocationId == PrototypeContent.LocationPretoria, "F5 stayed");
            Assert(r5.Log.Contains("era=05_mars") && r5.Log.Contains("missing as place"), "F5 log: " + r5.Log);
        }

        private static void TestHudPanelExclusivity()
        {
            var open = HudLargePanel.None;

            open = HudPanelExclusivity.Toggle(open, HudLargePanel.Inbox);
            Assert(HudPanelExclusivity.IsOpen(open, HudLargePanel.Inbox), "open Inbox");
            Assert(!HudPanelExclusivity.IsMap(open), "map closed when inbox open");
            Assert(!HudPanelExclusivity.IsCompanies(open), "companies closed");
            Assert(!HudPanelExclusivity.IsResolve(open), "resolve closed");

            open = HudPanelExclusivity.Toggle(open, HudLargePanel.Map);
            Assert(HudPanelExclusivity.IsMap(open), "open Map closes Inbox");
            Assert(!HudPanelExclusivity.IsInbox(open), "inbox closed after map");
            Assert(!HudPanelExclusivity.IsCompanies(open), "companies still closed");

            open = HudPanelExclusivity.Toggle(open, HudLargePanel.Inbox);
            Assert(HudPanelExclusivity.IsInbox(open), "inbox from map");

            open = HudPanelExclusivity.Toggle(open, HudLargePanel.Inbox);
            Assert(open == HudLargePanel.None, "toggle Inbox again closes");
            Assert(!HudPanelExclusivity.IsInbox(open), "inbox closed");

            open = HudPanelExclusivity.Toggle(open, HudLargePanel.Companies);
            Assert(HudPanelExclusivity.IsCompanies(open), "companies open");
            open = HudPanelExclusivity.Close();
            Assert(open == HudLargePanel.None, "Close → none");

            open = HudPanelExclusivity.Open(open, HudLargePanel.Companies);
            Assert(HudPanelExclusivity.IsCompanies(open), "open Companies");
            open = HudPanelExclusivity.Open(open, HudLargePanel.Resolve);
            Assert(HudPanelExclusivity.IsResolve(open), "open Resolve replaces Companies");
            Assert(!HudPanelExclusivity.IsCompanies(open), "companies closed by resolve");

            open = HudPanelExclusivity.Open(HudLargePanel.None, HudLargePanel.Inbox);
            open = HudPanelExclusivity.OnDialogueOrStory(open);
            Assert(open == HudLargePanel.None, "dialogue closes Inbox");

            open = HudPanelExclusivity.Open(HudLargePanel.None, HudLargePanel.Map);
            open = HudPanelExclusivity.OnDialogueOrStory(open);
            Assert(open == HudLargePanel.None, "story closes Map");

            open = HudPanelExclusivity.Open(HudLargePanel.None, HudLargePanel.Companies);
            open = HudPanelExclusivity.OnDialogueOrStory(open);
            Assert(open == HudLargePanel.None, "dialogue closes Companies");
        }

        private static void TestHudPanelMenuAndStory()
        {
            var open = HudLargePanel.None;

            open = HudPanelExclusivity.Open(open, HudLargePanel.Menu);
            Assert(HudPanelExclusivity.IsMenu(open), "menu open");
            Assert(!HudPanelExclusivity.IsInbox(open), "inbox closed while menu open");
            Assert(!HudPanelExclusivity.IsStory(open), "story closed while menu open");

            open = HudPanelExclusivity.Open(open, HudLargePanel.Inbox);
            Assert(HudPanelExclusivity.IsInbox(open), "opening Inbox from Menu replaces Menu");
            Assert(!HudPanelExclusivity.IsMenu(open), "menu replaced by inbox");

            open = HudPanelExclusivity.Open(open, HudLargePanel.Menu);
            open = HudPanelExclusivity.Open(open, HudLargePanel.Story);
            Assert(HudPanelExclusivity.IsStory(open), "opening Story from Menu replaces Menu");
            Assert(!HudPanelExclusivity.IsMenu(open), "menu replaced by story");

            open = HudPanelExclusivity.Toggle(HudLargePanel.None, HudLargePanel.Menu);
            Assert(HudPanelExclusivity.IsMenu(open), "toggle opens Menu");
            open = HudPanelExclusivity.Toggle(open, HudLargePanel.Menu);
            Assert(open == HudLargePanel.None, "toggle Menu closes");

            open = HudPanelExclusivity.Open(HudLargePanel.None, HudLargePanel.Menu);
            open = HudPanelExclusivity.OnDialogueOrStory(open);
            Assert(open == HudLargePanel.None, "dialogue clears Menu");

            open = HudPanelExclusivity.Open(HudLargePanel.None, HudLargePanel.Story);
            open = HudPanelExclusivity.OnDialogueOrStory(open);
            Assert(open == HudLargePanel.None, "dialogue clears Story");

            open = HudPanelExclusivity.Open(HudLargePanel.Inbox, HudLargePanel.Story);
            Assert(HudPanelExclusivity.IsStory(open), "story replaces inbox");
            Assert(!HudPanelExclusivity.IsInbox(open), "inbox closed by story");
        }

        private static void TestUiStyleTokens()
        {
            Assert(UiStyleTokens.TitleFontSize > UiStyleTokens.PrimaryButtonFontSize,
                "title larger than primary button");
            Assert(UiStyleTokens.PrimaryButtonFontSize > UiStyleTokens.CaptionFontSize,
                "primary larger than caption");
            Assert(UiStyleTokens.PrimaryButtonHeight > UiStyleTokens.SecondaryButtonHeight,
                "primary button taller than secondary");
            Assert(UiStyleTokens.PrimaryButtonHeight > UiStyleTokens.TopBarButtonHeight,
                "primary taller than top-bar");
            Assert(UiStyleTokens.PrimaryR != UiStyleTokens.SecondaryR ||
                   UiStyleTokens.PrimaryG != UiStyleTokens.SecondaryG,
                "primary color distinct from secondary");
            Assert(UiStyleTokens.GameTitle.IndexOf("Elon", StringComparison.Ordinal) >= 0, "title names the game");
            Assert(UiStyleTokens.GameSubtitle.IndexOf("Pretoria", StringComparison.Ordinal) >= 0, "subtitle pret");
            Assert(UiStyleTokens.GameSubtitle.IndexOf("Mars", StringComparison.Ordinal) >= 0, "subtitle mars");
            Assert(UiStyleTokens.TopBarHeight > UiStyleTokens.TopBarButtonHeight, "top bar fits buttons");
            Assert(UiStyleTokens.ReferenceWidth == 1280 && UiStyleTokens.ReferenceHeight == 720,
                "reference resolution");
            Assert(UiStyleTokens.DisclaimerLabel.IndexOf("Not affiliated", StringComparison.Ordinal) >= 0,
                "disclaimer present");
            Assert(UiStyleTokens.TopBarScreenPadding >= 12, "screen padding");
            Assert(UiStyleTokens.TopBarHeight <= 52, "top bar not thick");
            Assert(UiStyleTokens.ActiveNavG != UiStyleTokens.SecondaryG, "active nav color distinct");
            Assert(UiStyleTokens.HasTypeScale(), "type scale 12/14/16/22/36");
            Assert(UiStyleTokens.PaddingInRange(), "padding 16-24");
            Assert(UiStyleTokens.PanelMotionInRange(), "motion 120-180ms");
            Assert(UiStyleTokens.OverlayDimInRange(), "resolve dim 70-80%");
            Assert(UiStyleTokens.PanelIsNearBlack(), "panel near-black");
            Assert(UiStyleTokens.PrimaryIsNotUnityBlue(), "not Unity default blue");
            Assert(UiStyleTokens.UsesPointFilter(), "point filter documented");
        }

        private static void TestAaaThemeTokens()
        {
            Assert(UiStyleTokens.CaptionFontSize == 12, "12");
            Assert(UiStyleTokens.BodyFontSize == 14, "14");
            Assert(UiStyleTokens.UiFontSize == 16, "16");
            Assert(UiStyleTokens.PanelTitleFontSize == 22, "22");
            Assert(UiStyleTokens.TitleFontSize == 36, "36");
            Assert(UiStyleTokens.DangerR > UiStyleTokens.DangerG, "danger reads red");
            Assert(UiStyleTokens.DisabledA < 1f, "disabled is faded");
            Assert(UiStyleTokens.PanelBorderR > 0.3f, "highlight edge exists");
            Assert(UiStyleTokens.AccentR > UiStyleTokens.AccentB, "accent is warm brass, not blue");
            Assert(UiStyleTokens.PanelMotionSeconds >= 0.12f && UiStyleTokens.PanelMotionSeconds <= 0.18f,
                "fade " + UiStyleTokens.PanelMotionSeconds);
            Assert(UiStyleTokens.OverlayA >= 0.70f && UiStyleTokens.OverlayA <= 0.80f,
                "overlay " + UiStyleTokens.OverlayA);
            Assert(UiStyleTokens.HoverScale > 1f, "hover scale");
            Assert(UiStyleTokens.SpriteFilterModePoint == 0, "Point = 0");
            Assert(UiStyleTokens.SpriteFilterModeBilinear == 1, "Bilinear = 1");
            Assert(!UiStyleTokens.IsUnityDefaultButtonBlue(UiStyleTokens.PrimaryR, UiStyleTokens.PrimaryG, UiStyleTokens.PrimaryB),
                "primary fill is not (0.26,0.52,0.96)");
        }

        private static void TestTitleScreenCopy()
        {
            Assert(TitleScreenCopy.Tagline == "From Pretoria to Mars", "tagline");
            Assert(TitleScreenCopy.PrimaryCta == "New Game", "primary CTA");
            Assert(TitleScreenCopy.SecondaryCta == "Quit", "secondary CTA");
            Assert(TitleScreenCopy.IsValidTitleScreen(), "valid title screen");
            Assert(!TitleScreenCopy.IsPlaceholder(TitleScreenCopy.Title), "title not placeholder");
            Assert(!TitleScreenCopy.IsPlaceholder(TitleScreenCopy.Tagline), "tagline not placeholder");
        }

        private static void TestWorldHorizonLine()
        {
            Assert(WorldBackdropTokens.HasHorizonLine(), "horizon line token");
            Assert(WorldBackdropTokens.HorizonLineName == "HorizonLine", "line name");
            Assert(WorldBackdropTokens.VignetteName == "Vignette", "vignette name");
            var p = WorldBackdropTokens.Pretoria();
            Assert(WorldBackdropTokens.IsDesignedBackdrop(p), "composed pretora");
        }

        private static void TestTopBarLayoutNoClip()
        {
            Assert(TopBarLayout.NavCount == 4, "four nav buttons");
            Assert(TopBarLayout.NavLabels[0] == "Inbox", "full Inbox label, not ibox");
            Assert(TopBarLayout.NavLabels[1] == "Map", "Map");
            Assert(TopBarLayout.NavLabels[2] == "Companies", "Companies");
            Assert(TopBarLayout.NavLabels[3] == "Story", "Story");
            Assert(TopBarLayout.NavLabels[0].IndexOf("ibox", StringComparison.Ordinal) < 0, "label is Inbox");

            var inbox = TopBarLayout.NavButton(0);
            Assert(inbox.X >= TopBarLayout.ScreenPad, "Inbox not flush with screen edge: x=" + inbox.X);
            Assert(inbox.W == TopBarLayout.ButtonWidth, "Inbox width");
            Assert(inbox.H == TopBarLayout.ButtonHeight, "Inbox height");
            Assert(inbox.FullyInside(TopBarLayout.CanvasWidth, TopBarLayout.BarHeight, TopBarLayout.ScreenPad),
                "Inbox fully inside canvas");

            for (int i = 1; i < TopBarLayout.NavCount; i++)
            {
                var a = TopBarLayout.NavButton(i - 1);
                var b = TopBarLayout.NavButton(i);
                Assert(b.H == a.H && b.W == a.W, "same button size " + i);
                Assert(Math.Abs((b.X - a.Right) - TopBarLayout.Gap) < 0.01f, "gap between " + (i - 1) + " and " + i);
                Assert(!a.Overlaps(b), "buttons do not overlap");
                Assert(b.FullyInside(TopBarLayout.CanvasWidth, TopBarLayout.BarHeight, TopBarLayout.ScreenPad),
                    "button " + TopBarLayout.NavLabels[i] + " fully visible");
            }

            Assert(TopBarLayout.AllNavButtonsFullyVisible(), "all nav + status visible");
            Assert(TopBarLayout.StatusClearsNav(), "status does not cover Story");
            var status = TopBarLayout.StatusCluster();
            Assert(status.Right <= TopBarLayout.CanvasWidth - TopBarLayout.ScreenPad + 0.01f, "status padding");
        }

        private static void TestHudStatusCopy()
        {
            Assert(HudStatusCopy.ActLine(1, "Pretoria") == "Act 1: Pretoria", "act line");
            Assert(HudStatusCopy.ActLineForLocation(PrototypeContent.LocationPretoria) == "Act 1: Pretoria",
                "pretoria act");
            Assert(HudStatusCopy.LocationLine("Pretoria, South Africa") == "Pretoria, South Africa", "location");
            Assert(HudStatusCopy.ActLineForLocation(PrototypeContent.LocationPretoria)
                    .IndexOf("·", StringComparison.Ordinal) < 0,
                "top-bar act is not a debug list");
        }

        private static void TestWorldBackdropPretoria()
        {
            var p = WorldBackdropTokens.ForLocation(PrototypeContent.LocationPretoria);
            Assert(WorldBackdropTokens.IsDesignedBackdrop(p), "pretoria backdrop designed");
            Assert(!WorldBackdropTokens.LooksLikeEditorGray(p.SkyR, p.SkyG, p.SkyB), "sky not editor gray");
            Assert(!WorldBackdropTokens.LooksLikeEditorGray(p.GroundR, p.GroundG, p.GroundB), "ground not editor gray");
            Assert(p.SkyB > p.SkyR, "sky is blue-ish dusk");
            Assert(p.GroundR > p.GroundB, "ground is warm earth");
            Assert(p.SkyLuma < p.GroundLuma || p.SkyLuma < 0.18f, "sky stays dark");
            Assert(p.GroundTop < 0f, "floor under the actor so the body reads against sky");
            Assert(WorldBackdropTokens.BackdropRootName == "WorldBackdrop", "root name");

            var toronto = WorldBackdropTokens.ForLocation(PrototypeContent.LocationToronto);
            Assert(WorldBackdropTokens.IsDesignedBackdrop(toronto), "toronto designed");
        }

        private static void TestHudNavHighlightAndSheetClose()
        {
            Assert(HudNavHighlight.ActiveIndex(HudLargePanel.Inbox) == 0, "inbox index");
            Assert(HudNavHighlight.ActiveIndex(HudLargePanel.Map) == 1, "map index");
            Assert(HudNavHighlight.ActiveIndex(HudLargePanel.None) == -1, "none unlit");
            Assert(HudNavHighlight.ActiveIndex(HudLargePanel.Menu) == -1, "menu is not a nav");
            Assert(HudNavHighlight.IsActive(HudLargePanel.Inbox, HudLargePanel.Inbox), "inbox lit");
            Assert(!HudNavHighlight.IsActive(HudLargePanel.Inbox, HudLargePanel.Map), "map unlit when inbox");
            Assert(HudPanelExclusivity.OnSheetClose(HudLargePanel.Inbox) == HudLargePanel.None,
                "Close returns to world");
            Assert(HudPanelExclusivity.OnSheetClose(HudLargePanel.Story) == HudLargePanel.None,
                "Story Close returns to world");
        }

        private static void TestDialogueStripBelowTopBar()
        {
            Assert(!DialogueStripLayout.OverlapsTopBar(), "dialogue below top bar");
            Assert(DialogueStripLayout.AnchorMaxY < DialogueStripLayout.TopBarBottomNormalized(),
                "strip max y under bar");
            Assert(DialogueStripLayout.AnchorMinY == 0f, "strip sits on the bottom");
        }

        private static void TestHudSourceWiresLayout()
        {
            string root = FindRepoRoot();
            string hud = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Unity", "UI", "GameplayHudBuilder.cs"));
            Assert(hud.IndexOf("CreateTopBarNav", StringComparison.Ordinal) >= 0, "builder creates top-bar nav");
            Assert(hud.IndexOf("NavInbox", StringComparison.Ordinal) >= 0, "NavInbox exists");
            Assert(hud.IndexOf("TopBarLayout.NavButton", StringComparison.Ordinal) >= 0, "builder uses TopBarLayout");
            Assert(hud.IndexOf("TopBarLayout.NavLabels", StringComparison.Ordinal) >= 0, "labels come from TopBarLayout");
            Assert(hud.IndexOf("HudStatusCopy.ActLineForLocation", StringComparison.Ordinal) >= 0, "act copy wired");
            Assert(hud.IndexOf("DialogueStripLayout", StringComparison.Ordinal) >= 0, "dialogue strip tokens");
            Assert(hud.IndexOf("HUD_Canvas", StringComparison.Ordinal) >= 0, "single HUD canvas name");

            string setup = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Unity", "Bootstrap", "GameplaySceneSetup.cs"));
            Assert(setup.IndexOf("WorldBackdropTokens.ForLocation", StringComparison.Ordinal) >= 0,
                "scene setup uses backdrop tokens");
            Assert(setup.IndexOf("WorldBackdropTokens.BackdropRootName", StringComparison.Ordinal) >= 0,
                "named WorldBackdrop root");

            string inbox = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Unity", "UI", "InboxUI.cs"));
            Assert(inbox.IndexOf("hud.Close()", StringComparison.Ordinal) >= 0, "Inbox Close dismisses");
            Assert(inbox.IndexOf("hud.Open(HudLargePanel.Menu)", StringComparison.Ordinal) < 0,
                "Inbox Close does not dump into Esc menu");

            string menu = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Unity", "Bootstrap", "MainMenuSceneSetup.cs"));
            Assert(menu.IndexOf("TitleScreenCopy.Title", StringComparison.Ordinal) >= 0, "title wired");
            Assert(menu.IndexOf("TitleScreenCopy.Tagline", StringComparison.Ordinal) >= 0, "tagline wired");
            Assert(menu.IndexOf("NewGameButton", StringComparison.Ordinal) >= 0, "New Game button");
            Assert(menu.IndexOf("QuitButton", StringComparison.Ordinal) >= 0, "Quit button");
            Assert(menu.IndexOf("MenuSky", StringComparison.Ordinal) >= 0, "composed sky");
            Assert(menu.IndexOf("MenuGround", StringComparison.Ordinal) >= 0, "composed ground");
            Assert(menu.IndexOf("\"PLACEHOLDER\"", StringComparison.Ordinal) < 0, "no PLACEHOLDER copy");

            string hudCtrl = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Unity", "UI", "HudPanelController.cs"));
            Assert(hudCtrl.IndexOf("PanelMotionSeconds", StringComparison.Ordinal) >= 0, "sheets fade with token duration");
            Assert(hudCtrl.IndexOf("IEnumerator Transition", StringComparison.Ordinal) >= 0, "no pop-in");

            string hover = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Unity", "UI", "UiHoverAffordance.cs"));
            Assert(hover.IndexOf("HoverScale", StringComparison.Ordinal) >= 0, "hover scale");

            Assert(setup.IndexOf("HorizonLineName", StringComparison.Ordinal) >= 0, "horizon line in world");
            Assert(setup.IndexOf("VignetteName", StringComparison.Ordinal) >= 0, "vignette in world");
            Assert(setup.IndexOf("DebugLocationJump.Ensure", StringComparison.Ordinal) >= 0, "F1-F5 still ensured");
            Assert(setup.IndexOf("FilterMode.Point", StringComparison.Ordinal) >= 0
                   || File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Unity", "Characters", "ElonSpriteCatalog.cs"))
                       .IndexOf("FilterMode.Point", StringComparison.Ordinal) >= 0,
                "point filter on sprites");
        }

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var hud = Path.Combine(dir.FullName, "Assets", "Scripts", "Unity", "UI", "GameplayHudBuilder.cs");
                if (File.Exists(hud))
                    return dir.FullName;
                dir = dir.Parent;
            }

            throw new InvalidOperationException("repo root with GameplayHudBuilder.cs not found from " + AppContext.BaseDirectory);
        }
    }
}
