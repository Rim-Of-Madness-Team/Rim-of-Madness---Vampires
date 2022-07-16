using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Vampire.VampireTracker;

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
        
        private static Vector2 scrollPosition = Vector2.zero;
        private static float scrollViewHeight;

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
            if (compVampire != null && (compVampire.IsVampire || compVampire.IsGhoul))
            {
                Rect rect7 = new Rect(CharacterCardUtility.BasePawnCardSize.x - 105f, 0f, 30f, 30f);
                TooltipHandler.TipRegion(rect7, new TipSignal("ROMV_CharacterSheet".Translate()));
                if (Widgets.ButtonImage(rect7, TexButton.ROMV_HumanIcon))
                {
                    HarmonyPatches.isSwitched = false;
                }

                if (DebugSettings.godMode)
                {
                    Rect rect8 = new Rect(CharacterCardUtility.BasePawnCardSize.x - 70f, 0f, 30f, 30f);
                    TooltipHandler.TipRegion(rect8, new TipSignal("ROMV_ChangeVampireSettings".Translate()));
                    if (Widgets.ButtonImage(rect8, TexButton.ROMV_VampireSettingsIcon))
                    {
                        Find.WindowStack.Add(new Dialog_VampireCharacterSetup(pawn, false, true));
                    }
                }

                if (compVampire.IsVampire)
                {
                    Rect rectVampOptions = new Rect(CharacterCardUtility.BasePawnCardSize.x - 105f, 150f, 30f, 30f);
                    switch (VampireTracker.GetSunlightPolicy(pawn))
                    {
                        case SunlightPolicy.Relaxed:
                            TooltipHandler.TipRegion(rectVampOptions, new TipSignal("ROMV_SP_Relaxed".Translate()));
                            if (Widgets.ButtonImage(rectVampOptions, TexButton.ROMV_SunlightPolicyRelaxed))
                            {
                                VampireTracker.SetSunlightPolicy(pawn, SunlightPolicy.Restricted);
                            }
                            break;
                        case SunlightPolicy.Restricted:
                            TooltipHandler.TipRegion(rectVampOptions, new TipSignal("ROMV_SP_Restricted".Translate()));
                            if (Widgets.ButtonImage(rectVampOptions, TexButton.ROMV_SunlightPolicyRestricted))
                            {
                                VampireTracker.SetSunlightPolicy(pawn, SunlightPolicy.NoAI);
                            }
                            break;
                        case SunlightPolicy.NoAI:
                            TooltipHandler.TipRegion(rectVampOptions, new TipSignal("ROMV_SP_NoAI".Translate()));
                            if (Widgets.ButtonImage(rectVampOptions, TexButton.ROMV_SunlightPolicyNoAI))
                            {
                                VampireTracker.SetSunlightPolicy(pawn, SunlightPolicy.Relaxed);
                            }
                            break;
                    }
                }

                NameTriple nameTriple = pawn.Name as NameTriple;
                Rect rectSkillsLabel = new Rect(rect.xMin, rect.yMin - 15, rect.width, HeaderSize);
                
                Text.Font = GameFont.Medium;
                Widgets.Label(rectSkillsLabel, pawn.Name.ToStringFull);
                Text.Font = GameFont.Small;
                //string label = (compVampire.IsGhoul) ? GhoulUtility.MainDesc(pawn)  : VampireUtility.MainDesc(pawn);
                float currentY = 0f;
                if (!compVampire.IsGhoul)
                {
                    Rect rBloodlineDesc = new Rect(0f, 45f, rect.width * 0.3f, 30f);
                    Rect rGenerationDesc = new Rect((rect.width * 0.3f) + 8f, 45f, rect.width * 0.3f, 30f);
                    TooltipHandler.TipRegion(rBloodlineDesc, new TipSignal(VampireStringUtility.GetVampireTooltip(compVampire.Bloodline, compVampire.Generation)));
                    TooltipHandler.TipRegion(rGenerationDesc, new TipSignal(VampireStringUtility.GetGenerationDescription(compVampire.Generation)));
                    Widgets.Label(rBloodlineDesc, "ROMV_Bloodline".Translate() + ": " + compVampire.Bloodline.LabelCap);
                    Widgets.Label(rGenerationDesc, "ROMV_Generation".Translate() + ": " + compVampire.Generation);
                    currentY = rGenerationDesc.yMax;
                }
                else
                {
                    Rect rectDesc = new Rect(0f, 45f, rect.width, 60f);
                    Widgets.Label(rectDesc, GhoulUtility.MainDesc(pawn));
                    currentY = rectDesc.yMax;
                }
                currentY += 36f;

                //                               Skills

                //Widgets.DrawLineHorizontal(rect.x - 10, rectSkillsLabel.yMax + Padding, rect.width - 15f);
                //---------------------------------------------------------------------

                Rect rectSkills = new Rect(rect.x, currentY - 42 + Padding, rectSkillsLabel.width, SkillsColumnHeight);
                    Rect rectInfoPane = new Rect(rectSkills.x, rectSkills.y + Padding, rect.width * 0.9f, SkillsColumnHeight);
                
                    //Rect rectSkillsPane = new Rect(rectSkills.x + SkillsColumnDivider, rectSkills.y + Padding, rectSkills.width - SkillsColumnDivider, SkillsColumnHeight);

                    LevelPane(rectInfoPane, compVampire);
                    //InfoPane(rectSkillsPane, compVampire);

                    // LEVEL ________________             |       
                    // ||||||||||||||||||||||             |       
                    // Points Available 1                 |       
                    //

                    float powersTextSize = Text.CalcSize("ROMV_Disciplines".Translate()).x;
                    Rect rectPowersLabel = new Rect((rect.width / 2 - powersTextSize / 2) - 15f, rectSkills.yMax + SectionOffset - 5, rect.width, HeaderSize);
                    Text.Font = GameFont.Medium;
                    Widgets.Label(rectPowersLabel, "ROMV_Disciplines".Translate().CapitalizeFirst());
                //                                   Disciplines
                    Text.Font = GameFont.Small;

                    //Powers

                    Widgets.DrawLineHorizontal(rect.x - 10, rectPowersLabel.yMax, rect.width - 15f);
                //---------------------------------------------------------------------
                float curY = rectPowersLabel.yMax;
                var outRect = new Rect(rect.x, curY + 3f, rect.width, (rectSkills.height * 4f) + 16f);
                //rectSkills.width -= 10f;
                var viewRect = new Rect(rect.x, curY + 3f, rect.width, scrollViewHeight);//rectSkills.height * 4f);
                float scrollViewY = 0f;
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
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
                        Rect rectDisciplines = new Rect(rect.x, curY, rectPowersLabel.width, ButtonSize + Padding);
                        PowersGUIHandler(rectDisciplines, compVampire, compVampire.Sheet.Disciplines[i]);
                        var y = ButtonSize + Padding * 2 + TextSize * 2;
                        curY += y;
                        scrollViewY += y;
                    }
                if (Event.current.type == EventType.Layout)
                {
                    scrollViewHeight = scrollViewY + (ButtonSize + Padding * 2 + TextSize * 2) + 30f;
                }
                Widgets.EndScrollView();
            }
            GUI.EndGroup();
        }
        
        #region LevelPane
        public static void LevelPane(Rect inRect, CompVampire compVampire)
        {
            Rect rectLevelBar = new Rect(inRect.x, inRect.y, inRect.width, HeaderSize * 0.6f);
            DrawLevelBar(rectLevelBar, compVampire);

            //rectPointsAvail.yMax + 3f
            //[||||||||||||||||||||||||||||||||||||||||||||||||||||||||||]

            string levelText = "ROMV_Level".Translate().CapitalizeFirst() + " " + compVampire.Level.ToString();
            float levelTextLength = Text.CalcSize(levelText).x;
            Rect rectLevel = new Rect(inRect.x + (inRect.width / 2) - (levelTextLength / 2), rectLevelBar.yMax + 3, inRect.width, TextSize);
            Text.Font = GameFont.Tiny;
            Widgets.Label(rectLevel, levelText);
            Text.Font = GameFont.Small;
            //                           Level 1


            string pointsAvailableText = compVampire.AbilityPoints + " " + "ROMV_PointsAvailable".Translate();
            float pointsAvailableTextLength = Text.CalcSize(pointsAvailableText).x;
            Rect rectPointsAvail = new Rect(inRect.x + (inRect.width / 2) - (pointsAvailableTextLength / 2), rectLevel.yMax, inRect.width, TextSize);
            Text.Font = GameFont.Tiny;
            Widgets.Label(rectPointsAvail, pointsAvailableText);
            Text.Font = GameFont.Small;

            //0 points available

            if (DebugSettings.godMode)
            {
                Rect rectDebugPlus = new Rect(rectLevelBar.xMax - 30, rectLevelBar.yMin, inRect.width * 0.1f, TextSize);
                if (Widgets.ButtonText(rectDebugPlus, "+"))
                {
                    compVampire.Notify_LevelUp(false);
                }
                if (compVampire.Level > 0)
                {
                    Rect rectDebugReset = new Rect(rectDebugPlus.x, rectDebugPlus.yMax + 1, rectDebugPlus.width, TextSize);
                    if (Widgets.ButtonText(rectDebugReset, "~"))
                    {
                        compVampire.Notify_ResetAbilities();
                    }
                }
            }

            

        }

        public static string XPTipString(CompVampire compVampire)
        {
            return compVampire.XP.ToString() + " / " + compVampire.XPTillNextLevel.ToString() + "\n" + ((compVampire.IsGhoul) ? "ROMV_XPDescGhoul".Translate() : "ROMV_XPDesc".Translate());
        }

        public static void DrawLevelBar(Rect rect, CompVampire compVampire)
        {
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
            Color colorToUseTwo = new Color(0.4f, 0.1f, 0.1f);
            Widgets.FillableBar(rect3, (float)compVampire.XPTillNextLevelPercent, SolidColorMaterials.NewSolidColorTexture(colorToUse), SolidColorMaterials.NewSolidColorTexture(colorToUseTwo), false);
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
                Rect rectLabel = new Rect(inRect.x, inRect.y, inRect.width * 0.7f, TextSize);
                Text.Font = GameFont.Small;
                Widgets.Label(rectLabel, discipline.Def.LabelCap);
                Text.Font = GameFont.Small;
                int count = 0;
                int pntCount = 0;

                foreach (VitaeAbilityDef ability in discipline.Def.abilities)
                {

                    Rect buttonRect = new Rect(buttonXOffset, rectLabel.yMax, VampButtonSize, VampButtonSize);
                    TooltipHandler.TipRegion(buttonRect, () => ability.LabelCap + "\n\n" + ability.description, 398452); //"\n\n" + "PJ_CheckStarsForMoreInfo".Translate()

                    
                    Texture2D abilityTex = ability.uiIcon;
                    bool disabledForGhouls =
                        compVampire.IsGhoul && (int) compVampire.GhoulHediff.ghoulPower < discipline.Level;
                    if (disabledForGhouls)
                        GUI.color = Color.gray;
                    if (compVampire.AbilityPoints == 0 || discipline.Level >= 4)
                    {
                        Widgets.DrawTextureFitted(buttonRect, abilityTex, 1.0f);
                    }
                    else if (Widgets.ButtonImage(buttonRect, abilityTex) && compVampire.AbilityUser.Faction == Faction.OfPlayer)
                    {
                        if (compVampire.AbilityUser.story != null && compVampire.AbilityUser.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent) && ability.MainVerb.isViolent)
                        {
                            Messages.Message("IsIncapableOfViolenceLower".Translate(new object[]
                            {
                            compVampire.parent.LabelShort
                            }), MessageTypeDefOf.RejectInput);
                            return;
                        }
                        if (disabledForGhouls)
                        {
                            Messages.Message("ROMV_DomitorVitaeIsTooWeak".Translate(new object[]
                            {
                                compVampire.parent.LabelShort
                            }), MessageTypeDefOf.RejectInput);
                            return;
                        }

                        discipline.Notify_PointsInvested(1); //LevelUpPower(power);
                        compVampire.Notify_UpdateAbilities();
                        compVampire.AbilityPoints -= 1; //powerDef.abilityPoints;
                    }

                    float drawXOffset = buttonXOffset;
                    float modifier = 0f;
                    switch (count)
                    {
                        case 0: break;
                        case 1: modifier = 0.75f; break;
                        case 2: modifier = 0.72f; break;
                        case 3: modifier = 0.60f; break;
                    }
                    if (count != 0) drawXOffset -= VampButtonPointSize * count * modifier;
                    else drawXOffset -= 2;

                    for (int j = 0; j < count + 1; j++)
                    {

                        ++pntCount;
                        float drawYOffset = VampButtonSize + TextSize + Padding;
                        Rect powerRegion = new Rect(inRect.x + drawXOffset + VampButtonPointSize * j, inRect.y + drawYOffset, VampButtonPointSize, VampButtonPointSize);
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
                    buttonXOffset += ButtonSize * 3f + Padding;
                    if (disabledForGhouls)
                        GUI.color = Color.white;
                }
            }
        }
        #endregion PowersGUI
    }
}
