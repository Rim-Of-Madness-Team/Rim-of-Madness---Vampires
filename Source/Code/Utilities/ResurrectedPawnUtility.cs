// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises

// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace Vampire;

internal class ResurrectedPawnUtility
{
    public static Pawn DoGeneratePawnFromSource(Pawn sourcePawn, bool isBerserk = true, bool oathOfHastur = false)
    {
        var pawnKindDef = sourcePawn.kindDef;
        var factionDirect =
            isBerserk ? Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile) : Faction.OfPlayer;
        var pawn = (Pawn)ThingMaker.MakeThing(pawnKindDef.race);
        try
        {
            pawn.kindDef = pawnKindDef;
            pawn.SetFactionDirect(factionDirect);
            PawnComponentsUtility.CreateInitialComponents(pawn);
            pawn.gender = sourcePawn.gender;
            pawn.ageTracker.AgeBiologicalTicks = sourcePawn.ageTracker.AgeBiologicalTicks;
            pawn.ageTracker.AgeChronologicalTicks = sourcePawn.ageTracker.AgeChronologicalTicks;
            pawn.workSettings = new Pawn_WorkSettings(pawn);
            if (pawn.workSettings != null && sourcePawn.Faction.IsPlayer) pawn.workSettings.EnableAndInitialize();

            pawn.needs.SetInitialLevels();
            //Add hediffs?
            //Add relationships?
            if (pawn.RaceProps.Humanlike)
            {
                pawn.story.skinColorOverride =  sourcePawn.story.SkinColor;
                pawn.story.headType = sourcePawn.story.headType;
                pawn.story.HairColor = sourcePawn.story.HairColor;
                pawn.story.Childhood = sourcePawn.story.Childhood;
                pawn.story.Adulthood = sourcePawn.story.Adulthood;
                pawn.story.bodyType = sourcePawn.story.bodyType;
                pawn.story.hairDef = sourcePawn.story.hairDef;
                foreach (var current in sourcePawn.story.traits.allTraits) pawn.story.traits.GainTrait(current);

                SkillFixer(pawn, sourcePawn);
                RelationshipFixer(pawn, sourcePawn);
                AddedPartFixer(pawn, sourcePawn);
                //pawn.story.GenerateSkillsFromBackstory();
                var nameTriple = sourcePawn.Name as NameTriple;
                //if (!oathOfHastur)
                //{
                //    pawn.Name = new NameTriple(nameTriple.First, string.Concat(new string[]
                //        {
                //        "* ",
                //        Translator.Translate("Reanimated"),
                //        " ",
                //        nameTriple.Nick,
                //        " *"
                //        }), nameTriple.Last);
                //}
                pawn.Name = nameTriple;
            }

            var headGraphicFieldInfo = typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic);
            
            var sourceheadGraphic = headGraphicFieldInfo.GetValue(sourcePawn.story);
            headGraphicFieldInfo.SetValue(pawn.story, sourceheadGraphic);

