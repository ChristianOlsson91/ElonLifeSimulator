using System;
using System.Collections.Generic;
using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Ordered Act 1 (Pretoria) beats. Completing the last beat unlocks Canada and Zip2 founding.
    /// </summary>
    public sealed class Act1Progression
    {
        public enum Beat
        {
            NotStarted = 0,
            HomeChoice = 1,
            Encyclopedia = 2,
            Vic20Night = 3,
            Blastar = 4,
            BryanstonStairs = 5,
            BoysHigh = 6,
            GardenRocket = 7,
            WorldOutside = 8,
            ExitPlan = 9,
            Complete = 10
        }

        public static readonly IReadOnlyList<Beat> OrderedBeats = new[]
        {
            Beat.HomeChoice,
            Beat.Encyclopedia,
            Beat.Vic20Night,
            Beat.Blastar,
            Beat.BryanstonStairs,
            Beat.BoysHigh,
            Beat.GardenRocket,
            Beat.WorldOutside,
            Beat.ExitPlan
        };

        public const string TagEncyclopedia = "encyclopedia";
        public const string TagProgramming = "programming";
        public const string TagPhysics = "physics";
        public const string TagSpace = "space";
        public const string TagEnergy = "energy";
        public const string TagComputers = "computers";
        public const string TagFirstCompany = "first_company";

        public Beat CurrentBeat { get; private set; } = Beat.NotStarted;
        public bool IsComplete => CurrentBeat == Beat.Complete;
        public bool CanadaUnlocked { get; private set; }
        public bool Zip2FoundingUnlocked { get; private set; }

        public int Focus { get; private set; }
        public int ThickSkin { get; private set; }
        public int ExitPlan { get; private set; }
        public int Money { get; private set; }

        private readonly HashSet<string> _tags = new HashSet<string>(StringComparer.Ordinal);

        public IReadOnlyCollection<string> Tags => _tags;

        public event Action<Beat, Beat> BeatChanged;
        public event Action Act1Completed;

        public static Beat FirstBeat => OrderedBeats[0];

        public static string DialogueId(Beat beat)
        {
            switch (beat)
            {
                case Beat.HomeChoice: return "act1_home_choice";
                case Beat.Encyclopedia: return "act1_encyclopedia";
                case Beat.Vic20Night: return "act1_vic20_night";
                case Beat.Blastar: return "act1_blastar";
                case Beat.BryanstonStairs: return "act1_bryanston_stairs";
                case Beat.BoysHigh: return "act1_boys_high";
                case Beat.GardenRocket: return "act1_garden_rocket";
                case Beat.WorldOutside: return "act1_world_outside";
                case Beat.ExitPlan: return "act1_exit_plan";
                default: return null;
            }
        }

        public static IReadOnlyList<string> NamedEventIds
        {
            get
            {
                var ids = new string[OrderedBeats.Count];
                for (int i = 0; i < OrderedBeats.Count; i++)
                    ids[i] = DialogueId(OrderedBeats[i]);
                return ids;
            }
        }

        public void Reset()
        {
            CurrentBeat = Beat.NotStarted;
            CanadaUnlocked = false;
            Zip2FoundingUnlocked = false;
            Focus = 0;
            ThickSkin = 0;
            ExitPlan = 0;
            Money = 0;
            _tags.Clear();
        }

        public void Begin()
        {
            SetBeat(FirstBeat);
        }

        public bool Advance()
        {
            if (IsComplete)
                return false;

            if (CurrentBeat == Beat.NotStarted)
            {
                SetBeat(FirstBeat);
                return true;
            }

            int i = IndexOf(CurrentBeat);
            if (i < 0)
                return false;
            if (i >= OrderedBeats.Count - 1)
            {
                CompleteAct1();
                return true;
            }

            SetBeat(OrderedBeats[i + 1]);
            return true;
        }

        public DialogueDefinition GetCurrentDialogue()
        {
            return Act1Content.GetDialogueForBeat(CurrentBeat);
        }

        public void ApplyChoice(DialogueChoice choice)
        {
            if (choice == null)
                return;
            Focus += choice.FocusDelta;
            ThickSkin += choice.ThickSkinDelta;
            ExitPlan += choice.ExitPlanDelta;
            Money += choice.MoneyDelta;
            if (!string.IsNullOrEmpty(choice.Tag))
                _tags.Add(choice.Tag);
        }

        public bool HasTag(string tag)
        {
            return !string.IsNullOrEmpty(tag) && _tags.Contains(tag);
        }

        public string GetBeatLocationLabel()
        {
            switch (CurrentBeat)
            {
                case Beat.HomeChoice: return "Home · Pretoria";
                case Beat.Encyclopedia: return "Books · Pretoria";
                case Beat.Vic20Night: return "VIC-20 · Pretoria";
                case Beat.Blastar: return "Blastar · Pretoria";
                case Beat.BryanstonStairs: return "Bryanston High";
                case Beat.BoysHigh: return "Pretoria Boys High";
                case Beat.GardenRocket: return "Garden · Pretoria";
                case Beat.WorldOutside: return "Pretoria · 1988";
                case Beat.ExitPlan: return "Leaving · 1989";
                case Beat.Complete: return "Act 1 complete";
                default: return "Pretoria";
            }
        }

        private static int IndexOf(Beat beat)
        {
            for (int i = 0; i < OrderedBeats.Count; i++)
            {
                if (OrderedBeats[i] == beat)
                    return i;
            }
            return -1;
        }

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
