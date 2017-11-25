using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Vampire
{
    public static class VampireGen
    {

        public static bool TryGiveVampirismHediff(Pawn pawn, int generation, BloodlineDef bloodline, Pawn sire, bool firstVampire = false)
        {
            try
            {
                HediffVampirism vampHediff = (HediffVampirism)HediffMaker.MakeHediff(VampDefOf.ROM_Vampirism, pawn, null);
                vampHediff.firstVampire = firstVampire;
                vampHediff.sire = sire?.VampComp() ?? null;
                vampHediff.generation = generation;
                vampHediff.bloodline = bloodline;
                pawn.health.AddHediff(vampHediff, null, null);
                return true;
            }
            catch (Exception e) { Log.Error(e.ToString()); }
            return false;
        }

        public static bool TryGiveVampirismHediffFromSire(Pawn pawn, Pawn sire, bool firstVampire = false)
        {
            try
            {
                HediffVampirism vampHediff = (HediffVampirism)HediffMaker.MakeHediff(VampDefOf.ROM_Vampirism, pawn, null);
                vampHediff.firstVampire = firstVampire;
                vampHediff.sire = sire.VampComp();
                vampHediff.generation = sire.VampComp().Generation + 1;
                vampHediff.bloodline = sire.VampComp().Bloodline;
                pawn.health.AddHediff(vampHediff, null, null);
                return true;
            }
            catch (Exception e) { Log.Error(e.ToString()); }
            return false;
        }

        public static bool TryGiveVampireAdditionalHediffs(Pawn pawn)
        {
            try
            {
                BodyPartRecord bpR = pawn.health?.hediffSet?.GetNotMissingParts().FirstOrDefault(x => x.def == BodyPartDefOf.Jaw);

                if (bpR != null && pawn?.VampComp()?.Bloodline?.fangsHediff != null)
                {
                    pawn.health.RestorePart(bpR, null, true);
                    pawn.health.AddHediff(pawn.VampComp().Bloodline.fangsHediff, bpR, null);
                }
                if (pawn.VampComp()?.Bloodline?.bloodlineHediff != null)
                {
                    pawn.health.AddHediff(pawn.VampComp().Bloodline.bloodlineHediff, null, null);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return false;
            }
            return true;
        }

        public static Pawn GenerateVampire(int generation, BloodlineDef bloodline, Pawn sire, Faction vampFaction = null, bool firstVampire = false)
        {
            //Lower generation vampires are impossibly old.
            float? math = (sire != null) ? sire.ageTracker.AgeChronologicalYearsFloat + new FloatRange(100, 300).RandomInRange :
                (generation > 4) ? Mathf.Clamp(2000 - (generation * Rand.Range(20, 200)), 16, 2000) :
                                   100000 - (generation * Rand.Range(10000, 50000));

            Faction faction = (vampFaction != null) ? vampFaction :
                              (generation < 7) ? Find.FactionManager.FirstFactionOfDef(VampDefOf.ROMV_LegendaryVampires) : VampireUtility.RandVampFaction;
            PawnGenerationRequest request = new PawnGenerationRequest(
                PawnKindDefOf.SpaceRefugee, Faction.OfSpacer, PawnGenerationContext.NonPlayer,
                -1, false, false, false, false, true, true, 20f, false, true,
                true, false, false, false, false, null, null, null, null, null, null, null);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (firstVampire)
            {
                NameTriple caineName = new NameTriple("Caine", "Caine", "Darkfather");
                pawn.Name = caineName;
            }
            pawn.story.hairColor = PawnHairColors.RandomHairColor(pawn.story.SkinColor, 20);
            if (!bloodline.allowsHair)
                pawn.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved");
            pawn.VampComp().InitializeVampirism(sire, bloodline, generation, firstVampire);
            //TryGiveVampirismHediff(pawn, generation, bloodline, sire, firstVampire);
            return pawn;
        }

    }
}