            GenerateApparelFromSource(pawn, sourcePawn);
            var con = new PawnGenerationRequest();
            PawnInventoryGenerator.GenerateInventoryFor(pawn, con);
            if (isBerserk) pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
            //Log.Message(pawn.Name.ToStringShort);
            return pawn;
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            //Cthulhu.Utility.DebugReport(e.ToString());
        }

        return null;
    }

    public static void AddedPartFixer(Pawn pawn, Pawn sourcePawn = null)
    {
        foreach (var hediff in sourcePawn.health.hediffSet.hediffs)
            if (hediff is Hediff_AddedPart || hediff is Hediff_Implant)
                pawn.health.AddHediff(hediff);
    }

    public static void SkillFixer(Pawn pawn, Pawn sourcePawn = null)
    {
        //Add in and fix skill levels
        foreach (var skill in sourcePawn.skills.skills)
        {
            var pawnSkill = pawn.skills.GetSkill(skill.def);
            if (pawnSkill == null)
            {
                pawn.skills.skills.Add(skill);
            }
            else
            {
                pawnSkill.Level = skill.Level;
                pawnSkill.passion = skill.passion;
            }
        }
    }

    public static void RelationshipFixer(Pawn pawn, Pawn sourcePawn = null)
    {
        //Add in and fix all blood relationships
        if (sourcePawn.relations.DirectRelations != null && sourcePawn.relations.DirectRelations.Count > 0)
        {
            foreach (var pawnRel in sourcePawn.relations.DirectRelations)
                if (pawnRel.otherPawn != null && pawnRel.def != null)
                    pawn.relations.AddDirectRelation(pawnRel.def, pawnRel.otherPawn);
            sourcePawn.relations.ClearAllRelations();
        }
    }

    //public static bool Zombify(ReanimatedPawn pawn)
    //{
    //    if (pawn.Drawer == null)
    //    {
    //        return false;
    //    }
    //    if (pawn.Drawer.renderer == null)
    //    {
    //        return false;
    //    }
    //    if (pawn.Drawer.renderer.graphics == null)
    //    {
    //        return false;
    //    }
    //    if (!pawn.Drawer.renderer.graphics.AllResolved)
    //    {
    //        pawn.Drawer.renderer.graphics.ResolveAllGraphics();
    //    }
    //    if (pawn.Drawer.renderer.graphics.headGraphic == null)
    //    {
    //        return false;
    //    }
    //    if (pawn.Drawer.renderer.graphics.nakedGraphic == null)
    //    {
    //        return false;
    //    }
    //    if (pawn.Drawer.renderer.graphics.headGraphic.path == null)
    //    {
    //        return false;
    //    }
    //    if (pawn.Drawer.renderer.graphics.nakedGraphic.path == null)
    //    {
    //        return false;
    //    }
    //    GiveZombieSkinEffect(pawn);
    //    return true;
    //}

    // Credit goes to Justin C for the Zombie Apocalypse code.
    // Taken from Verse.ZombieMod_Utility
    public static Pawn GenerateClonePawnFromSource(Pawn sourcePawn)
    {
        var pawnKindDef = PawnKindDef.Named("ReanimatedCorpse");
        var factionDirect = Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile);
        var pawn = (Pawn)ThingMaker.MakeThing(pawnKindDef.race);
        pawn.kindDef = pawnKindDef;
        pawn.SetFactionDirect(factionDirect);
        pawn.pather = new Pawn_PathFollower(pawn);
        pawn.ageTracker = new Pawn_AgeTracker(pawn);
        pawn.health = new Pawn_HealthTracker(pawn);
        pawn.jobs = new Pawn_JobTracker(pawn);
        pawn.mindState = new Pawn_MindState(pawn);
        pawn.filth = new Pawn_FilthTracker(pawn);
        pawn.needs = new Pawn_NeedsTracker(pawn);
        pawn.stances = new Pawn_StanceTracker(pawn);
        pawn.natives = new Pawn_NativeVerbs(pawn);
        PawnComponentsUtility.CreateInitialComponents(pawn);
        if (pawn.RaceProps.ToolUser)
        {
            pawn.equipment = new Pawn_EquipmentTracker(pawn);
            pawn.carryTracker = new Pawn_CarryTracker(pawn);
            pawn.apparel = new Pawn_ApparelTracker(pawn);
            pawn.inventory = new Pawn_InventoryTracker(pawn);
        }

        if (pawn.RaceProps.Humanlike)
        {
            pawn.ownership = new Pawn_Ownership(pawn);
            pawn.skills = new Pawn_SkillTracker(pawn);
            pawn.relations = new Pawn_RelationsTracker(pawn);
            pawn.story = new Pawn_StoryTracker(pawn);
            pawn.workSettings = new Pawn_WorkSettings(pawn);
        }

        if (pawn.RaceProps.intelligence <= Intelligence.ToolUser) pawn.caller = new Pawn_CallTracker(pawn);
        //pawn.gender = Gender.None;
        pawn.gender = sourcePawn.gender;
        //Cthulhu.Utility.GenerateRandomAge(pawn, pawn.Map);
        pawn.ageTracker.AgeBiologicalTicks = sourcePawn.ageTracker.AgeBiologicalTicks;
        pawn.ageTracker.BirthAbsTicks = sourcePawn.ageTracker.BirthAbsTicks;

        pawn.needs.SetInitialLevels();
        if (pawn.RaceProps.Humanlike)
        {
            string headGraphicPath = sourcePawn.story.headType.graphicPath;
            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pawn.story, headGraphicPath);
            pawn.story.skinColorOverride = sourcePawn.story.SkinColor;
            pawn.story.headType = sourcePawn.story.headType;
            pawn.story.HairColor = sourcePawn.story.HairColor;
            var name = sourcePawn.Name as NameTriple;
            pawn.Name = name;
            pawn.story.Childhood = sourcePawn.story.Childhood;
            pawn.story.Adulthood = sourcePawn.story.Adulthood;
            pawn.story.hairDef = sourcePawn.story.hairDef;
            foreach (var current in sourcePawn.story.traits.allTraits) pawn.story.traits.GainTrait(current);
            //pawn.story.GenerateSkillsFromBackstory();
        }

        GenerateApparelFromSource(pawn, sourcePawn);
        var con = new PawnGenerationRequest();
        PawnInventoryGenerator.GenerateInventoryFor(pawn, con);
        var nakedBodyGraphic = GraphicDatabase.Get<Graphic_Multi>(sourcePawn.story.bodyType.bodyNakedGraphicPath,
            ShaderDatabase.CutoutSkin, Vector2.one, sourcePawn.story.SkinColor);
        var headGraphic = sourcePawn.story.headType.GetGraphic(sourcePawn.story.SkinColor);
        var hairGraphic = GraphicDatabase.Get<Graphic_Multi>(sourcePawn.story.hairDef.texPath, ShaderDatabase.Cutout,
            Vector2.one, sourcePawn.story.HairColor);
        pawn.Drawer.renderer.graphics.headGraphic = headGraphic;
        pawn.Drawer.renderer.graphics.nakedGraphic = nakedBodyGraphic;
        pawn.Drawer.renderer.graphics.hairGraphic = hairGraphic;
        return pawn;
    }

    //static public void GenerateRandomAge(Pawn pawn, Map map)
    //{
    //    int num = 0;
    //    int num2;
    //    do
    //    {
    //        if (pawn.RaceProps.ageGenerationCurve != null)
    //        {
    //            num2 = Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve, 200));
    //        }
    //        else if (pawn.RaceProps.IsMechanoid)
    //        {
    //            num2 = Rand.Range(0, 2500);
    //        }
    //        else
    //        {
    //            if (!pawn.RaceProps.Animal)
    //            {
    //                goto IL_84;
    //            }
    //            num2 = Rand.Range(1, 10);
    //        }
    //        num++;
    //        if (num > 100)
    //        {
    //            goto IL_95;
    //        }
    //    }
    //    while (num2 > pawn.kindDef.maxGenerationAge || num2 < pawn.kindDef.minGenerationAge);
    //    goto IL_A5;
    //    IL_84:
    //    Log.Warning("Didn't get age for " + pawn);
    //    return;
    //    IL_95:
    //    Log.Error("Tried 100 times to generate age for " + pawn);
    //    IL_A5:
    //    pawn.ageTracker.AgeBiologicalTicks = ((long)(num2 * 3600000f) + Rand.Range(0, 3600000));
    //    int num3;
    //    if (Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
    //    {
    //        float value = Rand.Value;
    //        if (value < 0.7f)
    //        {
    //            num3 = Rand.Range(0, 100);
    //        }
    //        else if (value < 0.95f)
    //        {
    //            num3 = Rand.Range(100, 1000);
    //        }
    //        else
    //        {
    //            int num4 = GenLocalDate.Year(map) - 2026 - pawn.ageTracker.AgeBiologicalYears;
    //            num3 = Rand.Range(1000, num4);
    //        }
    //    }
    //    else
    //    {
    //        num3 = 0;
    //    }
    //    long num5 = GenTicks.TicksAbs - pawn.ageTracker.AgeBiologicalTicks;
    //    num5 -= num3 * 3600000L;
    //    pawn.ageTracker.BirthAbsTicks = num5;
    //    if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
    //    {
    //        pawn.ageTracker.AgeChronologicalTicks = (pawn.ageTracker.AgeBiologicalTicks);
    //    }
    //}


    // More of Justin C's work. I can't take credit for this.
    // Verse.ZombieMod_Utility
    public static void GenerateApparelFromSource(Pawn newPawn, Pawn sourcePawn)
    {
        if (sourcePawn.apparel == null || sourcePawn.apparel.WornApparelCount == 0) return;
        foreach (var current in sourcePawn.apparel.WornApparel)
        {
            Apparel apparel;
            if (current.def.MadeFromStuff)
                apparel = (Apparel)ThingMaker.MakeThing(current.def, current.Stuff);
            else
                apparel = (Apparel)ThingMaker.MakeThing(current.def);
            apparel.DrawColor = new Color(current.DrawColor.r, current.DrawColor.g, current.DrawColor.b,
                current.DrawColor.a);
            newPawn.apparel.Wear(apparel);
        }
    }
}