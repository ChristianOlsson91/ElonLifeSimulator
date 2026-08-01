using System.Collections.Generic;
using ElonLifeSim.Core.Models;
using ElonLifeSim.Core.Services;

namespace ElonLifeSim.Core.Content
{
    /// <summary>
    /// Act 1 South Africa narrative dialogues for each beat.
    /// Tone: respectful, inspiring, lightly humorous — never mocking of trauma or people.
    /// Settings labeled as Home / School / Library without full multi-room tilemaps.
    /// </summary>
    public static class Act1Content
    {
        public static DialogueDefinition GetDialogueForBeat(Act1Progression.Beat beat)
        {
            switch (beat)
            {
                case Act1Progression.Beat.HomeIntro:
                    return HomeIntro();
                case Act1Progression.Beat.SchoolStaircase:
                    return SchoolStaircase();
                case Act1Progression.Beat.LibraryEncyclopedia:
                    return LibraryEncyclopedia();
                case Act1Progression.Beat.RocketsComputersPhysics:
                    return RocketsComputersPhysics();
                case Act1Progression.Beat.LeaveForCanada:
                    return LeaveForCanada();
                default:
                    return null;
            }
        }

        private static DialogueDefinition HomeIntro()
        {
            var start = new DialogueNode(
                "home_start",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "[PLACEHOLDER · Home, Pretoria] A quiet room, a stack of books, and a boy who would rather " +
                        "understand the universe than fit neatly into any classroom box."),
                    new DialogueLine("Young Elon",
                        "If something is hard… that usually means it's worth figuring out."),
                    new DialogueLine("Narrator",
                        "Act 1 begins here. Your choices won't rewrite history wholesale — they show how " +
                        "curiosity, grit, and hard decisions stacked up.")
                },
                new List<DialogueChoice>
                {
                    new DialogueChoice("Head to school", null),
                    new DialogueChoice("Read one more page first", "home_read")
                });

            var read = new DialogueNode(
                "home_read",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "One more page becomes ten. The world outside can wait a little — ideas won't.")
                });

            return new DialogueDefinition("act1_home_intro", "home_start", new[] { start, read });
        }

        private static DialogueDefinition SchoolStaircase()
        {
            // Historically inspired: severe bullying / staircase incident — handled with care, no mockery.
            var start = new DialogueNode(
                "school_start",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "[PLACEHOLDER · School] The day turns dark. Older boys go too far. " +
                        "You are shoved down a flight of stairs and badly hurt — a memory that never softens."),
                    new DialogueLine("Young Elon",
                        "…Why are people like that?"),
                    new DialogueLine("Narrator",
                        "Pain and isolation cut deep. Some find escape in bitterness. You find it in books, " +
                        "machines, and the idea that a better future can be engineered.")
                },
                new List<DialogueChoice>
                {
                    new DialogueChoice("Withdraw into reading and ideas", "school_books"),
                    new DialogueChoice("Tell yourself: build something that matters", "school_build"),
                    new DialogueChoice("Lean on family and keep going to class", "school_family")
                });

            var books = new DialogueNode(
                "school_books",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "The library and the page become safer ground. Understanding systems feels like power " +
                        "when people don't.")
                });
            var build = new DialogueNode(
                "school_build",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "A quiet vow forms: make things that lift people up — rockets, cars, networks — " +
                        "not tear them down.")
                });
            var family = new DialogueNode(
                "school_family",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "Home is not perfect, but it is a place to heal enough to keep learning.")
                });

            return new DialogueDefinition("act1_school_staircase", "school_start",
                new[] { start, books, build, family });
        }

        private static DialogueDefinition LibraryEncyclopedia()
        {
            var start = new DialogueNode(
                "lib_start",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "[PLACEHOLDER · Library] You dig into the encyclopedia — not a skim, a campaign. " +
                        "Volume after volume. Facts stack into a map of how the world works."),
                    new DialogueLine("Young Elon",
                        "If it's written down… I can learn it. If I can learn it, maybe I can change it."),
                    new DialogueLine("Narrator",
                        "Obsessive reading becomes a superpower: breadth first, then depth where it matters.")
                },
                new List<DialogueChoice>
                {
                    new DialogueChoice("Finish another volume tonight", "lib_finish"),
                    new DialogueChoice("Cross-check science entries carefully", "lib_science")
                });

            var finish = new DialogueNode(
                "lib_finish",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "Sleep loses. Knowledge wins a little more ground.")
                });
            var science = new DialogueNode(
                "lib_science",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "Physics and engineering entries stick hardest — they feel like tools, not trivia.")
                });

            return new DialogueDefinition("act1_library_encyclopedia", "lib_start",
                new[] { start, finish, science });
        }

        private static DialogueDefinition RocketsComputersPhysics()
        {
            var start = new DialogueNode(
                "tech_start",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "[PLACEHOLDER · Home study] Rockets, computers, and physics stop being daydreams " +
                        "and start looking like careers. Code is a kind of magic you can type. Space is a " +
                        "problem of energy, mass, and will."),
                    new DialogueLine("Young Elon",
                        "What if we could make life multi-planetary? What if software could organize a city?"),
                    new DialogueLine("Narrator",
                        "You don't have the companies yet — only the questions. That's enough to leave home for.")
                },
                new List<DialogueChoice>
                {
                    new DialogueChoice("Double down on computers and software", "tech_soft"),
                    new DialogueChoice("Keep rockets and physics at the center", "tech_rockets"),
                    new DialogueChoice("Study both — refuse a false choice", "tech_both")
                });

            var soft = new DialogueNode(
                "tech_soft",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "Software is the lever you can pull first. Rockets can wait for capital and teams.")
                });
            var rockets = new DialogueNode(
                "tech_rockets",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "The sky stays on the vision board. First you need a path to the places where " +
                        "hard engineering gets funded.")
                });
            var both = new DialogueNode(
                "tech_both",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "Breadth again: code pays the bills of ambition; physics keeps the ambition honest.")
                });

            return new DialogueDefinition("act1_rockets_computers_physics", "tech_start",
                new[] { start, soft, rockets, both });
        }

        private static DialogueDefinition LeaveForCanada()
        {
            var start = new DialogueNode(
                "leave_start",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "[PLACEHOLDER · Home · Decision] South Africa is home — and also a ceiling. " +
                        "Canada offers study, relatives who can host you, and a bridge toward North America."),
                    new DialogueLine("Young Elon",
                        "Leaving is hard. Staying still is harder."),
                    new DialogueLine("Narrator",
                        "This is the hinge of Act 1: commit to the journey, or delay and risk never going.")
                },
                new List<DialogueChoice>
                {
                    new DialogueChoice("Leave for Canada — start the next chapter", "leave_yes"),
                    new DialogueChoice("Hesitate… then pack anyway", "leave_hesitate")
                });

            // Both paths complete Act 1 — decision is to leave; framing differs.
            var yes = new DialogueNode(
                "leave_yes",
                new List<DialogueLine>
                {
                    new DialogueLine("Narrator",
                        "You choose motion. Canada unlocks. In time you will found Zip2 — but first, " +
                        "cross the ocean and keep learning.")
                });
            var hesitate = new DialogueNode(
                "leave_hesitate",
                new List<DialogueLine>
                {
                    new DialogueLine("Young Elon",
                        "One more night. Then the plane."),
                    new DialogueLine("Narrator",
                        "Fear is honest; the ticket still gets used. Canada unlocks. Zip2 waits in the future.")
                });

            return new DialogueDefinition("act1_leave_for_canada", "leave_start",
                new[] { start, yes, hesitate });
        }
    }
}
