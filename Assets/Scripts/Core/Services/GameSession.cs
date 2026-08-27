using System;
using System.Collections.Generic;
using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Facade over Inbox + Travel + Dialogue + Companies + Act1 for a play session.
    /// Pure C# — GameBootstrap (Unity) creates one instance and wires UI to it.
    ///
    /// Progression: Act1 beats → unlock Canada → travel → Found Zip2 → Zip2 tickets →
    /// sell Zip2 may unlock X.com founding → X.com tickets.
    /// </summary>
    public sealed class GameSession
    {
        public InboxService Inbox { get; }
        public TravelService Travel { get; }
        public DialogueRunner Dialogue { get; }
        public CompanyManager Companies { get; }
        public Act1Progression Act1 { get; }

        public string ActiveTicketId { get; private set; }
        public bool HasStarted { get; private set; }
        public bool Zip2TicketsDelivered { get; private set; }
        public bool XComTicketsDelivered { get; private set; }
        public string LastResolutionNarration { get; private set; }

        public event Action SessionStarted;
        public event Action<InboxTicket> TicketAccepted;
        public event Action TravelCompleted;
        public event Action Act1Completed;
        public event Action<string> CompanyFounded;
        public event Action<string, ProblemChoice> ProblemResolved;

        public GameSession()
            : this(new InboxService(), new TravelService(), new DialogueRunner(),
                new CompanyManager(), new Act1Progression())
        {
        }

        public GameSession(
            InboxService inbox,
            TravelService travel,
            DialogueRunner dialogue,
            CompanyManager companies,
            Act1Progression act1)
        {
            Inbox = inbox ?? throw new ArgumentNullException(nameof(inbox));
            Travel = travel ?? throw new ArgumentNullException(nameof(travel));
            Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
            Companies = companies ?? throw new ArgumentNullException(nameof(companies));
            Act1 = act1 ?? throw new ArgumentNullException(nameof(act1));
            Dialogue.ChoiceMade += OnDialogueChoice;
        }

        private void OnDialogueChoice(DialogueChoice choice)
        {
            Act1.ApplyChoice(choice);
        }

        /// <summary>
        /// Starts Act 1: locations, companies (not founded), Pretoria unlocked, Act1 at Home intro.
        /// No Zip2 tickets until Zip2 is founded after Act 1.
        /// </summary>
        public void StartNewGame()
        {
            Inbox.Clear();
            Companies.Clear();
            Act1.Reset();
            ActiveTicketId = null;
            Zip2TicketsDelivered = false;
            XComTicketsDelivered = false;
            LastResolutionNarration = null;

            Travel.RegisterLocations(PrototypeContent.CreateLocations());
            Travel.SetStartingLocation(PrototypeContent.LocationPretoria);
            // Canada / Palo Alto stay locked until progression unlocks them.

            Companies.RegisterMany(CompanyContent.CreateStartingCompanies());
            Act1.Begin();

            HasStarted = true;
            SessionStarted?.Invoke();
        }

        /// <summary>Dialogue for the current Act 1 beat (or null if complete).</summary>
        public DialogueDefinition GetAct1Dialogue()
        {
            return Act1.GetCurrentDialogue();
        }

        /// <summary>
        /// After the player finishes the current Act 1 beat dialogue, call this.
        /// Completing the final beat unlocks Canada (and Zip2 founding path).
        /// </summary>
        public bool AdvanceAct1Beat()
        {
            if (Act1.IsComplete)
                return false;

            var finished = Act1.CurrentBeat;
            var ok = Act1.Advance();
            if (ok)
                DeliverAct1Inbox(finished);
            if (Act1.IsComplete)
                OnAct1Complete();
            return ok;
        }

        private void DeliverAct1Inbox(Act1Progression.Beat finishedBeat)
        {
            foreach (var ticket in Act1Content.CreateInboxForCompletedBeat(finishedBeat))
                Inbox.ReceiveTicket(ticket);
        }

        private void OnAct1Complete()
        {
            Travel.Unlock(PrototypeContent.LocationToronto);
            ArchiveAct1Clippings();
            // Seed a guidance ticket to travel — a letter, not a company problem.
            Inbox.ReceiveTicket(new InboxTicket(
                id: "act1_travel_canada",
                companyId: "personal",
                companyDisplayName: "Personal Journey",
                locationId: PrototypeContent.LocationToronto,
                locationDisplayName: "Toronto, Canada",
                title: "North to Canada",
                description:
                    "A letter, not a company memo. The passport worked. Canada is unlocked. " +
                    "Travel to Toronto. Zip2 waits until you found it from Companies.",
                difficulty: 1,
                rewardDescription: "Arrive in Canada · Found Zip2 available"));
            Act1Completed?.Invoke();
        }

        /// <summary>Act 1 notes/clippings stay readable as completed; they must not hide Zip2 problems.</summary>
        private void ArchiveAct1Clippings()
        {
            var ids = new List<string>();
            foreach (var t in Inbox.ListTickets())
            {
                if (t.Id == "act1_travel_canada")
                    continue;
                if (TryGetProblem(t.Id, out _))
                    continue;
                ids.Add(t.Id);
            }

            foreach (var id in ids)
            {
                if (!Inbox.TryGet(id, out var t))
                    continue;
                if (t.Status == TicketStatus.Pending)
                    Inbox.AcceptTicket(id);
                Inbox.CompleteTicket(id);
            }
        }

        /// <summary>
        /// Found Zip2 (requires Act1 complete / Zip2FoundingUnlocked).
        /// Delivers Zip2-relevant inbox problems through the shared Inbox service.
        /// </summary>
        public bool FoundZip2()
        {
            if (!Act1.Zip2FoundingUnlocked && !Act1.IsComplete)
                return false;
            if (!Companies.Found(CompanyContent.Zip2, startingMoney: 25))
                return false;

            Travel.Unlock(PrototypeContent.LocationPaloAlto);
            DeliverZip2Tickets();
            CompanyFounded?.Invoke(CompanyContent.Zip2);
            return true;
        }

        /// <summary>
        /// Found X.com after Zip2 is Sold (or force if already allowed). Seeds X.com tickets.
        /// </summary>
        public bool FoundXCom()
        {
            if (!Companies.TryGet(CompanyContent.Zip2, out var zip2))
                return false;
            // Historically capital from Zip2 sale funds X.com — require Zip2 sold OR already founded path.
            if (zip2.Status != CompanyStatus.Sold && zip2.Status != CompanyStatus.Merged)
            {
                // Allow founding if Zip2 sold path not taken but player has high money from other outcomes?
                // Stick to sale unlock for clean progression.
                return false;
            }

            if (!Companies.Found(CompanyContent.XCom, startingMoney: 80))
                return false;

            Travel.Unlock(PrototypeContent.LocationPaloAlto);
            DeliverXComTickets();
            CompanyFounded?.Invoke(CompanyContent.XCom);
            return true;
        }

        /// <summary>Idempotent delivery of Zip2 problem tickets into the Inbox.</summary>
        public int DeliverZip2Tickets()
        {
            if (Zip2TicketsDelivered)
                return 0;
            if (!Companies.TryGet(CompanyContent.Zip2, out var z) || z.Status == CompanyStatus.NotFounded)
                return 0;

            int n = 0;
            foreach (var problem in CompanyProblemsContent.CreateZip2Problems())
            {
                if (Inbox.ReceiveTicket(problem.ToTicket()))
                    n++;
            }
            Zip2TicketsDelivered = true;
            return n;
        }

        public int DeliverXComTickets()
        {
            if (XComTicketsDelivered)
                return 0;
            if (!Companies.TryGet(CompanyContent.XCom, out var x) || x.Status == CompanyStatus.NotFounded)
                return 0;

            int n = 0;
            foreach (var problem in CompanyProblemsContent.CreateXComProblems())
            {
                if (Inbox.ReceiveTicket(problem.ToTicket()))
                    n++;
            }
            XComTicketsDelivered = true;
            return n;
        }

        public bool TryGetProblem(string problemId, out ProblemDefinition problem)
        {
            return CompanyProblemsContent.TryGet(problemId, out problem);
        }

        /// <summary>
        /// Accepts a pending inbox ticket as the active mission.
        /// </summary>
        public bool AcceptTicket(string ticketId)
        {
            if (!Inbox.AcceptTicket(ticketId))
                return false;

            ActiveTicketId = ticketId;
            if (Inbox.TryGet(ticketId, out var ticket))
                TicketAccepted?.Invoke(ticket);
            return true;
        }

        public bool TravelToActiveTicketLocation()
        {
            if (string.IsNullOrEmpty(ActiveTicketId))
                return false;
            if (!Inbox.TryGet(ActiveTicketId, out var ticket))
                return false;

            // Auto-unlock ticket destination if progression already allowed it.
            if (!Travel.IsUnlocked(ticket.LocationId))
                Travel.Unlock(ticket.LocationId);

            if (!Travel.TravelForTicket(ticket))
                return false;

            // Guidance / narrative tickets complete on arrival; company problems stay Accepted for Resolve.
            CompleteGuidanceTicketIfNeeded(ActiveTicketId, ticket.LocationId);

            TravelCompleted?.Invoke();
            return true;
        }

        public bool TravelTo(string locationId)
        {
            if (!Travel.TravelTo(locationId))
                return false;

            // Complete any guidance tickets targeting this location (accepted or still pending).
            CompleteGuidanceTicketsAtLocation(locationId);

            TravelCompleted?.Invoke();
            return true;
        }

        /// <summary>
        /// Completes non-problem (guidance) tickets when the player reaches their location.
        /// Company problems remain open until <see cref="ResolveProblem"/>.
        /// </summary>
        private void CompleteGuidanceTicketIfNeeded(string ticketId, string arrivedLocationId)
        {
            if (string.IsNullOrEmpty(ticketId))
                return;
            if (!Inbox.TryGet(ticketId, out var ticket))
                return;
            if (ticket.LocationId != arrivedLocationId)
                return;
            if (TryGetProblem(ticketId, out _))
                return;
            if (ticket.Status == TicketStatus.Completed || ticket.Status == TicketStatus.Failed)
                return;

            // Accept if still pending so CompleteTicket status rules pass.
            if (ticket.Status == TicketStatus.Pending)
                Inbox.AcceptTicket(ticketId);

            Inbox.CompleteTicket(ticketId);
            if (ActiveTicketId == ticketId)
                ActiveTicketId = null;
        }

        private void CompleteGuidanceTicketsAtLocation(string locationId)
        {
            foreach (var t in Inbox.ListTickets())
            {
                if (t.LocationId != locationId)
                    continue;
                if (TryGetProblem(t.Id, out _))
                    continue;
                if (t.Status == TicketStatus.Completed || t.Status == TicketStatus.Failed)
                    continue;
                CompleteGuidanceTicketIfNeeded(t.Id, locationId);
            }
        }

        /// <summary>
        /// Resolves the active (or specified) problem ticket with a choice index.
        /// Applies company stat deltas, completes the ticket, may unlock X.com after Zip2 sale.
        /// </summary>
        public bool ResolveProblem(string ticketId, int choiceIndex)
        {
            if (string.IsNullOrEmpty(ticketId))
                return false;
            if (!CompanyProblemsContent.TryGet(ticketId, out var problem))
                return false;
            if (choiceIndex < 0 || choiceIndex >= problem.Choices.Count)
                return false;

            // Ticket must exist and be accepted (or pending — allow resolve after accept).
            if (!Inbox.TryGet(ticketId, out var ticket))
                return false;
            if (ticket.Status == TicketStatus.Pending)
            {
                if (!Inbox.AcceptTicket(ticketId))
                    return false;
            }
            else if (ticket.Status != TicketStatus.Accepted && ticket.Status != TicketStatus.InProgress)
            {
                return false;
            }

            var choice = problem.Choices[choiceIndex];
            if (!Companies.ApplyChoice(problem.CompanyId, choice))
                return false;

            LastResolutionNarration = choice.OutcomeNarration;
            Inbox.CompleteTicket(ticketId);
            if (ActiveTicketId == ticketId)
                ActiveTicketId = null;

            // After selling Zip2, X.com can be founded.
            if (problem.CompanyId == CompanyContent.Zip2 &&
                Companies.TryGet(CompanyContent.Zip2, out var zip2) &&
                zip2.Status == CompanyStatus.Sold)
            {
                // Do not auto-found; player founds X.com explicitly. Hint via personal ticket.
                Inbox.ReceiveTicket(new InboxTicket(
                    id: "found_xcom_hint",
                    companyId: "personal",
                    companyDisplayName: "Personal Journey",
                    locationId: PrototypeContent.LocationPaloAlto,
                    locationDisplayName: "Palo Alto / Silicon Valley",
                    title: "What next after Zip2?",
                    description:
                        "Zip2 is sold. Capital and lessons are yours. From the Companies panel you can " +
                        "found X.com and step into online banking and payments.",
                    difficulty: 2,
                    rewardDescription: "Unlock founding X.com"));
            }

            ProblemResolved?.Invoke(ticketId, choice);
            return true;
        }

        public bool CompleteActiveTicket()
        {
            if (string.IsNullOrEmpty(ActiveTicketId))
                return false;
            var ok = Inbox.CompleteTicket(ActiveTicketId);
            if (ok)
                ActiveTicketId = null;
            return ok;
        }

        /// <summary>Whether the player may found Zip2 now.</summary>
        public bool CanFoundZip2()
        {
            if (!Act1.IsComplete && !Act1.Zip2FoundingUnlocked)
                return false;
            if (!Companies.TryGet(CompanyContent.Zip2, out var z))
                return false;
            return z.Status == CompanyStatus.NotFounded;
        }

        public bool CanFoundXCom()
        {
            if (!Companies.TryGet(CompanyContent.Zip2, out var zip2))
                return false;
            if (zip2.Status != CompanyStatus.Sold && zip2.Status != CompanyStatus.Merged)
                return false;
            if (!Companies.TryGet(CompanyContent.XCom, out var x))
                return false;
            return x.Status == CompanyStatus.NotFounded;
        }

        public IReadOnlyList<ProblemDefinition> ListKnownProblems()
        {
            return CompanyProblemsContent.CreateAll();
        }
    }
}
