using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Vampire
{
    public static class BloodTypeUtility
    {
        public static BloodType BloodType(Pawn pawn)
        {
            if (!pawn?.Dead ?? false)
            {
                if (pawn?.RaceProps?.Animal ?? false)
                {
                    return Vampire.BloodType.Animal;
                }
                if (pawn?.RaceProps?.Humanlike ?? false)
                {
                    bool lowBlood = (BloodTypeUtility.IsLowblood(pawn));
                    bool highBlood = (BloodTypeUtility.IsHighblood(pawn));

                    if (BloodTypeUtility.IsHighblood(pawn) && BloodTypeUtility.IsLowblood(pawn))
                        return Vampire.BloodType.AverageBlood;
                    if (highBlood)
                        return Vampire.BloodType.HighBlood;
                    if (lowBlood)
                        return Vampire.BloodType.LowBlood;

                    return Vampire.BloodType.AverageBlood;
                }
            }
            return Vampire.BloodType.None;
        }

        public static string GetLabel(this BloodType bloodtype)
        {
            switch (bloodtype)
            {
                case Vampire.BloodType.Animal:
                    return "ROMV_BloodTypeAnimal".Translate();
                case Vampire.BloodType.AverageBlood:
                    return "ROMV_BloodTypeAverage".Translate();
                case Vampire.BloodType.LowBlood:
                    return "ROMV_BloodTypeLow".Translate();
                case Vampire.BloodType.HighBlood:
                    return "ROMV_BloodTypeHigh".Translate();
            }
            return "ROMV_BloodType_Unavailable".Translate();
        }

        //Slaves, chem-addicts, the sickly, or prostitutes
        public static bool IsLowblood(Pawn pawn)
        {
            if (pawn.story != null)
            {
                if (pawn.story.adulthood != null)
                {
                    string toCheck = pawn.story.GetBackstory(BackstorySlot.Adulthood).Title;
                    switch (toCheck)
                    {
                        case "Urbworld pimp":
                        case "Drifter":
                        case "Urbworld sex slave":
                        case "Slave chemist":
                            return true;
                    }
                }
                if (pawn.story.childhood != null)
                {
                    string toCheck2 = pawn.story.GetBackstory(BackstorySlot.Childhood).Title;
                    switch (toCheck2)
                    {
                        case "Colosseum cleaner":
                        case "Clone-farmed":
                        case "Child slave":
                        case "Work camp slave":
                        case "Organ farm":
                        case "Toxic child":
                        case "Rebel slave":
                        case "Medieval slave":
                        case "Slave farmer":
                        case "Sickly child":
                        case "Shunned girl":
                        case "Vatgrown slavegirl":
                            return true;
                    }
                }
            }
            return false;
        }

        //Those with 'high' status
        public static bool IsHighblood(Pawn pawn)
        {
            if (pawn.story != null)
            {
                if (pawn.story.adulthood != null)
                {
                    string toCheck = pawn.story.GetBackstory(BackstorySlot.Adulthood).Title;
                    switch (toCheck)
                    {
                        case "Medieval lord":
                        case "Glitterworld officer":
                        case "Glitterworld empath":
                        case "Glitterworld surgeon":
                            return true;
                    }
                }
                if (pawn.story.childhood != null)
                {
                    string toCheck2 = pawn.story.GetBackstory(BackstorySlot.Childhood).Title;
                    switch (toCheck2)
                    {
                        case "Squire":
                        case "Displaced noble":
                        case "Child-knave":
                        case "Aristocrat":
                        case "Upper urbworlder":
                        case "Spoiled child":
                        case "Reclusive prodigy":
                        case "Rich boy":
                        case "Pampered":
                        case "Privileged prodigy":
                        case "Feudal lordling":
                        case "Medieval lordling":
                        case "Noble ward":
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
