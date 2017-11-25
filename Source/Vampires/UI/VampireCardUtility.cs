using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Vampire
{
    public class VampireCardUtility
    {
        // RimWorld.CharacterCardUtility
        public static Vector2 vampCardSize = default(Vector2);
        public static Vector2 VampCardSize
        {
            get
            {
                if (vampCardSize == default(Vector2))
                {
                    vampCardSize = new Vector2(395f, 536f);
                    if (LanguageDatabase.activeLanguage == LanguageDatabase.AllLoadedLanguages.FirstOrDefault(x => x.folderName == "German"))
                    {
                        vampCardSize = new Vector2(470f, 536f);
                    }
                }
                return vampCardSize;
            }
        }
        //public static Vector2 vampCardSize = new Vector2(470f, 536f);  // Ideal for German version

        public static float ButtonSize = 40f;

        public static float VampButtonSize = 46f;

        public static float VampButtonPointSize = 24f;

        public static float HeaderSize = 32f;

        public static float TextSize = 22f;

        public static float Padding = 3f;

        public static float SpacingOffset = 15f;

        public static float SectionOffset = 8f;

        public static float ColumnSize = 245f;

        public static float SkillsColumnHeight = 80f;

        public static float SkillsColumnDivider = 114f;
        //public static float SkillsColumnDivider = 170f; // Ideal for German version

        public static float SkillsTextWidth = 138f;
        //public static float SkillsTextWidth = 170f; // Ideal for German version

        public static float SkillsBoxSize = 18f;

        public static float PowersColumnHeight = 195f;

        public static float PowersColumnWidth = 123f;

        public static bool adjustedForLanguage = false;

        public static void AdjustForLanguage()
        {
            if (!adjustedForLanguage)
            {
                adjustedForLanguage = true;
                if (LanguageDatabase.activeLanguage == LanguageDatabase.AllLoadedLanguages.FirstOrDefault(x => x.folderName == "German"))
                {
                    SkillsColumnDivider = 170f;
                    SkillsTextWidth = 170f;
                }
            }
        }

        // RimWorld.CharacterCardUtility
        public static void DrawVampCard(Rect rect, Pawn pawn)
        {
            AdjustForLanguage();

            GUI.BeginGroup(rect);

            CompVampire compVampire = pawn.GetComp<CompVampire>();
            if (compVampire != null && compVampire.IsVampire)
            {
                Rect rect7 = new Rect(CharacterCardUtility.PawnCardSize.x - 105f, 0f, 30f, 30f);
                TooltipHandler.TipRegion(rect7, new TipSignal("ROMV_CharacterSheet".Translate()));
                if (Widgets.ButtonImage(rect7, TexButton.ROMV_HumanIcon))
                {
                    HarmonyPatches.isSwitched = false;
                }

                NameTriple nameTriple = pawn.Name as NameTriple;
                Rect rectSkillsLabel = new Rect(rect.xMin, rect.yMin - 15, rect.width, HeaderSize);
                
                Text.Font = GameFont.Medium;
                Widgets.Label(rectSkillsLabel, pawn.Name.ToStringFull);
                Text.Font = GameFont.Small;
                string label = VampireUtility.MainDesc(pawn);
                Rect rectDesc = new Rect(0f, 45f, rect.width, 60f);
                Widgets.Label(rectDesc, label);

                //                               Skills

                //Widgets.DrawLineHorizontal(rect.x - 10, rectSkillsLabel.yMax + Padding, rect.width - 15f);
                    //---------------------------------------------------------------------

                    Rect rectSkills = new Rect(rect.x, rectDesc.yMax - 30 + Padding, rectSkillsLabel.width, SkillsColumnHeight);
                    Rect rectInfoPane = new Rect(rectSkills.x, rectSkills.y + Padding, SkillsColumnDivider, SkillsColumnHeight);
                    Rect rectSkillsPane = new Rect(rectSkills.x + SkillsColumnDivider, rectSkills.y + Padding, rectSkills.width - SkillsColumnDivider, SkillsColumnHeight);

                    LevelPane(rectInfoPane, compVampire);
                    InfoPane(rectSkillsPane, compVampire);

                    // LEVEL ________________             |       
                    // ||||||||||||||||||||||             |       
                    // Points Available 1                 |       
                    //

                    float powersTextSize = Text.CalcSize("ROMV_Disciplines".Translate()).x;
                    Rect rectPowersLabel = new Rect((rect.width / 2) - (powersTextSize / 2), rectSkills.yMax + SectionOffset - 5, rect.width, HeaderSize);
                    Text.Font = GameFont.Medium;
                    Widgets.Label(rectPowersLabel, "ROMV_Disciplines".Translate().CapitalizeFirst());
                    Text.Font = GameFont.Small;

                    //Powers

                    Widgets.DrawLineHorizontal(rect.x - 10, rectPowersLabel.yMax, rect.width - 15f);
                //---------------------------------------------------------------------


                float curY = rectPowersLabel.yMax;
                if (compVampire.Sheet.Pawn == null)
                {
                    compVampire.Sheet.Pawn = pawn;
                }
                if (compVampire.Sheet.Disciplines.NullOrEmpty())
                {
                    compVampire.Sheet.InitializeDisciplines();
                }
                    for (int i = 0; i < compVampire.Sheet.Disciplines.Count; i++)
                    {
                        Rect rectDisciplines = new Rect(rect.x + ButtonSize, curY, rectPowersLabel.width, ButtonSize + Padding);
                        PowersGUIHandler(rectDisciplines, compVampire, compVampire.Sheet.Disciplines[i]);
                        curY += ButtonSize + (Padding * 2) + (TextSize * 2);
                    }
            }
            GUI.EndGroup();
        }
        
        #region LevelPane
        public static void LevelPane(Rect inRect, CompVampire compVampire)
        {
            Rect rectLevel = new Rect(inRect.x, inRect.y, inRect.width * 0.7f, TextSize);
            Text.Font = GameFont.Small;
            Widgets.Label(rectLevel, "ROMV_Level".Translate().CapitalizeFirst() + " " + compVampire.Level.ToString());
            Text.Font = GameFont.Small;

            if (DebugSettings.godMode)
            {
                Rect rectDebugPlus = new Rect(rectLevel.xMax, inRect.y, inRect.width * 0.3f, TextSize);
                if (Widgets.ButtonText(rectDebugPlus, "+", true, false, true))
                {
                    compVampire.Notify_LevelUp(false);
                }
                if (compVampire.Level > 0)
                {
                    Rect rectDebugReset = new Rect(rectDebugPlus.x, rectDebugPlus.yMax + 1, rectDebugPlus.width, TextSize);
                    if (Widgets.ButtonText(rectDebugReset, "~", true, false, true))
                    {
                        compVampire.Notify_ResetAbilities();
                    }
                }
            }

            //Level 0

            Rect rectPointsAvail = new Rect(inRect.x, rectLevel.yMax, inRect.width, TextSize);
            Text.Font = GameFont.Tiny;
            Widgets.Label(rectPointsAvail, compVampire.AbilityPoints + " " + "ROMV_PointsAvailable".Translate());
            Text.Font = GameFont.Small;

            //0 points available

            Rect rectLevelBar = new Rect(rectPointsAvail.x, rectPointsAvail.yMax + 3f, inRect.width - 10f, HeaderSize * 0.6f);
            DrawLevelBar(rectLevelBar, compVampire);

            //[|||||||||||||]
        }

        public static string XPTipString(CompVampire compVampire)
        {
            return compVampire.XP.ToString() + " / " + compVampire.XPTillNextLevel.ToString() + "\n" + "ROMV_XPDesc".Translate();
        }

        public static void DrawLevelBar(Rect rect, CompVampire compVampire)
        {
            ////base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
            if (rect.height > 70f)
            {
                float num = (rect.height - 70f) / 2f;
                rect.height = 70f;
                rect.y += num;
            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            TooltipHandler.TipRegion(rect, new TipSignal(() => XPTipString(compVampire), rect.GetHashCode()));
            float num2 = 14f;
            if (rect.height < 50f)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height);
            rect3 = new Rect(rect3.x, rect3.y, rect3.width, rect3.height - num2);

            Color colorToUse = new Color(1.0f, 0.91f, 0f);
            Widgets.FillableBar(rect3, (float)compVampire.XPTillNextLevelPercent, SolidColorMaterials.NewSolidColorTexture(colorToUse), BaseContent.GreyTex, false);
            //Widgets.FillableBar(rect3, (float)compVampire.XPTillNextLevelPercent, (Texture2D)AccessTools.Field(typeof(Widgets), "BarFullTexHor").GetValue(null), BaseContent.GreyTex, false);
            //compVampire.XPTillNextLevelPercent
        }
        #endregion InfoPane

        #region SkillsPane
        public static void InfoPane(Rect inRect, CompVampire compVampire)
        {
            float currentYOffset = inRect.y;
            
        }
        #endregion SkillsPane

        #region PowersGUI
        public static void PowersGUIHandler(Rect inRect, CompVampire compVampire, Discipline discipline)
        {
            float buttonXOffset = inRect.x;
            if (discipline?.Def?.abilities is List<VitaeAbilityDef> abilities && !abilities.NullOrEmpty())
            {
                Rect rectLabel = new Rect(inRect.x - ButtonSize, inRect.y, inRect.width * 0.7f, TextSize);
                Text.Font = GameFont.Small;
                Widgets.Label(rectLabel, discipline.Def.LabelCap);
                Text.Font = GameFont.Small;
                int count = 0;
                int pntCount = 0;

                foreach (VitaeAbilityDef ability in discipline.Def.abilities)
                {

                    Rect buttonRect = new Rect(buttonXOffset, rectLabel.yMax, VampButtonSize, VampButtonSize);
                    TooltipHandler.TipRegion(buttonRect, () => ability.LabelCap + "\n\n" + ability.description, 398452); //"\n\n" + "PJ_CheckStarsForMoreInfo".Translate()

                    if (compVampire.AbilityPoints == 0 || discipline.Level >= 4)
                    {

                        Widgets.DrawTextureFitted(buttonRect, ability.uiIcon, 1.0f);
                    }
                    else if (Widgets.ButtonImage(buttonRect, ability.uiIcon) && (compVampire.AbilityUser.Faction == Faction.OfPlayer))
                    {

                        //if (compVampire.AbilityPoints < ability.abilityCost)
                        //{
                        //    Messages.Message("ROMV_NotEnoughAbilityPoints".Translate(new object[]
                        //    {
                        //    compVampire.AbilityPoints,
                        //    ability.abilityCost
                        //    }), MessageTypeDefOf.RejectInput);
                        //    return;
                        //}
                        if (compVampire.AbilityUser.story != null && (compVampire.AbilityUser.story.WorkTagIsDisabled(WorkTags.Violent) && ability.MainVerb.isViolent))
                        {
                            Messages.Message("IsIncapableOfViolenceLower".Translate(new object[]
                            {
                            compVampire.parent.LabelShort
                            }), MessageTypeDefOf.RejectInput);
                            return;
                        }
                        discipline.Notify_PointsInvested(1); //LevelUpPower(power);
                        compVampire.Notify_UpdateAbilities();
                        compVampire.AbilityPoints -= 1; //powerDef.abilityPoints;
                    }

                    float drawXOffset = buttonXOffset - ButtonSize;
                    float modifier = 0f;
                    switch (count)
                    {
                        case 0: break;
                        case 1: modifier = 0.75f; break;
                        case 2: modifier = 0.72f; break;
                        case 3: modifier = 0.60f; break;
                    }
                    if (count != 0) drawXOffset -= ((VampButtonPointSize * count) * modifier);
                    else drawXOffset -= 2;

                    for (int j = 0; j < count + 1; j++)
                    {

                        ++pntCount;
                        float drawYOffset = VampButtonSize + TextSize + Padding;
                        Rect powerRegion = new Rect(inRect.x + drawXOffset + (VampButtonPointSize * j), inRect.y + drawYOffset, VampButtonPointSize, VampButtonPointSize);
                        if (discipline.Points >= pntCount)
                        {
                            Widgets.DrawTextureFitted(powerRegion, TexButton.ROMV_PointFull, 1.0f);
                        }
                        else
                        {
                            Widgets.DrawTextureFitted(powerRegion, TexButton.ROMV_PointEmpty, 1.0f);
                        }

                        TooltipHandler.TipRegion(powerRegion, () => ability.GetDescription() + "\n" + compVampire.PostAbilityVerbCompDesc(ability.MainVerb), 398462);
                    }
                    ++count;
                    buttonXOffset += (ButtonSize * 3f) + Padding;
                }
            }
        }
        #endregion PowersGUI
    }
}
