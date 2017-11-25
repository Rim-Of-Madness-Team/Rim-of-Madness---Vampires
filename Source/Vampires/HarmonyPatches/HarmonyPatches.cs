using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;
using RimWorld.Planet;
using Verse.AI.Group;
using AbilityUser;

namespace Vampire
{
    [StaticConstructorOnStartup]
    static partial class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.vampire");

            //Fixes issues with having no food need.
            harmony.Patch(AccessTools.Method(typeof(Pawn_NeedsTracker), "ShouldHaveNeed"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(ShouldHaveNeed_Vamp))), null);
            harmony.Patch(AccessTools.Method(typeof(ThinkNode_ConditionalNeedPercentageAbove), "Satisfied"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(Satisfied_Vamp))), null);

            //The wander handler now makes vampires wander indoors (for their safety).
            harmony.Patch(AccessTools.Method(typeof(PawnUtility), "KnownDangerAt"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(KnownDangerAt_Vamp))), null);
            //harmony.Patch(AccessTools.Method(typeof(JobGiver_Wander), "GetExactWanderDest"),
            //new HarmonyMethod(typeof(HarmonyPatches), (nameof(GetExactWanderDest_Vamp))), null);
            //harmony.Patch(AccessTools.Method(typeof(JobGiver_WanderColony), "GetWanderRoot"),
            //new HarmonyMethod(typeof(HarmonyPatches), (nameof(GetWanderRoot_Vamp))), null);
            harmony.Patch(AccessTools.Method(typeof(JoyUtility), "EnjoyableOutsideNow", new Type[] { typeof(Pawn), typeof(StringBuilder) }), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(EnjoyableOutsideNow_Vampire))), null);
            harmony.Patch(AccessTools.Method(typeof(JobGiver_GetRest), "FindGroundSleepSpotFor"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(FindGroundSleepSpotFor_Vampire))), null);
            harmony.Patch(AccessTools.Method(typeof(JobGiver_TakeCombatEnhancingDrug), "TryGiveJob"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(TryGiveJob_DrugGiver_Vampire))), null);
            harmony.Patch(AccessTools.Method(typeof(ReachabilityUtility), "CanReach", new Type[] { typeof(Pawn), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(Danger), typeof(bool), typeof(TraverseMode) }), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(CanReach_Vampire))), null);

            //The Doctor alert will no longer check a vampire to see if it's fed.
            harmony.Patch(AccessTools.Method(typeof(Alert_NeedDoctor), "get_Patients"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(get_Patients_Vamp))), null);

            //Vampires vomit blood instead of their digested meals.
            harmony.Patch(AccessTools.Method(typeof(JobDriver_Vomit), "MakeNewToils"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(MakeNewToils_VampVomit))), null);

            //Adds vampire right click float menus.
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(AddHumanlikeOrders_Vamp))), null);

            //Gives different skin color for Vampires
            harmony.Patch(AccessTools.Method(typeof(Pawn_StoryTracker), "get_SkinColor"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(get_SkinColor_Vamp))), null);

            //Adds debug/dev tools for making vampires.
            harmony.Patch(AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DoListingItems_MapTools"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(DoListingItems_MapTools_Vamp))), null);

            //Adds blood extraction recipes to all living organisms
            harmony.Patch(AccessTools.Method(typeof(ThingDef), "get_AllRecipes"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(get_AllRecipes_BloodFeedable))), null);

            //Adds blood extraction recipes to all living organisms
            harmony.Patch(AccessTools.Method(typeof(Bill_Medical), "Notify_DoBillStarted"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(Notify_DoBillStarted_Debug))), null);

            //Add overrides to methods if CompVampBed is active.
            harmony.Patch(AccessTools.Method(typeof(Building_Casket), "Draw"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(Draw_VampBed))), null);
            harmony.Patch(AccessTools.Method(typeof(Building_Casket), "Accepts"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(Accepts_VampBed))), null);
            harmony.Patch(AccessTools.Method(typeof(Building_Grave), "get_Graphic"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(get_Graphic_VampBed))), null);
            harmony.Patch(AccessTools.Method(typeof(Building_Casket), "GetFloatMenuOptions"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(GetFloatMenuOptions_VampBed))), null);
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_BuryCorpses), "FindBestGrave"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(FindBestGrave_VampBed))), null);

            //Caravan patches
            harmony.Patch(AccessTools.Method(typeof(Dialog_FormCaravan), "CheckForErrors"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(CheckForErrors_Vampires))), null);
            harmony.Patch(AccessTools.Method(typeof(Caravan), "get_Resting"), null,
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(get_Resting_Vampires))), null);

            //Lord_AI patches
            harmony.Patch(AccessTools.Method(typeof(Trigger_UrgentlyHungry), "ActivateOn"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(ActivateOn_Vampire))), null);

            //Allow fortitude to soak damage
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), "PreApplyDamage"),
                new HarmonyMethod(typeof(HarmonyPatches), (nameof(VampFortitude))), null);

            //Changes vampire appearances and statistics based on their current forms
            harmony.Patch(AccessTools.Method(typeof(Pawn), "get_BodySize"), null, new HarmonyMethod(typeof(HarmonyPatches),
                nameof(VampireBodySize)), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn), "get_HealthScale"), null, new HarmonyMethod(typeof(HarmonyPatches),
                nameof(VampireHealthScale)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics"), null, new HarmonyMethod(typeof(HarmonyPatches),
                nameof(Vamp_ResolveAllGraphics)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics"), new HarmonyMethod(typeof(HarmonyPatches),
                nameof(Vamp_ResolveApparelGraphics)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) }), new HarmonyMethod(typeof(VampireGraphicUtility),
                nameof(VampireGraphicUtility.RenderVampire)), null);

            //Allows skill adjustments
            harmony.Patch(AccessTools.Method(typeof(SkillRecord), "get_Level"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampLevel)), null);

            //Vampires do not age like others.
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), "BirthdayBiological"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampireBirthdayBiological)), null);
            //Nor do they suffer health effects as they age.
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("AgeInjuryUtility"), "GenerateRandomOldAgeInjuries"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_GenerateRandomOldAgeInjuries)), null);


            //Adds vampire skill sheet button to CharacterCard
            harmony.Patch(AccessTools.Method(typeof(CharacterCardUtility), "DrawCharacterCard", new Type[] { typeof(Rect), typeof(Pawn), typeof(Action), typeof(Rect) }), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DrawCharacterCard)), null);
            //Fills the character card with a vampire skill sheet
            harmony.Patch(AccessTools.Method(typeof(ITab_Pawn_Character), "FillTab"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_FillTab)), null);

            harmony.Patch(AccessTools.Method(typeof(Verb_Shoot), "TryCastShot"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_TryCastShot)), null);

            harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "TryCastShot"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_TryCastShot)), null);

            // Add XP every time a pawn learns a skill.
            harmony.Patch(AccessTools.Method(typeof(SkillRecord), "Learn"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Learn_PostFix")));

            // RimWorld.SickPawnVisitUtility
            harmony.Patch(AccessTools.Method(typeof(SickPawnVisitUtility), "CanVisit"), new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_CanVisit)), null);

            //Adds blood shield
            harmony.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("GetGizmos_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawEquipment"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("DrawEquipment_PostFix")));

            harmony.Patch(AccessTools.Method(typeof(ForbidUtility), "IsForbidden", new Type[] { typeof(IntVec3), typeof(Pawn) }), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IsForbidden)));

            //Patches so that wardens do not try to feed vampires
            harmony.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), "get_CanBeBroughtFood"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_WardensDontFeedVamps)));


            //Patches to remove vampires from daylight raids.
            harmony.Patch(AccessTools.Method(typeof(Scenario), "Notify_PawnGenerated"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DontGenerateVampsInDaylight)));

            //Makes vampires use one blood point to be forced awake from slumber.
            harmony.Patch(AccessTools.Method(typeof(Pawn_JobTracker), "EndCurrentJob"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_EndCurrentJob)), null);

            //Patch to add comfort to vampire beds.
            harmony.Patch(AccessTools.Method(typeof(PawnUtility), "GainComfortFromCellIfPossible"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_BedComfort)), null);

            //Patch to remove vampire's ability to bleed.
            harmony.Patch(AccessTools.Method(typeof(Hediff_Injury), "get_BleedRate"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(get_VampBleedRate)), null);

            //Patch to hide vampire capacities.
            //harmony.Patch(AccessTools.Method(typeof(PawnCapacitiesHandler), "GetLevel"), null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_HidePawnCapacities)), null);
            harmony.Patch(AccessTools.Method(typeof(HealthCardUtility), "GetPawnCapacityTip"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_GetPawnCapacityTip)), null);
            harmony.Patch(AccessTools.Method(typeof(HealthCardUtility), "GetEfficiencyLabel"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(GetEfficiencyLabel)), null);


            //Vampires do not worry about hot and cold
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Hot), "CurrentStateInternal"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IgnoreHotAndCold)), null);
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Cold), "CurrentStateInternal"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IgnoreHotAndCold)), null);

            //Vampires are not affected by Hypothermia nor Heatstroke
            harmony.Patch(AccessTools.Method(typeof(HediffGiver_Heat), "OnIntervalPassed"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IgnoreStrokeAndHypotherm)), null);
            harmony.Patch(AccessTools.Method(typeof(HediffGiver_Hypothermia), "OnIntervalPassed"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IgnoreStrokeAndHypotherm)), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), "AddHediff", new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?) }),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHediff)), null);

            //Vampires do not make breath motes
            harmony.Patch(AccessTools.Method(typeof(PawnBreathMoteMaker), "BreathMoteMakerTick"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_NoBreathingMote)), null);


            //Vampires had trouble with lovin' due to a food check.
            harmony.Patch(AccessTools.Method(typeof(LovePartnerRelationUtility), "GetLovinMtbHours"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_LovinFoodFix)), null);
            //Guests were also checking for "food" related items.
            harmony.Patch(AccessTools.Method(typeof(GatheringsUtility), "ShouldGuestKeepAttendingGathering"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_GuestFix)), null);
            //More food checks
            harmony.Patch(AccessTools.Method(typeof(JobGiver_EatInPartyArea), "TryGiveJob"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DontEatAtTheParty)), null);

            //Players can't slaughter temporary summons
            harmony.Patch(AccessTools.Method(typeof(Designator_Slaughter), "CanDesignateThing"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_CantSlaughterTemps)), null);

            //Vampires should not worry about sleeping in the same coffin.
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover), "CurrentStateInternal"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_FineSleepingAlone)), null);

            //Vampire corpses can resurrect themselves.
            harmony.Patch(AccessTools.Method(typeof(ThingWithComps), "GetGizmos"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_TheyNeverDie)), null);

            //Vampires should not dislike the darkness.
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Dark), "CurrentStateInternal"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_TheyDontDislikeDarkness)), null);


            //Fixes random red errors relating to food need checks in this method (WillIngestStackCountOf).
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "WillIngestStackCountOf"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_WillIngestStackCountOf)), null);


            //Vampires should tire very much during the daylight hours.
            harmony.Patch(AccessTools.Method(typeof(Need_Rest), "NeedInterval"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_SleepyDuringDaylight)), null);



            #region DubsBadHygiene
            {
                try
                {
                    ((Action)(() =>
                    {
                        if (AccessTools.Method(typeof(DubsBadHygiene.Need_Bladder), nameof(DubsBadHygiene.Need_Bladder.crapPants)) != null)
                        {
                            harmony.Patch(AccessTools.Method(typeof(Pawn_NeedsTracker), "ShouldHaveNeed"),
                                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_NoBladderNeed)), null);
                        }
                    })).Invoke();
                }
                catch (TypeLoadException ex) { /*Log.Message(ex.ToString());*/ }
            }
            #endregion
        }

        // RimWorld.Need_Rest
        public static void Vamp_SleepyDuringDaylight(Need_Rest __instance)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Need_Rest), "pawn").GetValue(__instance);
            if (pawn != null && pawn.IsVampire())
            {
                if (VampireUtility.IsDaylight(pawn))
                {
                    __instance.CurLevel = Mathf.Max(0.1f, __instance.CurLevel);
                }
                else
                {
                    __instance.CurLevel = 1.0f;
                }
            }
        }


        // RimWorld.FoodUtility
        public static bool Vamp_WillIngestStackCountOf(Pawn ingester, ThingDef def, ref int __result)
        {
            if (ingester.IsVampire())
            {
                __result = Mathf.Min(def.ingestible.maxNumToIngestAtOnce, 1);
                return false;
            }
            return true;
        }

                //ThoughtWorker_Dark
                public static void Vamp_TheyDontDislikeDarkness(Pawn p, ref ThoughtState __result)
        {
            bool temp = __result.Active;
            __result = temp && !p.IsVampire();
        }


        //ThingWithComps
        public static void Vamp_TheyNeverDie(ThingWithComps __instance, ref IEnumerable<Gizmo> __result)
        {

            //Log.Message("4");
            if (__instance is Corpse c && c.InnerPawn is Pawn p)
            {
                if (p.Faction == Faction.OfPlayer && p.IsVampire())
                {
                    __result = __result.Concat(GizmoGetter(p));
                }
            }

        }

        public static IEnumerable<Gizmo> GizmoGetter(Pawn AbilityUser)
        {
            Vampire.VitaeAbilityDef bloodResurrection = DefDatabase<Vampire.VitaeAbilityDef>.GetNamedSilentFail("ROMV_VampiricResurrection");
            if (AbilityUser?.Corpse?.GetRotStage() < RotStage.Dessicated)
            {
                yield return new Command_Action()
                {
                    defaultLabel = bloodResurrection.label,
                    defaultDesc = bloodResurrection.GetDescription(),
                    icon = bloodResurrection.uiIcon,
                    action = delegate
                    {
                        AbilityUser.Drawer.Notify_DebugAffected();
                        ResurrectionUtility.Resurrect(AbilityUser);
                        MoteMaker.ThrowText(AbilityUser.PositionHeld.ToVector3(), AbilityUser.MapHeld, StringsToTranslate.AU_CastSuccess, -1f);
                        AbilityUser.BloodNeed().AdjustBlood(-99999999);
                        HealthUtility.AdjustSeverity(AbilityUser, VampDefOf.ROMV_TheBeast, 1.0f);
                        MentalStateDef MentalState_VampireBeast = DefDatabase<MentalStateDef>.GetNamed("ROMV_VampireBeast");
                        AbilityUser.mindState.mentalStateHandler.TryStartMentalState(MentalState_VampireBeast, null, true, false, null);
                    },
                    disabled = false
                };
            }
        }

        // RimWorld.ThoughtWorker_WantToSleepWithSpouseOrLover
        public static void Vamp_FineSleepingAlone(Pawn p, ref ThoughtState __result)
        {
            if (p != null && p.IsVampire())
                __result = false;
        }

            // RimWorld.Designator_Slaughter
            public static bool Vamp_CantSlaughterTemps(Thing t, ref AcceptanceReport __result)
        {
            if (t is PawnTemporary)
            {
                __result = false;
                return false;
            }
            return true;
        }

            //public class JobGiver_EatInPartyArea : ThinkNode_JobGiver
            //{
            public static bool Vamp_DontEatAtTheParty(Pawn pawn, ref Job __result)
        {
            if (pawn.IsVampire())
            {
                __result = null;
                return false;
            }
            return true;
        }

                // RimWorld.GatheringsUtility
                public static bool Vamp_GuestFix(Pawn p, ref bool __result)
        {
            if (p.IsVampire())
            {
                __result = !p.Downed && p.health.hediffSet.BleedRateTotal <= 0f && p?.needs?.rest?.CurCategory < RestCategory.Exhausted &&
                !p.health.hediffSet.HasTendableNonInjuryNonMissingPartHediff(false) && p.Awake() && !p.InAggroMentalState && !p.IsPrisoner;
                return false;
            }
            return true;
        }


        // RimWorld.LovePartnerRelationUtility
        private static float LovinMtbSinglePawnFactor(Pawn pawn)
        {
            float num = 1f;
            num /= 1f - pawn.health.hediffSet.PainTotal;
            float level = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
            if (level < 0.5f)
            {
                num /= level * 2f;
            }
            return num / GenMath.FlatHill(0f, 14f, 16f, 25f, 80f, 0.2f, pawn.ageTracker.AgeBiologicalYearsFloat);
        }


        // RimWorld.LovePartnerRelationUtility
        public static bool Vamp_LovinFoodFix(Pawn pawn, Pawn partner, ref float __result)
        {
            if (pawn.IsVampire() || partner.IsVampire())
            {
                if (pawn.Dead || partner.Dead)
                {
                    __result = -1f;
                    return false;
                }
                if (DebugSettings.alwaysDoLovin)
                {
                    __result = 0.1f;
                    return false;
                }
                if (pawn?.needs?.food is Need_Food food && food.Starving || partner?.needs?.food is Need_Food foodPartner && foodPartner.Starving)
                {
                    __result = -1f;
                    return false;
                }
                if (pawn.health.hediffSet.BleedRateTotal > 0f || partner.health.hediffSet.BleedRateTotal > 0f)
                {
                    __result = -1f;
                    return false;
                }
                float num = HarmonyPatches.LovinMtbSinglePawnFactor(pawn);
                if (num <= 0f)
                {
                    __result = -1f;
                    return false;
                }
                float num2 = HarmonyPatches.LovinMtbSinglePawnFactor(partner);
                if (num2 <= 0f)
                {
                    __result = -1f;
                    return false;
                }
                float num3 = 12f;
                num3 *= num;
                num3 *= num2;
                num3 /= Mathf.Max(pawn.relations.SecondaryRomanceChanceFactor(partner), 0.1f);
                num3 /= Mathf.Max(partner.relations.SecondaryRomanceChanceFactor(pawn), 0.1f);
                num3 *= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, (float)pawn.relations.OpinionOf(partner));
                __result = num3 * GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, (float)partner.relations.OpinionOf(pawn));
                return false;
            }
            return true;

        }


        // Verse.HealthUtility
        // Verse.Pawn_HealthTracker
        public static bool AddHediff(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_HealthTracker), "pawn").GetValue(__instance);

            if (pawn != null && !pawn.Dead && hediff.def is HediffDef hdDef)
            {
                if (pawn.IsVampire())
                {
                    if (hediff is Hediff_MissingPart missingPart)
                    {
                        missingPart.IsFresh = false;
                    }

                    if (hdDef == HediffDefOf.CryptosleepSickness ||
                        hdDef == HediffDefOf.Flu ||
                        hdDef == HediffDefOf.Heatstroke ||
                        hdDef == HediffDefOf.Hypothermia ||
                        hdDef == HediffDefOf.Malaria ||
                        hdDef == HediffDefOf.ToxicBuildup ||
                        hdDef == HediffDefOf.WoundInfection ||
                        hdDef == HediffDefOf.Plague ||
                        hediff is Hediff_HeartAttack)
                    {
                        if (pawn?.health?.hediffSet?.GetFirstHediffOfDef(hdDef) is Hediff hd)
                        {
                            pawn.health.hediffSet.hediffs.Remove(hd);
                        }
                        return false;
                    }
                }
                if (hediff is HediffVampirism v)
                {
                    if (pawn.MapHeld != null && VampireUtility.IsDaylight(pawn.MapHeld) && pawn.Faction != Faction.OfPlayerSilentFail)
                    {
                        if (pawn?.health?.hediffSet?.GetFirstHediffOfDef(hdDef) is Hediff hd)
                        {
                            pawn.health.hediffSet.hediffs.Remove(hd);
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        // RimWorld.HealthCardUtility
        public static void GetEfficiencyLabel(ref Pair<string, Color> __result, Pawn pawn, PawnCapacityDef activity)
        {
            if (pawn.IsVampire() &&
               (
               activity == PawnCapacityDefOf.Breathing ||
               activity == PawnCapacityDefOf.BloodPumping ||
               activity == PawnCapacityDefOf.BloodFiltration ||
               activity == PawnCapacityDefOf.Eating ||
               activity == PawnCapacityDefOf.Metabolism))
            {
                __result = new Pair<string, Color>("ROMV_HI_Unused".Translate(), VampireUtility.VampColor);
            }
        }

        // Verse.HediffGiver_Heat
        public static void Vamp_IgnoreStrokeAndHypotherm(Pawn pawn, Hediff cause)
        {
            if (cause?.def == HediffDefOf.Hypothermia ||
                cause?.def == HediffDefOf.Heatstroke)
            {
                if (pawn?.health?.hediffSet?.GetFirstHediffOfDef(cause.def) is Hediff h)
                {
                    pawn.health.RemoveHediff(h);
                }
            }
        }

        // RimWorld.PawnBreathMoteMaker
        public static bool Vamp_NoBreathingMote(PawnBreathMoteMaker __instance)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(PawnBreathMoteMaker), "pawn").GetValue(__instance);
            if (pawn.IsVampire())
            {
                return false;
            }
            return true;
        }

        //ThoughtWorker_Hot
        public static void Vamp_IgnoreHotAndCold(Pawn p, ref ThoughtState __result)
        {
            if (p != null && p.IsVampire())
            {
                __result = ThoughtState.Inactive;
            }
        }

        // RimWorld.HealthCardUtility
        public static void Vamp_GetPawnCapacityTip(Pawn pawn, PawnCapacityDef capacity, ref string __result)
        {
            if (pawn.IsVampire() &&
                (
                capacity == PawnCapacityDefOf.Breathing ||
                capacity == PawnCapacityDefOf.BloodPumping ||
                capacity == PawnCapacityDefOf.BloodFiltration ||
                capacity == PawnCapacityDefOf.Eating ||
                capacity == PawnCapacityDefOf.Metabolism))
            {
                StringBuilder s = new StringBuilder();
                s.AppendLine(capacity.LabelCap + ": 0%");
                s.AppendLine();
                s.AppendLine("AffectedBy".Translate());
                s.AppendLine("  " + "ROMV_HI_Vampirism".Translate());
                s.AppendLine("  " + "ROMV_HI_UnusedCapacities".Translate().AdjustedFor(pawn));
                __result = s.ToString();

            }
        }


        //// Verse.PawnCapacitiesHandler
        //public static void Vamp_HidePawnCapacities(PawnCapacitiesHandler __instance, PawnCapacityDef capacity, ref float __result)
        //{
        //    Pawn pawn = (Pawn)AccessTools.Field(typeof(PawnCapacitiesHandler), "pawn").GetValue(__instance);
        //    if (pawn.IsVampire() &&
        //        (
        //        capacity == PawnCapacityDefOf.Breathing ||
        //        capacity == PawnCapacityDefOf.BloodPumping ||
        //        capacity == PawnCapacityDefOf.BloodFiltration ||
        //        capacity == PawnCapacityDefOf.Eating ||
        //        capacity == PawnCapacityDefOf.Metabolism))
        //        {
        //        __result = 0f;

        //    }

        //}


        // Verse.Hediff_Injury
        public static void get_VampBleedRate(Hediff_Injury __instance, ref float __result)
        {
            if (__instance.pawn is Pawn p && p.IsVampire())
            {
                __result = 0f;
            }
        }

        // RimWorld.PawnUtility
        public static void Vamp_BedComfort(Pawn p)
        {
            if (Find.TickManager.TicksGame % 10 == 0)
            {
                Building edifice = p.Position.GetEdifice(p.Map);
                if (edifice != null)
                {
                    if (edifice.TryGetComp<CompVampBed>() is CompVampBed vBed && vBed.Bed != null)
                    {
                        float statValue = vBed.Bed.GetStatValue(StatDefOf.Comfort, true);
                        if (statValue >= 0f && p.needs != null && p.needs.comfort != null)
                        {
                            p.needs.comfort.ComfortUsed(statValue);
                        }
                    }
                }
            }
        }

        // Verse.AI.Pawn_JobTracker
        public static void Vamp_EndCurrentJob(Pawn_JobTracker __instance, JobCondition condition, bool startNewJob)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_JobTracker), "pawn").GetValue(__instance);
            if (pawn.IsVampire())
            {
                if (__instance.curJob != null && __instance.curDriver.layingDown != LayingDownState.NotLaying && !pawn.Downed)
                {
                    if (pawn.BloodNeed() is Need_Blood bN)
                    {
                        bN.AdjustBlood(-1);
                    }
                    else
                    {
                        Log.Warning("Vampires :: Failed to show blood need.");
                    }
                }
            }
        }

        // RimWorld.Scenario
        public static void Vamp_DontGenerateVampsInDaylight(Scenario __instance, Pawn pawn, PawnGenerationContext context)
        {
            if (pawn.IsVampire() && VampireUtility.IsDaylight(pawn) && pawn.Faction != Faction.OfPlayerSilentFail &&
                pawn?.health?.hediffSet?.hediffs is List<Hediff> hdiffs)
            {
                hdiffs.RemoveAll(x => x.def == VampDefOf.ROM_Vampirism);
            }
        }


        // RimWorld.Pawn_GuestTracker
        public static void Vamp_WardensDontFeedVamps(Pawn_GuestTracker __instance, ref bool __result)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_GuestTracker), "pawn").GetValue(__instance);
            if (pawn.IsVampire())
            {
                __result = false;
            }
        }


        // RimWorld.Pawn_NeedsTracker
        private static void Vamp_FullBladder(Pawn_NeedsTracker __instance, ref float __result)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_NeedsTracker), "pawn").GetValue(__instance);
            if (pawn.IsVampire())
            {
                __result = 1.0f;
            }
        }

        // RimWorld.Pawn_NeedsTracker
        private static bool Vamp_NoBladderNeed(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_NeedsTracker), "pawn").GetValue(__instance);
            if (pawn.IsVampire())
            {
                if (nd.defName == "Bladder")
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }


        // DubsBadHygiene.dubUtils
        public static void Vamp_StopThePoopStorm(Pawn pawn, ref bool __result)
        {
            if (pawn.IsVampire())
            {
                __result = true;
            }
        }

        // RimWorld.ForbidUtility
        public static void Vamp_StopThePoopStorm(IntVec3 c, Pawn pawn, ref bool __result)
        {
            if (pawn.IsVampire() && VampireUtility.IsDaylight(pawn) && !c.Roofed(pawn.Map))
                __result = true;
        }

        // RimWorld.ForbidUtility
        public static void Vamp_IsForbidden(IntVec3 c, Pawn pawn, ref bool __result)
        {
            if (pawn.IsVampire() && VampireUtility.IsDaylight(pawn) && !c.Roofed(pawn.Map))
                __result = true;
        }

        // Verse.PawnRenderer
        public static void DrawEquipment_PostFix(PawnRenderer __instance, Vector3 rootLoc)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(PawnRenderer), "pawn").GetValue(__instance);
            if (pawn.health != null)
            {
                if (pawn.health.hediffSet != null)
                {
                    if (pawn.health.hediffSet.hediffs != null && pawn.health.hediffSet.hediffs.Count > 0)
                    {
                        Hediff shieldHediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.TryGetComp<HediffComp_Shield>() != null);
                        if (shieldHediff != null)
                        {
                            HediffComp_Shield shield = shieldHediff.TryGetComp<HediffComp_Shield>();
                            if (shield != null)
                            {
                                shield.DrawWornExtras();
                            }
                        }
                    }
                }
            }

        }

        public static IEnumerable<Gizmo> gizmoGetter(HediffComp_Shield compHediffShield)
        {
            if (compHediffShield.GetWornGizmos() != null)
            {
                IEnumerator<Gizmo> enumerator = compHediffShield.GetWornGizmos().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Gizmo current = enumerator.Current;
                    yield return current;
                }
            }
        }


        public static void GetGizmos_PostFix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            Pawn pawn = __instance;
            if (pawn.health != null)
            {
                if (pawn.health.hediffSet != null)
                {
                    if (pawn.health.hediffSet.hediffs != null && pawn.health.hediffSet.hediffs.Count > 0)
                    {
                        Hediff shieldHediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.TryGetComp<HediffComp_Shield>() != null);
                        if (shieldHediff != null)
                        {
                            HediffComp_Shield shield = shieldHediff.TryGetComp<HediffComp_Shield>();
                            if (shield != null)
                            {
                                __result = __result.Concat<Gizmo>(gizmoGetter(shield));
                            }
                        }
                    }
                }
            }
        }


        // RimWorld.SickPawnVisitUtility
        public static bool Vamp_CanVisit(ref bool __result, Pawn pawn, Pawn sick, JoyCategory maxPatientJoy)
        {
            if (sick.IsVampire())
            {
                __result = sick.IsColonist && !sick.Dead && pawn != sick && sick.InBed() && sick.Awake() && !sick.IsForbidden(pawn) && sick.needs.joy != null && 
                    sick.needs.joy.CurCategory <= maxPatientJoy && InteractionUtility.CanReceiveInteraction(sick) && 
                    pawn.CanReserveAndReach(sick, PathEndMode.InteractionCell, Danger.None, 1, -1, null, false) && !HarmonyPatches.AboutToRecover(sick);
                return false;
            }
            return true; 
        }

        // RimWorld.SickPawnVisitUtility
        private static bool AboutToRecover(Pawn pawn)
        {
            if (pawn.Downed)
            {
                return false;
            }
            if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) && !HealthAIUtility.ShouldSeekMedicalRest(pawn))
            {
                return true;
            }
            if (pawn.health.hediffSet.HasTendedImmunizableNotImmuneHediff())
            {
                return false;
            }
            float num = 0f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff_Injury hediff_Injury = hediffs[i] as Hediff_Injury;
                if (hediff_Injury != null && (hediff_Injury.CanHealFromTending() || hediff_Injury.CanHealNaturally() || hediff_Injury.Bleeding))
                {
                    num += hediff_Injury.Severity;
                }
            }
            return num < 8f * pawn.RaceProps.baseHealthScale;
        }


        // Verse.Verb_Shoot
        public static bool Vamp_TryCastShot(Verb_Shoot __instance, ref bool __result)
        {
            if (__instance?.CasterPawn is Pawn p && p.IsVampire())
            {
                if (__instance.CasterPawn.health.hediffSet.hediffs.FirstOrDefault(x => x.TryGetComp<HediffComp_AnimalForm>() != null) is HediffWithComps ht && ht.TryGetComp<HediffComp_AnimalForm>() is HediffComp_AnimalForm af && !af.Props.canGiveDamage)
                {
                    __instance.CasterPawn.health.hediffSet.hediffs.Remove(ht);
                    __instance.CasterPawn.VampComp().CurrentForm = null;
                    __instance.CasterPawn.VampComp().CurFormGraphic = null;
                    __instance.CasterPawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }
            }
            return true;
        }

        public static void Learn_PostFix(SkillRecord __instance, float xp, bool direct = false)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(SkillRecord), "pawn").GetValue(__instance);
            if (xp > 0 && pawn.TryGetComp<CompVampire>() is CompVampire compVamp &&
                Find.TickManager.TicksGame > compVamp.ticksToLearnXP)
            {
                int delay = 132;
                if (__instance.def == SkillDefOf.Intellectual || __instance.def == SkillDefOf.Growing) delay += 52;
                compVamp.ticksToLearnXP = Find.TickManager.TicksGame + delay;
                //Log.Message("XP");
                compVamp.XP++;
            }
        }

        // Verse.PawnGraphicSet
        public static bool Vamp_ResolveApparelGraphics(PawnGraphicSet __instance)
        {
            if (__instance.pawn.VampComp() is CompVampire v && v.Transformed)
            {
                __instance.ClearCache();
                __instance.apparelGraphics.Clear();
                return false;
            }
            return true;
        }

        // RimWorld.AgeInjuryUtility
        public static bool Vamp_GenerateRandomOldAgeInjuries(Pawn pawn, bool tryNotToKillPawn)
        {
            if (pawn.IsVampire())
            {
                return false;
            }
            return true;
        }

        public static void Vamp_ResolveAllGraphics(PawnGraphicSet __instance)
        {
            if (__instance?.pawn?.VampComp() is CompVampire v && v.IsVampire && !v.Transformed)
            {
                if (v?.Bloodline?.nakedBodyGraphicsPath != "")
                {
                    Graphic newBodyGraphic = VampireGraphicUtility.GetNakedBodyGraphic(__instance.pawn, __instance.pawn.story.bodyType, ShaderDatabase.CutoutSkin, __instance.pawn.story.SkinColor);
                    if (newBodyGraphic != null)
                        __instance.nakedGraphic = newBodyGraphic;
                }
                if (v?.Bloodline?.headGraphicsPath != "")
                {
                    string headPath = VampireGraphicUtility.GetHeadGraphicPath(__instance.pawn);
                    Graphic newHeadGraphic = VampireGraphicUtility.GetVampireHead(__instance.pawn, headPath, __instance.pawn.story.SkinColor);
                    if (newHeadGraphic != null)
                        __instance.headGraphic = newHeadGraphic;
                }
                __instance.ResolveApparelGraphics();
            }
        }

        // RimWorld.CharacterCardUtility
        public static bool isSwitched = false;
        // RimWorld.ITab_Pawn_Character
        public static bool Vamp_FillTab(ITab_Pawn_Character __instance)
        {
            Pawn p = (Pawn)AccessTools.Method(typeof(ITab_Pawn_Character), "get_PawnToShowInfoAbout").Invoke(__instance, null);
            if (p.IsVampire())
            {
                Rect rect = new Rect(17f, 17f, CharacterCardUtility.PawnCardSize.x, CharacterCardUtility.PawnCardSize.y);
                if (isSwitched)
                    VampireCardUtility.DrawVampCard(rect, p);
                else
                    CharacterCardUtility.DrawCharacterCard(rect, p);
                return false;
            }
            return true;
        }


        public static void Vamp_DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback, Rect creationRect = default(Rect))
        {
            if (pawn.IsVampire())
            {
                bool flag = randomizeCallback != null;
                if (!flag && pawn.IsColonist && !pawn.health.Dead)
                {
                    Rect rect7 = new Rect(CharacterCardUtility.PawnCardSize.x - 140f, 14f, 30f, 30f);
                    TooltipHandler.TipRegion(rect7, new TipSignal("ROMV_VampireSheet".Translate()));
                    if (Widgets.ButtonImage(rect7, TexButton.ROMV_VampireIcon))
                    {
                        isSwitched = true;
                    }
                }
            }
        }


        // Verse.Pawn_AgeTracker
        public static bool VampireBirthdayBiological(Pawn_AgeTracker __instance)
        {
            Pawn p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (p.RaceProps.Humanlike && p.IsVampire() && PawnUtility.ShouldSendNotificationAbout(p))
            {

                Find.LetterStack.ReceiveLetter("LetterLabelBirthday".Translate(), "ROMV_VampireBirthday".Translate(new object[]{
                    p.Label,
                    p.ageTracker.AgeBiologicalYears
                }), LetterDefOf.PositiveEvent, p, null);
                return false;
            }
            return true;
        }


        public static void VampLevel(SkillRecord __instance, ref int __result)
        {
            Pawn p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (!__instance.TotallyDisabled)
            {
                if (p.health.hediffSet.hediffs.FindAll(x => 
                x.TryGetComp<HediffComp_SkillOffset>() is HediffComp_SkillOffset hSkill && hSkill.Props.skillDef == __instance.def) is List<Hediff> hediffs && 
                !hediffs.NullOrEmpty())
                {
                    foreach (Hediff hediff in hediffs)
                    {
                        __result += hediff.TryGetComp<HediffComp_SkillOffset>().Props.offset;
                    }
                }
            }
        }
        
        

        // Verse.Pawn
        public static void VampireBodySize(Pawn __instance, ref float __result)
        {
            if (__instance?.VampComp() is CompVampire v && v.Transformed)
            {
                __result = v.CurrentForm.race.race.baseBodySize;  //Mathf.Clamp((__result * w.CurrentWerewolfForm.def.sizeFactor) + (w.CurrentWerewolfForm.level * 0.1f), __result, __result * (w.CurrentWerewolfForm.def.sizeFactor * 2));
            }
        }

        // Verse.Pawn
        public static void VampireHealthScale(Pawn __instance, ref float __result)
        {
            if (__instance?.VampComp() is CompVampire v && v.Transformed)
            {
                __result = v.CurrentForm.race.race.baseHealthScale;  //Mathf.Clamp((__result * w.CurrentWerewolfForm.def.sizeFactor) + (w.CurrentWerewolfForm.level * 0.1f), __result, __result * (w.CurrentWerewolfForm.def.sizeFactor * 2));
            }
        }

        // Verse.Pawn_HealthTracker
        public static bool VampFortitude(Pawn_HealthTracker __instance, DamageInfo dinfo, out bool absorbed)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_HealthTracker), "pawn").GetValue(__instance);
            if (pawn != null)
            {
                if (pawn is PawnTemporary t)
                {
                    t.Drawer.Notify_DebugAffected();
                    absorbed = true;
                    return false;
                }
                if (pawn.health.hediffSet.hediffs != null && pawn.health.hediffSet.hediffs.Count > 0)
                {
                    Hediff fortitudeHediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.TryGetComp<HediffComp_DamageSoak>() != null);
                    if (fortitudeHediff != null)
                    {
                        HediffComp_DamageSoak soaker = fortitudeHediff.TryGetComp<HediffComp_DamageSoak>();
                        if (soaker != null)
                        {
                            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ROMV_DamageSoaked".Translate(soaker.Props.damageToSoak), -1f);
                            dinfo.SetAmount(Mathf.Max(dinfo.Amount - soaker.Props.damageToSoak, 0));
                        }
                    }
                    if (pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.TryGetComp<HediffComp_ReadMind>() != null) is HediffWithComps h && h.TryGetComp<HediffComp_ReadMind>() is HediffComp_ReadMind rm)
                    {
                        if (rm.MindBeingRead == dinfo.Instigator)
                        {
                            pawn.Drawer.Notify_DebugAffected();
                            absorbed = true;
                            return false;
                        }
                    }
                    if (pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.TryGetComp<HediffComp_AnimalForm>() != null) is HediffWithComps ht && ht.TryGetComp<HediffComp_AnimalForm>().Props.immuneTodamage)
                    {
                        pawn.Drawer.Notify_DebugAffected();
                        absorbed = true;
                        return false;
                    }

                    if (pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.TryGetComp<HediffComp_Shield>() != null) is HediffWithComps htt && htt.TryGetComp<HediffComp_Shield>() is HediffComp_Shield shield)
                    {
                        if (shield.CheckPreAbsorbDamage(dinfo))
                        {
                            absorbed = true;
                            return false;
                        }
                    }
                }
            }
            absorbed = false;
            return true;
        }

        public static bool TryGiveJob_DrugGiver_Vampire(Pawn pawn, ref Job __result)
        {
            if (pawn.IsVampire())
            {
                __result = null;
                return false;
            }
            return true;
        }


        // Verse.AI.Group.Trigger_UrgentlyHungry
        public static bool ActivateOn_Vampire(Lord lord, TriggerSignal signal, ref bool __result)
        {
            if (lord?.ownedPawns?.Any(x => x.IsVampire()) ?? false)
            {
                if (signal.type == TriggerSignalType.Tick)
                {
                    for (int i = 0; i < lord.ownedPawns.Count; i++)
                    {
                        if (lord?.ownedPawns[i]?.needs?.food?.CurCategory >= HungerCategory.UrgentlyHungry)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                __result = false;
                return false;
            }
            return true;
        }

        //WorkGiver_BuryCorpses
        public static void FindBestGrave_VampBed(Pawn p, Corpse corpse, ref Building_Grave __result)
        {
            if (__result != null && __result is Building_Grave g && g?.def.GetCompProperties<CompProperties_VampBed>() is CompProperties_VampBed b)
            {
                Predicate<Thing> predicate = (Thing m) => !m.IsForbidden(p) && p.CanReserve(m, 1, -1, null, false) && m is Building_Grave mG && !mG.HasAnyContents && (mG?.Accepts(corpse) ?? false) && (mG.GetComp<CompVampBed>() == null || mG.GetComp<CompVampBed>() is CompVampBed v && (v?.Bed == null || v?.Bed?.AssignedPawns?.Count() == 0));
                if (corpse?.InnerPawn?.ownership != null && corpse?.InnerPawn?.ownership?.AssignedGrave != null)
                {
                    Building_Grave assignedGrave = corpse?.InnerPawn?.ownership?.AssignedGrave;
                    if (predicate(assignedGrave) && (p?.Map?.reachability?.CanReach(corpse.Position, assignedGrave, PathEndMode.ClosestTouch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false)) ?? false))
                    {
                        __result = assignedGrave;
                        return;
                    }
                }
                Func<Thing, float> priorityGetter = (Thing t) => (float)((IStoreSettingsParent)t).GetStoreSettings().Priority;
                Predicate<Thing> validator = predicate;
                __result = (Building_Grave)GenClosest.ClosestThing_Global_Reachable(corpse.Position, corpse.Map, corpse.Map.listerThings.ThingsInGroup(ThingRequestGroup.Grave), PathEndMode.ClosestTouch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, priorityGetter);
                return;
            }
        }

        public static bool Draw_VampBed(Building_Casket __instance)
        {
            if (__instance.GetComps<CompVampBed>() is CompVampBed b)
            {
                if (!__instance.Spawned || __instance.GetDirectlyHeldThings()?.Count() == 0)
                    __instance.Draw();
                return false;
            }
            return true;
        }

        public static bool Accepts_VampBed(Building_Casket __instance, Thing thing, ref bool __result)
        {
            if (__instance.GetComps<CompVampBed>() is CompVampBed b)
            {
                if (!__instance.HasAnyContents && thing is Pawn p && p.IsVampire())
                {
                    __result = true;
                    return false;
                }
                __result = __instance.Accepts(thing);
                return false;
            }
            return true;
        }

        public static bool get_Graphic_VampBed(Building_Grave __instance, ref Graphic __result)
        {
            if (__instance.def.GetCompProperties<CompProperties_VampBed>() is CompProperties_VampBed b)
            {
                if (!__instance.HasAnyContents)
                {
                    __result = __instance.DefaultGraphic;
                    return false;
                }
                if (__instance.def.building.fullGraveGraphicData == null)
                {
                    __result = __instance.DefaultGraphic;
                    return false;
                }

                if (AccessTools.Field(typeof(Building_Grave), "cachedGraphicFull").GetValue(__instance) == null)
                {
                    AccessTools.Field(typeof(Building_Grave),"cachedGraphicFull").SetValue(__instance, __instance.def.building.fullGraveGraphicData.GraphicColoredFor(__instance));
                }
                __result = (Graphic)AccessTools.Field(typeof(Building_Grave), "cachedGraphicFull").GetValue(__instance);
                return false;
            }
            return true;
        }
        
        public static void GetFloatMenuOptions_VampBed(Building_Casket __instance, Pawn selPawn, ref IEnumerable<FloatMenuOption> __result)
        {
            if (__instance.GetComps<CompVampBed>() is CompVampBed b)
            {
                if (selPawn?.IsVampire() ?? false)
                {
                    __result = __result.Concat(new[] { new FloatMenuOption("ROMV_EnterTorpor".Translate(new object[]
                    {
                       selPawn.Label
                    }), delegate
                    {
                        selPawn.jobs.TryTakeOrderedJob(new Job(VampDefOf.ROMV_EnterTorpor, __instance));
                    }, MenuOptionPriority.Default, null, null, 0f, null, null)});
                }
            }
        }

        //Bill_Medical
        public static bool Notify_DoBillStarted_Debug(Bill_Medical __instance, Pawn billDoer)
        {
            if (__instance.recipe == VampDefOf.ROMV_ExtractBloodPack || __instance.recipe == VampDefOf.ROMV_ExtractBloodVial)
            {
                //Pawn GiverPawn = (Pawn)AccessTools.Method(typeof(Bill_Medical), "get_GiverPawn").Invoke(__instance, null);
                //if (!GiverPawn.Dead && __instance.recipe.anesthetize && HealthUtility.TryAnesthetize(GiverPawn))
                //{

                //}
                return false;
            }
            return true;
        }

        // Verse.ThingDef
        public static void get_AllRecipes_BloodFeedable(ThingDef __instance, ref List<RecipeDef> __result)
        {
            if (!__result.NullOrEmpty())
            {
                if ((__instance?.race?.Animal ?? false) || (__instance?.race?.Humanlike ?? false))
                {
                    if (!__result.Contains(VampDefOf.ROMV_ExtractBloodVial))
                        __result.Add(VampDefOf.ROMV_ExtractBloodVial);
                    if (!__result.Contains(VampDefOf.ROMV_ExtractBloodPack))
                        __result.Add(VampDefOf.ROMV_ExtractBloodPack);
                }
            }
        }

        // RimWorld.Pawn_StoryTracker
        public static bool get_SkinColor_Vamp(Pawn_StoryTracker __instance, ref Color __result)
        {
            Pawn p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (p.IsVampire())
            {
                __result = VampireSkinColors.GetVampireSkinColor(p, Traverse.Create(__instance).Field("melanin").GetValue<float>());
                return false;
            }
            return true;
        }


        // RimWorld.FloatMenuMakerMap
        private static void AddHumanlikeOrders_Vamp(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            CompVampire selVampComp = pawn.VampComp();
            bool pawnIsVampire = pawn.IsVampire();
            if (selVampComp != null && pawnIsVampire)
            {
                //Hide food consumption from menus.
                Thing food = c.GetThingList(pawn.Map).FirstOrDefault(t => t.def.ingestible != null);
                if (food != null)
                {
                    string text;
                    if (food.def.ingestible.ingestCommandString.NullOrEmpty())
                    {
                        text = "ConsumeThing".Translate(new object[]
                        {
                        food.LabelShort
                        });
                    }
                    else
                    {
                        text = string.Format(food.def.ingestible.ingestCommandString, food.LabelShort);
                    }

                    FloatMenuOption o = opts.FirstOrDefault(x => x.Label.Contains(text));
                    if (o != null)
                    {
                        opts.Remove(o);
                    }

                }
                
                //Add blood consumption
                Thing bloodItem = c.GetThingList(pawn.Map).FirstOrDefault(t => t.def.GetCompProperties<CompProperties_BloodItem>() != null);
                if (bloodItem != null)
                {
                    string text = "";
                    if (bloodItem.def.ingestible.ingestCommandString.NullOrEmpty())
                    {
                        text = "ConsumeThing".Translate(new object[]
                        {
                        food.LabelShort
                        });
                    }
                    if (!bloodItem.IsSociallyProper(pawn))
                    {
                        text = text + " (" + "ReservedForPrisoners".Translate() + ")";
                    }
                    FloatMenuOption item5;
                    if (bloodItem.def.IsPleasureDrug && pawn.IsTeetotaler())
                    {
                        item5 = new FloatMenuOption(text + " (" + TraitDefOf.DrugDesire.DataAtDegree(-1).label + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else if (!pawn.CanReach(bloodItem, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        item5 = new FloatMenuOption(text + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else
                    {
                        MenuOptionPriority priority = (!(bloodItem is Corpse)) ? MenuOptionPriority.Default : MenuOptionPriority.Low;
                        item5 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
                        {
                            bloodItem.SetForbidden(false, true);
                            Job job = new Job(VampDefOf.ROMV_ConsumeBlood, bloodItem);
                            job.count = BloodUtility.WillConsumeStackCountOf(pawn, bloodItem.def);
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        }, priority, null, null, 0f, null, null), pawn, bloodItem, "ReservedBy");
                    }
                    opts.Add(item5);
                }
                
                /////////////////////////////////////////////////////////////////////////////////////////////
                ////Pawn VICTIM
                ////////////////
                //Pawn victim = c.GetThingList(pawn.Map).FirstOrDefault(t => t != pawn && t is Pawn) as Pawn;
                //if (victim != null && victim.BloodNeed() is Need_Blood n)
                //{
                //    bool victimIsVampire = victim.IsVampire();
                //    // FEED //////////////////////////
                //    if (!victimIsVampire || (selVampComp?.Bloodline?.canFeedOnVampires ?? false))
                //    {
                //        Action action = delegate
                //        {
                //            Job job = new Job(VampDefOf.ROMV_Feed, victim);
                //            job.count = 1;
                //            pawn.jobs.TryTakeOrderedJob(job);
                //        };
                //        opts.Add(new FloatMenuOption("ROMV_Feed".Translate(new object[]
                //        {
                //                victim.LabelCap
                //        }) + ((n.CurBloodPoints == 1) ? " " + "ROMV_LethalWarning".Translate() : ""), action, MenuOptionPriority.High, null, victim, 0f, null, null));
                //    }
                //    // SIP //////////////////////////
                //    if (victim?.BloodNeed()?.CurBloodPoints > 1)
                //    {
                //        Action action2 = delegate
                //        {
                //            Job job = new Job(VampDefOf.ROMV_Sip, victim);
                //            job.count = 1;
                //            pawn.jobs.TryTakeOrderedJob(job);
                //        };
                //        opts.Add(new FloatMenuOption("ROMV_Sip".Translate(new object[]
                //        {
                //            victim.LabelCap
                //        }), action2, MenuOptionPriority.High, null, victim, 0f, null, null));
                //    }
                //    // EMBRACE /////////////////////
                //    if (victim?.RaceProps?.Humanlike ?? false)
                //    {
                //        if (selVampComp.Thinblooded)
                //        {
                //            opts.Add(new FloatMenuOption("ROMV_CannotEmbrace".Translate(new object[]
                //            {
                //            victim.LabelCap
                //            } + " (" + "ROMV_Thinblooded".Translate() + ")"), null, MenuOptionPriority.High, null, victim, 0f, null, null));
                //        }
                //        else
                //        {
                //            Action actionTwo = delegate
                //            {
                //                Job job = new Job(VampDefOf.ROMV_Embrace, victim);
                //                job.count = 1;
                //                pawn.jobs.TryTakeOrderedJob(job);
                //            };
                //            opts.Add(new FloatMenuOption("ROMV_Embrace".Translate(new object[]
                //            {
                //            victim.LabelCap
                //            }), actionTwo, MenuOptionPriority.High, null, victim, 0f, null, null));
                //        }
                //    }

                //    // Diablerie /////////////////////
                //    if (pawnIsVampire && victimIsVampire)
                //    {
                //        //Action action = delegate
                //        //{
                //        //    Job job = new Job(VampDefOf.ROMV_FeedVampire, victim);
                //        //    job.count = 1;
                //        //    job.playerForced = true;
                //        //    pawn.jobs.TryTakeOrderedJob(job);
                //        //};
                //        //opts.Add(new FloatMenuOption("ROMV_FeedVampire".Translate(new object[]
                //        //{
                //        //        victim.LabelCap
                //        //}), action, MenuOptionPriority.High, null, victim, 0f, null, null));
                //        Action action2 = delegate
                //        {
                //            Job job = new Job(VampDefOf.ROMV_Diablerie, victim);
                //            job.count = 1;
                //            job.playerForced = true;
                //            pawn.jobs.TryTakeOrderedJob(job);
                //        };
                //        string benefitWarning = (selVampComp.Generation < victim.VampComp().Generation) ? " " + "ROMV_DiablerieNoBenefit".Translate() : "";
                //        opts.Add(new FloatMenuOption("ROMV_Diablerie".Translate(new object[]
                //        {
                //                victim.LabelCap
                //        }) + benefitWarning, action2, MenuOptionPriority.High, null, victim, 0f, null, null));
                //    }
                //}
            }
        }

        //public class JobDriver_Vomit : JobDriver
        public static bool MakeNewToils_VampVomit(JobDriver_Vomit __instance, ref IEnumerable<Toil> __result)
        {
            if (__instance.pawn.IsVampire())
            {
                Toil to = new Toil()
                {
                    initAction = delegate
                    {
                        AccessTools.Field(typeof(JobDriver_Vomit), "ticksLeft").SetValue(__instance, Rand.Range(300, 900));
                        int num = 0;
                        IntVec3 c;
                        while (true)
                        {
                            c = __instance.pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
                            num++;
                            if (num > 12)
                            {
                                break;
                            }
                            if (c.InBounds(__instance.pawn.Map) && c.Standable(__instance.pawn.Map))
                            {
                                goto IL_A1;
                            }
                        }
                        c = __instance.pawn.Position;
                        IL_A1:
                        __instance.pawn.CurJob.targetA = c;
                        __instance.pawn.rotationTracker.FaceCell(c);
                        __instance.pawn.pather.StopDead();
                    },
                    tickAction = delegate
                    {
                        int curTicks = Traverse.Create(__instance).Field("ticksLeft").GetValue<int>();
                        if (curTicks % 150 == 149)
                        {
                            FilthMaker.MakeFilth(__instance.pawn.CurJob.targetA.Cell, __instance.pawn.Map, ThingDefOf.FilthBlood, __instance.pawn.LabelIndefinite(), 1);
                            if (__instance.pawn.BloodNeed() is Need_Blood n && n.CurBloodPoints > 0)
                            {
                                n.AdjustBlood(-1);
                            }

                            //if (__instance.pawn.needs.food.CurLevelPercentage > 0.1f)
                            //{
                            //    __instance.pawn.needs.food.CurLevel -= __instance.pawn.needs.food.MaxLevel * 0.04f;
                            //}
                        }

                        AccessTools.Field(typeof(JobDriver_Vomit), "ticksLeft").SetValue(__instance, curTicks - 1);

                        if (curTicks - 1 <= 0)
                        {

                            __instance.ReadyForNextToil();
                            TaleRecorder.RecordTale(TaleDefOf.Vomited, new object[]
                            {
                                __instance.pawn
                            });

                        }

                    }
                };
                to.defaultCompleteMode = ToilCompleteMode.Never;
                to.WithEffect(EffecterDef.Named("ROMV_BloodVomit"), TargetIndex.A);
                to.PlaySustainerOrSound(() => SoundDef.Named("Vomit"));
                __result = __result.Add(to);

                return false;
            }
            return true;
        }

        //public class Alert_NeedDoctor : Alert
        public static bool get_Patients_Vamp(IEnumerable<Pawn> __result)
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome)
                {
                    HashSet<Pawn> pawns = new HashSet<Pawn>(maps[i].mapPawns.FreeColonistsSpawned);
                    List<Pawn> Patients = new List<Pawn>();
                    if (pawns != null && pawns.Count > 0 && pawns.FirstOrDefault(x => x.IsVampire()) != null)
                    {
                        bool healthyDoc = false;
                        foreach (Pawn p in pawns)
                        {
                            if (!p.Downed && p.workSettings != null && p.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
                            {
                                healthyDoc = true;
                                break;
                            }
                        }
                        if (!healthyDoc)
                        {
                            foreach (Pawn p2 in pawns)
                            {
                                if ((p2.Downed && (p2?.needs?.food?.CurCategory ?? HungerCategory.Fed) < HungerCategory.Fed && p2.InBed()) || HealthAIUtility.ShouldBeTendedNow(p2))
                                {
                                    Patients.Add(p2);
                                }
                            }
                        }
                        __result = null;
                        __result = Patients;
                        return false;
                    }

                }
            }
            return true;
        }

        //JobGiver_WanderColony
        public static bool GetWanderRoot_Vamp(Pawn pawn, ref IntVec3 __result)
        {
            if (pawn.VampComp() is CompVampire v && v.IsVampire && GenLocalDate.HourInteger(pawn) >= 6 && GenLocalDate.HourInteger(pawn) <= 17)
            {
                __result = pawn.Position;
                return false;
            }
            return true;
        }

        //JobGiver_Wander 
        public static bool GetExactWanderDest_Vamp(JobGiver_Wander __instance, Pawn pawn, ref IntVec3 __result)
        {
            if (pawn.VampComp() is CompVampire v && v.IsVampire && VampireUtility.IsDaylight(pawn))
            {
                IntVec3 wanderRoot = pawn.Position;
                Func<Pawn, IntVec3, bool> wanderDestValidator = ((Pawn pawnB, IntVec3 loc) => WanderRoomUtility.IsValidWanderDest(pawnB, loc, pawnB.Position) && loc.Roofed(pawnB.Map));
                __result = RCellFinder.RandomWanderDestFor(pawn, wanderRoot, 7f, wanderDestValidator, PawnUtility.ResolveMaxDanger(pawn, Danger.None));
                return false;
            }
            return true;

        }

        // RimWorld.ThinkNode_ConditionalNeedPercentageAbove
        public static bool Satisfied_Vamp(ThinkNode_ConditionalNeedPercentageAbove __instance, Pawn pawn, ref bool __result)
        {
            if (pawn.VampComp() is CompVampire v && v.IsVampire && Traverse.Create(__instance).Field("need").GetValue<NeedDef>() == NeedDefOf.Food)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // RimWorld.Pawn_NeedsTracker
        public static void ShouldHaveNeed_Vamp(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result)
        {
            Pawn p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (p.VampComp() != null && p.VampComp().IsVampire)
            {
                if (nd == NeedDefOf.Food)
                {
                    __result = false;
                    return;
                }
            }
        }
    }
}
