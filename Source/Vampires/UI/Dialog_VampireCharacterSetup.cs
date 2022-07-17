using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Vampire
{
    public class Dialog_VampireCharacterSetup : Window
    {
        private Listing_Standard selectedVampireInfoListing = new Listing_Standard();
        private float radioRectHeight = 125f;
        private float spacing = 10f;
        private Vector2 currentScrollPoint = Vector2.zero;
        private Pawn vampirePawn = null;
        private BloodlineDef chosenBloodline = null;
        private int chosenGeneration = 11;
        private BloodlineDef originalBloodline = null;
        private int originalGeneration = 11;
        private bool debugMode = false;
        private bool randomBloodline = false;
        private bool randomGeneration = false;

        public Dialog_VampireCharacterSetup(Pawn pawn, bool random, bool debug) : base()
        {
            vampirePawn = pawn;
            debugMode = debug;
            randomBloodline = random;
            randomGeneration = random;

            if (vampirePawn.IsVampire(true) && vampirePawn.VampComp() is CompVampire vComp)
            {
                originalBloodline = chosenBloodline = vComp.Bloodline;
                originalGeneration = chosenGeneration = vComp.Generation;

            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            DoWindowContents(inRect, ref currentScrollPoint);
        }

        private static bool BloodlineAllowed(BloodlineDef bloodline, int generation)
        {
            switch (generation)
            {
                case 1:
                    if (bloodline == VampDefOf.ROMV_Caine)
                        return true;
                    return false;
                case 2:
                    if (bloodline == VampDefOf.ROMV_TheThree)
                        return true;
                    return false;
                default:
                    if (bloodline == VampDefOf.ROMV_Caine || bloodline == VampDefOf.ROMV_TheThree)
                        return false;
                    return true;
            }
        }

        private static IntRange GetGenerationRange(BloodlineDef bloodline, bool random, bool debug)
        {
            if (bloodline == VampDefOf.ROMV_Caine)
                return new IntRange(1, 1);
            if (bloodline == VampDefOf.ROMV_TheThree)
                return new IntRange(2, 2);
            if (debug && !random)
                return new IntRange(1, 14);
            return new IntRange(VampireGen.VAMP_MINSPAWNGENERATION, VampireGen.VAMP_MAXSPAWNGENERATION);
        }


        public void DoWindowContents(Rect inRect, ref Vector2 scrollPosition)
        {
            float currentY = 0f;

            //Header
            Text.Font = GameFont.Medium;
            Rect headerRect = new Rect
                (
                    (inRect.width * 0.5f) - ("ROMV_VampireCharacterSettings".Translate().GetWidthCached() * 0.5f),
                    currentY,
                    inRect.width,
                    45f
                );
            Widgets.Label(headerRect, "ROMV_VampireCharacterSettings".Translate());
            currentY = headerRect.yMax;
            currentY += 8f;

            //Bloodline Section Label
            Rect bloodlineLabelRect = new Rect
                (
                    (inRect.width * 0.5f) - ("ROMV_Bloodline".Translate().GetWidthCached() * 0.5f),
                    currentY,
                    inRect.width,
                    30f
                );
            Text.Font = GameFont.Small;
            Widgets.Label(bloodlineLabelRect, "ROMV_Bloodline".Translate());
            currentY = bloodlineLabelRect.yMax;
            currentY += spacing;

            //Bloodline Section Description
            Rect bloodlineDescRect = new Rect
                (
                    inRect.x,
                    currentY,
                    inRect.width * 0.6f,
                    30f
                );
            Text.Font = GameFont.Tiny;
            Widgets.Label(bloodlineDescRect, "ROMV_ChooseBloodlineForPawn".Translate(vampirePawn.LabelCap));
            //Character icon
            Rect characterRect = new Rect
                (
                    inRect.width * 0.6f,
                    currentY,
                    inRect.width * 0.3f,
                    30f
                );
            Widgets.ThingIcon(characterRect, vampirePawn, 1f, null);
            currentY = bloodlineDescRect.yMax;
            currentY += spacing;

            //Bloodline Radio Selection Section
            Rect outerRadioRect = new Rect
                (
                    0f,
                    currentY,
                    inRect.width,
                    radioRectHeight
                );

            Rect innerRadioRect = new Rect
                (
                    20f,
                    currentY + 6f,
                    inRect.width - 20f,
                    DefDatabase<BloodlineDef>.DefCount * 30f
                );

            Widgets.BeginScrollView(outerRadioRect, ref scrollPosition, innerRadioRect, true);
            selectedVampireInfoListing.Begin(innerRadioRect);

            //Vampire bloodline list

            //No bloodline
            if (selectedVampireInfoListing.RadioButton("ROMV_NoBloodline".Translate(),
                chosenBloodline == null && randomBloodline == false, 0f,
                "ROMV_NoBloodlineDesc".Translate()))
            {
                randomBloodline = false;
                chosenBloodline = null;
            }
            selectedVampireInfoListing.Gap(6f);

            //Random bloodline
            if (selectedVampireInfoListing.RadioButton("ROMV_RandomBloodline".Translate(),
                chosenBloodline == null && randomBloodline == true, 0f,
                "ROMV_RandomBloodlineDesc".Translate()))
            {
                randomBloodline = true;
                chosenBloodline = null;
                if (!randomGeneration)
                    chosenGeneration = GetGenerationRange(null, true, debugMode).RandomInRange;
            }
            selectedVampireInfoListing.Gap(6f);

            //All bloodlines
            foreach (BloodlineDef bloodlineDef in DefDatabase<BloodlineDef>.AllDefs)
            {
                if (!debugMode && !bloodlineDef.scenarioCanAdd)
                    continue;

                if (!BloodlineAllowed(bloodlineDef, chosenGeneration))
                    continue;
                
                if (selectedVampireInfoListing.RadioButton(bloodlineDef.LabelCap,
                chosenBloodline == bloodlineDef, 0f,
                VampireStringUtility.GetVampireTooltip(bloodlineDef,chosenGeneration)))
                {
                    randomBloodline = false;
                    chosenBloodline = bloodlineDef;
                }

                selectedVampireInfoListing.Gap(6f);
            }

            selectedVampireInfoListing.End();
            Widgets.EndScrollView();

            currentY = outerRadioRect.yMax;

            if (chosenBloodline != null || randomBloodline)
            {
                //Generation Section Label
                Rect generationLabelRect = new Rect
                    (
                        (inRect.width * 0.5f) - ("ROMV_Generation".Translate().GetWidthCached() * 0.5f),
                        currentY,
                        inRect.width,
                        30f
                    );
                Text.Font = GameFont.Small;
                Widgets.Label(generationLabelRect, "ROMV_Generation".Translate());
                currentY = generationLabelRect.yMax;
                currentY += spacing;

                //Generation Section Description
                Rect generationDescRect = new Rect
                    (
                        inRect.x,
                        currentY,
                        inRect.width,
                        30f
                    );
                Text.Font = GameFont.Tiny;
                Widgets.Label(generationDescRect, "ROMV_ChooseGenerationForPawn".Translate());
                currentY = generationDescRect.yMax;
                currentY += spacing;

                //Random generation toggle
                Rect randomGenerationToggleRect = new Rect
                    (
                        inRect.x + 9f,
                        currentY,
                        inRect.width - 9f,
                        30f
                    );

                TooltipHandler.TipRegion(randomGenerationToggleRect, new TipSignal("ROMV_RandomGenerationDesc".Translate(
                    GetGenerationRange(chosenBloodline, randomBloodline, debugMode).min.ToString(),
                    GetGenerationRange(chosenBloodline, randomBloodline, debugMode).max.ToString()
                    )));
                Widgets.CheckboxLabeled
                    (
                        randomGenerationToggleRect,
                        "ROMV_RandomGeneration".Translate(), 
                        ref randomGeneration,
                        false, 
                        null, 
                        null, 
                        false
                    );
                currentY = randomGenerationToggleRect.yMax;
                currentY += spacing;
                Text.Font = GameFont.Tiny;

                //Generation widget
                if (!randomGeneration)
                {
                    Text.Font = GameFont.Small;
                    Rect generationWidgetRect = new Rect
                    (
                        inRect.x,
                        currentY,
                        inRect.width,
                        30f
                    );
                    chosenGeneration = //(int) selectedVampireInfoListing.Slider(1f, 4f, 13f);
                            (int)Widgets.HorizontalSlider(generationWidgetRect,
                                chosenGeneration,
                                (float)GetGenerationRange(chosenBloodline, randomBloodline, debugMode).min,
                                (float)GetGenerationRange(chosenBloodline, randomBloodline, debugMode).max, false,
                                chosenGeneration.ToString(),
                                GetGenerationRange(chosenBloodline, randomBloodline, debugMode).min.ToString(),
                                GetGenerationRange(chosenBloodline, randomBloodline, debugMode).max.ToString(), 1f);
                    TooltipHandler.TipRegion(generationWidgetRect, () => VampireStringUtility.GetGenerationDescription(chosenGeneration), 57427927);
                    currentY = generationWidgetRect.yMax;
                    currentY += spacing;
                }

            }

            //Accept Button
            Text.Font = GameFont.Small;
            Rect acceptButton = new Rect
                (
                    18,
                    (inRect.yMax - 34) - spacing,
                    90,
                    34
                );

            if (Widgets.ButtonText(acceptButton, "AcceptButton".Translate()))
            {
                if (chosenBloodline != null || randomBloodline)
                {
                    if (vampirePawn.VampComp() is CompVampire vComp)
                    {
                        if (vampirePawn.IsVampire(true))
                        {
                            vComp.Bloodline = chosenBloodline;

                            if (randomGeneration)
                                chosenGeneration = GetGenerationRange(chosenBloodline, randomBloodline, debugMode).RandomInRange;
                            
                            vComp.Generation = chosenGeneration;
                        }
                        else
                        {
                            vComp.InitializeVampirism(null, chosenBloodline, chosenGeneration, chosenGeneration == 1);
                        }
                    }
                }
                else
                {
                    VampireUtility.RemoveVampirism(vampirePawn, true, true);
                }

                this.Close(false);
            }

            //Reset Button
            Rect resetButton = new Rect
            (
                (inRect.width * 0.333f)+ 9f,
                (inRect.yMax - 34) - spacing,
                90,
                34
            );

            if (Widgets.ButtonText(resetButton, "ResetButton".Translate()))
            {
                chosenBloodline = originalBloodline;
                chosenGeneration = originalGeneration;
            }

            //Cancel Button
            Rect cancelButton = new Rect
            (
                (inRect.width * 0.666f),
                (inRect.yMax - 34) - spacing,
                90,
                34
            );

            if (Widgets.ButtonText(cancelButton, "CancelButton".Translate()))
                this.Close(false);
        }

    }
}