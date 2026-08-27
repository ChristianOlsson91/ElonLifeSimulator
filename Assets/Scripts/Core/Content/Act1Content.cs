using System.Collections.Generic;
using System.Text;
using ElonLifeSim.Core.Models;
using ElonLifeSim.Core.Services;

namespace ElonLifeSim.Core.Content
{
    /// <summary>
    /// Act 1 Pretoria: documented early-life beats. Tone is dry and respectful.
    /// Inbox items are notes, letters, and clippings — not company mail.
    /// </summary>
    public static class Act1Content
    {
        public static DialogueDefinition GetDialogueForBeat(Act1Progression.Beat beat)
        {
            switch (beat)
            {
                case Act1Progression.Beat.HomeChoice: return HomeChoice();
                case Act1Progression.Beat.Encyclopedia: return Encyclopedia();
                case Act1Progression.Beat.Vic20Night: return Vic20Night();
                case Act1Progression.Beat.Blastar: return Blastar();
                case Act1Progression.Beat.BryanstonStairs: return BryanstonStairs();
                case Act1Progression.Beat.BoysHigh: return BoysHigh();
                case Act1Progression.Beat.GardenRocket: return GardenRocket();
                case Act1Progression.Beat.WorldOutside: return WorldOutside();
                case Act1Progression.Beat.ExitPlan: return ExitPlan();
                default: return null;
            }
        }

        public static IReadOnlyList<DialogueChoice> StartChoices(DialogueDefinition dialogue)
        {
            if (dialogue == null || !dialogue.Nodes.TryGetValue(dialogue.StartNodeId, out var start))
                return new List<DialogueChoice>();
            return start.Choices;
        }

        public static bool StartChoicesHaveDistinctEffects(DialogueDefinition dialogue)
        {
            var choices = StartChoices(dialogue);
            if (choices.Count < 2 || choices.Count > 3)
                return false;
            for (int i = 0; i < choices.Count; i++)
            {
                for (int j = i + 1; j < choices.Count; j++)
                {
                    if (!choices[i].EffectsDifferFrom(choices[j]))
                        return false;
                }
            }
            return true;
        }

