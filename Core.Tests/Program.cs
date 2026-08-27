using System;
using System.Collections.Generic;
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
            Run("Act1_NineBeats_ChoicesTagsInboxFacts", TestAct1ShippedStoryFacts);

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
            string[] required =
            {
                "act1_home_choice",
                "act1_encyclopedia",
                "act1_vic20_night",
                "act1_blastar",
                "act1_bryanston_stairs",
                "act1_boys_high",
                "act1_garden_rocket",
                "act1_world_outside",
                "act1_exit_plan"
            };
            Assert(Act1Progression.NamedEventIds.Count == 9, "nine named beats");
            Assert(Act1Progression.OrderedBeats.Count == 9, "nine ordered beats");
            Assert(Act1Progression.NamedEventIds.SequenceEqual(required), "shipped ids match the Act 1 chain");
            Assert(Act1Progression.DialogueId(Act1Progression.Beat.BryanstonStairs) == "act1_bryanston_stairs",
                "stairs id");
            Assert(Act1Progression.DialogueId(Act1Progression.Beat.BoysHigh) == "act1_boys_high", "boys high id");

            var act1 = new Act1Progression();
            act1.Begin();
            for (int i = 0; i < Act1Progression.OrderedBeats.Count; i++)
            {
                var beat = Act1Progression.OrderedBeats[i];
                Assert(act1.CurrentBeat == beat, "beat " + beat);
                var dialogue = act1.GetCurrentDialogue();
                Assert(dialogue != null, "dialogue " + beat);
                Assert(dialogue.Id == Act1Progression.DialogueId(beat), "id matches DialogueId");
                Assert(dialogue.Id == Act1Progression.NamedEventIds[i], "id matches NamedEventIds");
                Assert(act1.Advance(), "advance from " + beat);
            }

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

        private static void TestAct1ShippedStoryFacts()
        {
            foreach (var beat in Act1Progression.OrderedBeats)
            {
                var d = Act1Content.GetDialogueForBeat(beat);
                Assert(d != null, "dialogue for " + beat);
                Assert(d.Id == Act1Progression.DialogueId(beat), "id " + beat);
                var choices = Act1Content.StartChoices(d);
                Assert(choices.Count >= 2 && choices.Count <= 3, beat + " has 2-3 choices, got " + choices.Count);
                Assert(Act1Content.StartChoicesHaveDistinctEffects(d), beat + " choices differ in tags/stats");
                foreach (var c in choices)
                    Assert(!string.IsNullOrWhiteSpace(c.Text), beat + " choice text");
            }

            var enc = Act1Content.GetDialogueForBeat(Act1Progression.Beat.Encyclopedia);
            Assert(!Act1Content.LooksLikeQuiz(enc), "encyclopedia is not a quiz");

            var vic = Act1Content.GetDialogueForBeat(Act1Progression.Beat.Vic20Night);
            var vicText = Act1Content.FlattenText(vic);
            Assert(vicText.IndexOf("VIC-20", StringComparison.OrdinalIgnoreCase) >= 0, "VIC-20 named");
            Assert(vicText.IndexOf("BASIC", StringComparison.OrdinalIgnoreCase) >= 0, "BASIC manual");
            Assert(vicText.IndexOf("days", StringComparison.OrdinalIgnoreCase) >= 0, "manual in days");
            Assert(vicText.IndexOf("six months", StringComparison.OrdinalIgnoreCase) >= 0, "mentions the book's six-month claim");
            Assert(vicText.IndexOf("six months of lessons", StringComparison.OrdinalIgnoreCase) < 0
                   || vicText.IndexOf("dare", StringComparison.OrdinalIgnoreCase) >= 0
                   || vicText.IndexOf("days", StringComparison.OrdinalIgnoreCase) >= 0,
                "six months is the manual's claim, not the time spent");

            var stairs = Act1Content.GetDialogueForBeat(Act1Progression.Beat.BryanstonStairs);
            var stairsStart = stairs.Nodes[stairs.StartNodeId];
            var stairsStartText = string.Join(" ", stairsStart.Lines.Select(l => l.Text));
            Assert(stairsStartText.IndexOf("Bryanston", StringComparison.Ordinal) >= 0, "stairs at Bryanston");
            Assert(stairsStartText.IndexOf("Boys High", StringComparison.Ordinal) < 0,
                "start node does not place the staircase at Boys High");
            foreach (var c in Act1Content.StartChoices(stairs))
            {
                var low = c.Text.ToLowerInvariant();
                Assert(low.IndexOf("win the fight") < 0, "no fight-win choice");
                Assert(low.IndexOf("qte") < 0, "no QTE");
                Assert(low.IndexOf("beat them") < 0, "no beating");
            }
            var stairsAll = Act1Content.FlattenText(stairs).ToLowerInvariant();
            Assert(stairsAll.IndexOf("hospital") >= 0, "hospital after stairs");
            Assert(stairsAll.IndexOf("boys high") >= 0, "move to Boys High after");

            var blastar = Act1Content.GetDialogueForBeat(Act1Progression.Beat.Blastar);
            var blastarText = Act1Content.FlattenText(blastar);
            Assert(blastarText.IndexOf("500", StringComparison.Ordinal) >= 0, "Blastar ~500");
            Assert(blastarText.IndexOf("magazine", StringComparison.OrdinalIgnoreCase) >= 0, "magazine sale");
            foreach (var c in Act1Content.StartChoices(blastar))
                Assert(c.MoneyDelta == 500, "sale credits 500");

            var clippings = Act1Content.CreateInboxForCompletedBeat(Act1Progression.Beat.Blastar);
            Assert(clippings.Count >= 1, "blastar clipping factory");
            Assert(clippings.Any(t => t.Id == "act1_clipping_blastar"), "magazine clipping id");
            Assert(clippings[0].CompanyDisplayName.IndexOf("Clipping", StringComparison.OrdinalIgnoreCase) >= 0,
                "clipping, not company mail");
            Assert(clippings[0].Description.IndexOf("500", StringComparison.Ordinal) >= 0, "clipping names 500");
            Assert(clippings[0].CompanyId != CompanyContent.Zip2 && clippings[0].CompanyId != CompanyContent.XCom,
                "not company mail");

            var session = new GameSession();
            session.StartNewGame();
            PlayCurrentBeatChoice(session, 0);
            Assert(session.Act1.HasTag(Act1Progression.TagEncyclopedia), "encyclopedia unlocked via Choose");
            session.AdvanceAct1Beat();

            PlayCurrentBeatChoice(session, 0);
            session.AdvanceAct1Beat();

            PlayCurrentBeatChoice(session, 0);
            Assert(session.Act1.HasTag(Act1Progression.TagProgramming), "programming unlocked");
            session.AdvanceAct1Beat();

            var moneyBefore = session.Act1.Money;
            PlayCurrentBeatChoice(session, 0);
            Assert(session.Act1.Money == moneyBefore + 500, "Blastar money via Dialogue.Choose");
            session.AdvanceAct1Beat();
            Assert(session.Inbox.ListTickets().Any(t => t.Id == "act1_clipping_blastar"),
                "session delivers magazine clipping");

            while (session.Act1.CurrentBeat != Act1Progression.Beat.GardenRocket)
                Assert(session.AdvanceAct1Beat(), "advance to rocket");
            PlayCurrentBeatChoice(session, 0);
            Assert(session.Act1.HasTag(Act1Progression.TagPhysics), "physics tag");

            var focusA = ApplyChoiceOnFreshBeat(Act1Progression.Beat.HomeChoice, 0);
            var focusB = ApplyChoiceOnFreshBeat(Act1Progression.Beat.HomeChoice, 1);
            Assert(focusA.Focus != focusB.Focus || focusA.ExitPlan != focusB.ExitPlan
                   || focusA.ThickSkin != focusB.ThickSkin,
                "home choices apply distinct stats");

            while (!session.Act1.IsComplete)
                Assert(session.AdvanceAct1Beat(), "finish act1");
            Assert(session.Travel.IsUnlocked(PrototypeContent.LocationToronto), "toronto after last beat");
            Assert(session.Act1.Zip2FoundingUnlocked, "zip2 founding");
            Assert(session.Inbox.ListTickets().Any(t => t.Id == "act1_travel_canada"), "canada ticket");

            foreach (var t in session.Inbox.ListTickets())
            {
                if (t.Id == "act1_travel_canada")
                    continue;
                Assert(t.CompanyId != CompanyContent.Zip2 && t.CompanyId != CompanyContent.XCom,
                    t.Id + " is not company mail");
            }
        }

        private static void PlayCurrentBeatChoice(GameSession session, int choiceIndex)
        {
            var d = session.GetAct1Dialogue();
            Assert(d != null, "current dialogue");
            Assert(session.Dialogue.Start(d), "start dialogue");
            int guard = 0;
            while (session.Dialogue.IsActive && !session.Dialogue.IsAwaitingChoice && guard++ < 20)
                session.Dialogue.Advance();
            Assert(session.Dialogue.IsAwaitingChoice, "awaiting choice on " + d.Id);
            Assert(session.Dialogue.Choose(choiceIndex), "choose " + choiceIndex);
        }

        private static Act1Progression ApplyChoiceOnFreshBeat(Act1Progression.Beat beat, int choiceIndex)
        {
            var act1 = new Act1Progression();
            var d = Act1Content.GetDialogueForBeat(beat);
            var choice = Act1Content.StartChoices(d)[choiceIndex];
            act1.ApplyChoice(choice);
            return act1;
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
            Assert(session.Act1.CurrentBeat == Act1Progression.FirstBeat, "act1 start");

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
            while (!session.Act1.IsComplete && guard++ < 30)
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
    }
}
