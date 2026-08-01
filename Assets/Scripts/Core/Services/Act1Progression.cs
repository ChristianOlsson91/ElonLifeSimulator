using System;
using System.Collections.Generic;
using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Ordered Act 1 (South Africa) narrative beats.
    /// Completing the sequence unlocks Canada and the path to found Zip2.
    ///
    /// Beats (GDD): staircase/bullying; encyclopedia; rockets/computers/physics; leave for Canada.
    /// School / home / library are represented as narrative labels in dialogue (not full tilemaps).
    /// </summary>
    public sealed class Act1Progression
    {
        public enum Beat
        {
            NotStarted = 0,
            /// <summary>Home — intro in Pretoria.</summary>
            HomeIntro = 1,
            /// <summary>School — bullying / staircase incident (respectful framing).</summary>
            SchoolStaircase = 2,
            /// <summary>Library — obsessive encyclopedia reading.</summary>
            LibraryEncyclopedia = 3,
            /// <summary>Home study — rockets, computers, physics curiosity.</summary>
            RocketsComputersPhysics = 4,
            /// <summary>Decision to leave South Africa for Canada.</summary>
            LeaveForCanada = 5,
            Complete = 6
        }

        public Beat CurrentBeat { get; private set; } = Beat.NotStarted;
        public bool IsComplete => CurrentBeat == Beat.Complete;
        public bool CanadaUnlocked { get; private set; }
        public bool Zip2FoundingUnlocked { get; private set; }

        /// <summary>Raised when beat advances (previous, current).</summary>
        public event Action<Beat, Beat> BeatChanged;
        public event Action Act1Completed;

        public void Reset()
        {
            CurrentBeat = Beat.NotStarted;
            CanadaUnlocked = false;
            Zip2FoundingUnlocked = false;
        }

        /// <summary>Begins Act 1 at Home intro.</summary>
        public void Begin()
        {
            SetBeat(Beat.HomeIntro);
        }

        /// <summary>
        /// Advances to the next beat after the player finishes the current beat's dialogue.
        /// Completing LeaveForCanada unlocks Canada + Zip2 founding path.
        /// </summary>
        public bool Advance()
        {
            if (IsComplete)
                return false;

            switch (CurrentBeat)
            {
                case Beat.NotStarted:
                    SetBeat(Beat.HomeIntro);
                    return true;
                case Beat.HomeIntro:
                    SetBeat(Beat.SchoolStaircase);
                    return true;
                case Beat.SchoolStaircase:
                    SetBeat(Beat.LibraryEncyclopedia);
                    return true;
                case Beat.LibraryEncyclopedia:
                    SetBeat(Beat.RocketsComputersPhysics);
                    return true;
                case Beat.RocketsComputersPhysics:
                    SetBeat(Beat.LeaveForCanada);
                    return true;
                case Beat.LeaveForCanada:
                    CompleteAct1();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Dialogue for the current beat (null if complete / not started).</summary>
        public DialogueDefinition GetCurrentDialogue()
        {
            return Act1Content.GetDialogueForBeat(CurrentBeat);
        }

        public string GetBeatLocationLabel()
        {
            switch (CurrentBeat)
            {
                case Beat.HomeIntro: return "Home · Pretoria";
                case Beat.SchoolStaircase: return "School · Pretoria";
                case Beat.LibraryEncyclopedia: return "Library · Pretoria";
                case Beat.RocketsComputersPhysics: return "Home study · Pretoria";
                case Beat.LeaveForCanada: return "Home · Decision";
                case Beat.Complete: return "Act 1 complete";
                default: return "Pretoria";
            }
        }

        /// <summary>All named Act 1 event ids for inventory/tests.</summary>
        public static IReadOnlyList<string> NamedEventIds => new[]
        {
            "act1_home_intro",
            "act1_school_staircase",
            "act1_library_encyclopedia",
            "act1_rockets_computers_physics",
            "act1_leave_for_canada"
        };

        private void CompleteAct1()
        {
            CanadaUnlocked = true;
            Zip2FoundingUnlocked = true;
            SetBeat(Beat.Complete);
            Act1Completed?.Invoke();
        }

        private void SetBeat(Beat next)
        {
            var prev = CurrentBeat;
            CurrentBeat = next;
            BeatChanged?.Invoke(prev, next);
        }
    }
}