        public static string FlattenText(DialogueDefinition dialogue)
        {
            if (dialogue == null)
                return string.Empty;
            var sb = new StringBuilder();
            foreach (var node in dialogue.Nodes.Values)
            {
                foreach (var line in node.Lines)
                {
                    sb.Append(line.Text);
                    sb.Append(' ');
                }
                foreach (var choice in node.Choices)
                {
                    sb.Append(choice.Text);
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }

        public static bool LooksLikeQuiz(DialogueDefinition dialogue)
        {
            var t = FlattenText(dialogue).ToLowerInvariant();
            return t.IndexOf("quiz") >= 0
                   || t.IndexOf("correct answer") >= 0
                   || t.IndexOf("score:") >= 0
                   || t.IndexOf("multiple choice test") >= 0;
        }

        public static IReadOnlyList<InboxTicket> CreateInboxForCompletedBeat(Act1Progression.Beat beat)
        {
            switch (beat)
            {
                case Act1Progression.Beat.HomeChoice:
                    return new[]
                    {
                        Note("act1_note_home", "The split",
                            "A short note on the kitchen table. Two houses now. The Britannica stays in Pretoria. So does the desk.")
                    };
                case Act1Progression.Beat.Encyclopedia:
                    return new[]
                    {
                        Note("act1_notes_encyclopedia", "Notebook — space / energy / computers",
                            "Handwritten lists. Rockets. Reactors. Circuits. Tags for later, not marks.")
                    };
                case Act1Progression.Beat.Vic20Night:
                    return new[]
                    {
                        Note("act1_note_vic20", "BASIC scribbles",
                            "A page of line numbers. The manual said six months. The examples ran in a few days. Assembly tomorrow will be slow.")
                    };
                case Act1Progression.Beat.Blastar:
                    return new[]
                    {
                        Clipping("act1_clipping_blastar", "Magazine: Blastar purchased",
                            "A computer magazine clipping. They will print Blastar and enclose about 500 dollars. First sale. Not a company memo.")
                    };
                case Act1Progression.Beat.BryanstonStairs:
                    return new[]
                    {
                        Letter("act1_letter_hospital", "Hospital, then a transfer",
                            "A folded hospital note. Recovery. Then a letter: Pretoria Boys High will take him. Bryanston is behind him.")
                    };
                case Act1Progression.Beat.BoysHigh:
                    return new[]
                    {
                        Notice("act1_notice_chapel", "Chapel attendance",
                            "A chapel slip. Sit still. Do not make a speech."),
                        Clipping("act1_clipping_club", "Computer club",
                            "A duplicated flyer. Tuesday, machines, no uniforms required in the room."),
                        Letter("act1_letter_grades", "Report",
                            "Physics is high. Afrikaans and divinity are not. The paper does not ask which ones he thinks matter.")
                    };
                case Act1Progression.Beat.GardenRocket:
                    return new[]
                    {
                        Note("act1_note_rocket", "Garden test — ion motor aside",
                            "A copied diagram from a technical book: ion motors, then a smaller note about a modest charge in the garden. No hospital addendum.")
                    };
                case Act1Progression.Beat.WorldOutside:
                    return new[]
                    {
                        Clipping("act1_clipping_press", "Newspaper with gaps",
                            "Columns end mid-sentence. You can still read the shape of what is missing."),
                        Notice("act1_notice_callup", "After matric",
                            "A call-up notice on the table. Not a lesson. A date.")
                    };
                case Act1Progression.Beat.ExitPlan:
                    return new[]
                    {
                        Letter("act1_letter_maye", "Papers through Maye",
                            "Mother's Canadian papers. A passport in motion. Pack the computer. Kimbal is not on this flight.")
                    };
                default:
                    return new InboxTicket[0];
            }
        }

        private static DialogueDefinition HomeChoice()
        {
            var start = Node("home_start",
                Lines(
                    "Narrator",
                    "Pretoria, 1971, then 1979. The marriage ends. Two houses. You are eight, and the encyclopedias are in one of them.",
                    "Maye",
                    "You can come with me.",
                    "Narrator",
                    "The other house has the Britannica and, later, a computer. Waterkloof House is still ahead. The books are here now."),
                new List<DialogueChoice>
                {
                    Choice("Stay in Pretoria with the shelves. That is the point.",
                        "home_pretoria", Act1Progression.TagEncyclopedia, focus: 2),
                    Choice("Visit your mother. Keep the desk and the volumes here.",
                        "home_visit", Act1Progression.TagEncyclopedia, focus: 1, exit: 1),
                    Choice("Ask that the encyclopedias follow you either way.",
                        "home_books", Act1Progression.TagEncyclopedia, focus: 1, thick: 1)
                });

            var pret = Node("home_pretoria",
                Lines("Narrator",
                    "You stay. The rooms are not gentle. The pages are. Encyclopedia unlocked — not as a prize, as furniture."));
            var visit = Node("home_visit",
                Lines("Narrator",
                    "You see her when you can. The set stays in Pretoria. So does the habit of finishing a volume."));
            var books = Node("home_books",
                Lines("Narrator",
                    "A few volumes travel. Most do not need to. You already know where the rest live."));

            return Def(Act1Progression.DialogueId(Act1Progression.Beat.HomeChoice), start, pret, visit, books);
        }

        private static DialogueDefinition Encyclopedia()
        {
            var start = Node("enc_start",
                Lines(
                    "Narrator",
                    "You do not skim. Space. Energy. Computers. Names go into a notebook for later — not for a mark.",
                    "Young Elon",
                    "If it is written down, I can learn it.",
                    "Narrator",
                    "The notes will turn into tags when you need them. Nothing here is scored."),
                new List<DialogueChoice>
                {
                    Choice("Copy the space and rocketry entries.",
                        "enc_space", Act1Progression.TagSpace, focus: 2),
                    Choice("Stay on energy and engines.",
                        "enc_energy", Act1Progression.TagEnergy, focus: 1, exit: 1),
                    Choice("Map the computer and electronics pages.",
                        "enc_comp", Act1Progression.TagComputers, focus: 1, thick: 1)
                });

            var space = Node("enc_space",
                Lines("Narrator", "Orbits, mass, and the word multi-planetary before you have a ticket."));
            var energy = Node("enc_energy",
                Lines("Narrator", "Power density sticks. Everything expensive is an energy problem wearing a costume."));
            var comp = Node("enc_comp",
                Lines("Narrator", "Circuits first. Software later. The notebook does not grade you."));

            return Def(Act1Progression.DialogueId(Act1Progression.Beat.Encyclopedia), start, space, energy, comp);
        }

        private static DialogueDefinition Vic20Night()
        {
            var start = Node("vic_start",
                Lines(
                    "Narrator",
                    "A VIC-20 arrives. The BASIC manual promises six months of lessons. You treat the estimate as a dare.",
                    "Young Elon",
                    "The examples run if I type them correctly.",
                    "Narrator",
                    "A few days later the book is done. School the next morning is slower than the code. Programming is unlocked. Sleep is not."),
                new List<DialogueChoice>
                {
                    Choice("Sit up until the last example runs. Pay for it in assembly.",
                        "vic_up", Act1Progression.TagProgramming, focus: 2, thick: 1),
                    Choice("Split the nights. Finish the manual in days, not months.",
                        "vic_split", Act1Progression.TagProgramming, focus: 1),
                    Choice("Keep the useful pages. Skip the rest of the lectures.",
                        "vic_skim", Act1Progression.TagProgramming, exit: 1)
                });

            var up = Node("vic_up",
                Lines("Narrator", "The machine obeys. The school day does not. You still go."));
            var split = Node("vic_split",
                Lines("Narrator", "Two late nights, not six months. The manual was padding."));
            var skim = Node("vic_skim",
                Lines("Narrator", "You keep GOTO, variables, and the habit of testing. The pep talk chapters stay shut."));

            return Def(Act1Progression.DialogueId(Act1Progression.Beat.Vic20Night), start, up, split, skim);
        }

        private static DialogueDefinition Blastar()
        {
            var start = Node("bla_start",
                Lines(
                    "Narrator",
                    "You write Blastar. A computer magazine will buy it for about 500 dollars and print the listing.",
                    "Young Elon",
                    "Then it is a company, even if it fits on a cassette.",
                    "Narrator",
                    "First sale. A clipping will arrive. Not a payroll."),
                new List<DialogueChoice>
                {
                    Choice("Post the tape as it is.",
                        "bla_send", Act1Progression.TagFirstCompany, exit: 1, money: 500),
                    Choice("Polish the title screen, then post it.",
                        "bla_polish", Act1Progression.TagFirstCompany, focus: 2, money: 500),
                    Choice("Keep a copy. Send a clean listing.",
                        "bla_copy", Act1Progression.TagFirstCompany, thick: 1, money: 500)
                });

            var send = Node("bla_send",
                Lines("Narrator", "Five hundred dollars, more or less. The magazine clipping is the receipt."));
            var polish = Node("bla_polish",
                Lines("Narrator", "The sprite is tidier. The cheque is the same size."));
            var copy = Node("bla_copy",
                Lines("Narrator", "You keep the source. They keep the game. About 500 dollars changes hands."));

            return Def(Act1Progression.DialogueId(Act1Progression.Beat.Blastar), start, send, polish, copy);
        }

        private static DialogueDefinition BryanstonStairs()
        {
            var start = Node("br_start",
                Lines(
                    "Narrator",
                    "Bryanston High. A staircase. Older boys go too far. You hit the bottom. Then a hospital.",
                    "Young Elon",
                    "There is nothing to win on these stairs.",
                    "Narrator",
                    "No contest. No drill. Recovery, then a transfer. Stay quiet, tell an adult, or get stronger later. Those are the tools. Winning a fight is not one of them."),
                new List<DialogueChoice>
                {
                    Choice("Stay quiet. Get through the hospital.",
                        "br_quiet", tag: null, thick: 2),
                    Choice("Tell an adult what happened.",
                        "br_tell", tag: null, thick: 1, exit: 1),
                    Choice("Train later. Not a fight today.",
                        "br_train", tag: null, focus: 1, thick: 1)
                });

            var quiet = Node("br_quiet",
                Lines("Narrator", "You keep the story small. The ward is enough. Pretoria Boys High is the next school, not this landing."));
            var tell = Node("br_tell",
                Lines("Narrator", "Someone is told. It does not undo the stairs. It is still a valid line."));
            var train = Node("br_train",
                Lines("Narrator", "A later version of you can lift more. This version goes to hospital, then to Boys High."));

            return Def(Act1Progression.DialogueId(Act1Progression.Beat.BryanstonStairs), start, quiet, tell, train);
        }

        private static DialogueDefinition BoysHigh()
        {
            var start = Node("bh_start",
                Lines(
                    "Narrator",
                    "Pretoria Boys High is steadier. Computer club. Physics. Chapel. Afrikaans. A report that wants all of it.",
                    "Young Elon",
                    "Physics is a tool. Some of the rest is noise.",
                    "Narrator",
                    "Inbox: a chapel slip, a club flyer, a grade letter. Skipping what feels pointless is allowed."),
                new List<DialogueChoice>
                {
                    Choice("Computer club and physics first.",
                        "bh_club", Act1Progression.TagComputers, focus: 2),
                    Choice("Sit chapel. Keep the argument to yourself.",
                        "bh_chapel", tag: null, focus: 1, thick: 1),
                    Choice("Skip the bits that feel like noise. Keep the marks that matter.",
                        "bh_skip", tag: null, focus: 1, exit: 1)
                });

            var club = Node("bh_club",
                Lines("Narrator", "The machines do not require a hymn. The club is enough church for a Tuesday."));
            var chapel = Node("bh_chapel",
                Lines("Narrator", "You sit. You do not preach back. The hour ends."));
            var skip = Node("bh_skip",
                Lines("Narrator", "Afrikaans and divinity get the minimum. Physics does not."));

            return Def(Act1Progression.DialogueId(Act1Progression.Beat.BoysHigh), start, club, chapel, skip);
        }

        private static DialogueDefinition GardenRocket()
        {
            var start = Node("rk_start",
                Lines(
                    "Narrator",
                    "A small rocket in the garden. A technical book mentions ion motors. You want a measurement, not a story about lost fingers.",
                    "Young Elon",
                    "Small charge. Watch the mass.",
                    "Narrator",
                    "Physics unlocks as a tag. Care is a choice, not a cutscene."),
                new List<DialogueChoice>
                {
                    Choice("Mix carefully. Small charge. Measure twice.",
                        "rk_care", Act1Progression.TagPhysics, focus: 2),
                    Choice("A larger mix. Still in the garden. Still intact.",
                        "rk_risk", Act1Progression.TagPhysics, focus: 1, thick: 1),
                    Choice("Read the ion-engine chapter, then a modest test.",
                        "rk_ion", Act1Progression.TagPhysics, focus: 1, exit: 1)
                });

            var care = Node("rk_care",
                Lines("Narrator", "It flies a little. You write the number down. No one drives to a clinic."));
            var risk = Node("rk_risk",
                Lines("Narrator", "Louder. Still a model. Hands stay attached. The note is shorter because you were busy."));
            var ion = Node("rk_ion",
                Lines("Narrator", "The ion motor stays in the book for now. The garden rocket is chemical and small. Both count."));

            return Def(Act1Progression.DialogueId(Act1Progression.Beat.GardenRocket), start, care, risk, ion);
        }

        private static DialogueDefinition WorldOutside()
        {
            var start = Node("wo_start",
                Lines(
                    "Narrator",
                    "A newspaper arrives with gaps in it. After matric, a call-up notice sits on the table. The United States is a farther road. Canada is a door that actually opens.",
                    "Young Elon",
                    "I am not writing an essay about this. I am leaving a ceiling.",
                    "Narrator",
                    "Background, not a lecture. Conscription and the censored press are facts in the room."),
                new List<DialogueChoice>
                {
                    Choice("Read the paper between the lines.",
                        "wo_press", tag: null, focus: 1, exit: 1),
                    Choice("Look at the call-up date. Then look north.",
                        "wo_call", tag: null, thick: 1, exit: 2),
                    Choice("Both. The country is the ceiling, not the lesson.",
                        "wo_both", tag: null, focus: 1, exit: 2)
                });

            var press = Node("wo_press",
                Lines("Narrator", "You infer the missing copy. You do not give a talk about it."));
            var call = Node("wo_call",
                Lines("Narrator", "The notice is specific. Your plan is also specific: not here."));
            var both = Node("wo_both",
                Lines("Narrator", "Censorship and the draft share a drawer. The passport application gets the desk."));

            return Def(Act1Progression.DialogueId(Act1Progression.Beat.WorldOutside), start, press, call, both);
        }

        private static DialogueDefinition ExitPlan()
        {
            var start = Node("ex_start",
                Lines(
                    "Narrator",
                    "Maye's Canadian papers make a passport possible. You pack the computer. Kimbal stays.",
                    "Young Elon",
                    "I will send for the rest later.",
                    "Narrator",
                    "If the stamp is slow, a term at the University of Pretoria can fill the wait. Then the flight. Act 1 ends in the air, not in a speech."),
                new List<DialogueChoice>
                {
                    Choice("Pack the VIC-20. Fly when the passport lands.",
                        "ex_fly", tag: null, exit: 2),
                    Choice("Sit a term at the University of Pretoria while the papers come.",
                        "ex_up", tag: null, focus: 1, exit: 1),
                    Choice("Say goodbye to Kimbal. He stays for now.",
                        "ex_kimbal", tag: null, thick: 1, exit: 1)
                });

            var fly = Node("ex_fly",
                Lines("Narrator", "The machine goes in the bag. Canada is unlocked. Zip2 is a later desk."));
            var up = Node("ex_up",
                Lines("Narrator", "A short wait at the University of Pretoria. Then the same plane. Kimbal is still not on it."));
            var kimbal = Node("ex_kimbal",
                Lines("Narrator", "He remains. You do not. The computer is packed. The flight does not wait for a reunion."));

            return Def(Act1Progression.DialogueId(Act1Progression.Beat.ExitPlan), start, fly, up, kimbal);
        }

        private static DialogueChoice Choice(
            string text,
            string next,
            string tag = null,
            int focus = 0,
            int thick = 0,
            int exit = 0,
            int money = 0)
        {
            return new DialogueChoice(text, next, tag, focus, thick, exit, money);
        }

        private static DialogueNode Node(string id, List<DialogueLine> lines, List<DialogueChoice> choices = null)
        {
            return new DialogueNode(id, lines, choices);
        }

        private static List<DialogueLine> Lines(params string[] speakerThenText)
        {
            var list = new List<DialogueLine>();
            for (int i = 0; i + 1 < speakerThenText.Length; i += 2)
                list.Add(new DialogueLine(speakerThenText[i], speakerThenText[i + 1]));
            return list;
        }

        private static DialogueDefinition Def(string id, DialogueNode start, params DialogueNode[] extra)
        {
            var nodes = new List<DialogueNode> { start };
            if (extra != null)
                nodes.AddRange(extra);
            return new DialogueDefinition(id, start.Id, nodes);
        }

        private static InboxTicket Note(string id, string title, string body)
        {
            return Personal(id, "Note", title, body);
        }

        private static InboxTicket Letter(string id, string title, string body)
        {
            return Personal(id, "Letter", title, body);
        }

        private static InboxTicket Clipping(string id, string title, string body)
        {
            return Personal(id, "Clipping", title, body);
        }

        private static InboxTicket Notice(string id, string title, string body)
        {
            return Personal(id, "Notice", title, body);
        }

        private static InboxTicket Personal(string id, string kind, string title, string body)
        {
            return new InboxTicket(
                id,
                kind.ToLowerInvariant(),
                kind,
                PrototypeContent.LocationPretoria,
                "Pretoria, South Africa",
                title,
                body,
                1,
                kind);
        }
    }
}
