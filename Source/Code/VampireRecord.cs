using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

/// <summary>
///     Protective code to prevent data cleaning mods from removing Vampire lineages from the game.
///     --Specifically, some mods claim to help speed up RimWorld by deleting unused characters
///     from the game. However, this actually breaks the entire Vampire mod, because it
///     destroys generated vampire hierarchies. To prevent this, we have the "VampireRecord"
///     class, which is simply a duplicate of Pawn's superficial data.
/// </summary>
public class VampireRecord : IExposable
{
    private BackstoryDef adulthood;
    private float age;
    private BloodlineDef bloodline;
    private BodyTypeDef bodyType;
    private BackstoryDef childhood;
    private HeadTypeDef headType;

    private bool dead;
    private bool destroyed;
    private Faction faction;
    private Gender gender;
    private int generation;
    private Color hairColor;
    private HairDef hairDef;
    private string id;

    private PawnKindDef kindDef;
    private Color skinColor;
    private Name nameInt;
    private readonly int recordAge;
    private string sireId;
    private TraitSet traits;


    public VampireRecord(Pawn originPawn, int originGeneration, BloodlineDef originBloodline, float originAge,
        string originSireId, Faction originFaction = null)
    {
        kindDef = originPawn.kindDef;
        nameInt = originPawn.Name;
        gender = originPawn.gender;
        generation = originGeneration;
        bloodline = originBloodline;
        age = originAge;
        sireId = originSireId;
        recordAge = Find.TickManager.TicksGame;
        faction = originFaction ?? Find.FactionManager.FirstFactionOfDef(VampDefOf.ROMV_LegendaryVampires);
        if (originPawn.story == null)
        {
            Log.Warning(originPawn.Name.ToStringFull + " has no story to copy.");
            return;
        }

        childhood = originPawn.story.Childhood;
        adulthood = originPawn.story.Adulthood;
        skinColor = originPawn.story.SkinColor;
        hairColor = originPawn.story.HairColor;
        headType = originPawn.story.headType;
        bodyType = originPawn.story.bodyType;
        hairDef = originPawn.story.hairDef;
        traits = originPawn.story.traits;

        id = generation + "_" + bloodline + "_" + nameInt.ToStringFull + "_" + gender;
    }

    public string NameFull => nameInt.ToStringFull;
    public string ID => id;
    public string SireID => sireId;

    public float Age => age + (Find.TickManager.TicksGame - recordAge);

    public bool Dead
    {
        get => dead;
        set => dead = value;
    }

    public bool Destroyed
    {
        get => destroyed;
        set => destroyed = value;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref id, "id");
        Scribe_Values.Look(ref sireId, "sireId");
        Scribe_Values.Look(ref kindDef, "kindDef");
        Scribe_Values.Look(ref nameInt, "nameInt");
        Scribe_Values.Look(ref gender, "gender");
        Scribe_Values.Look(ref generation, "generation");
        Scribe_Values.Look(ref bloodline, "bloodline");
        Scribe_Values.Look(ref age, "age");
        Scribe_Values.Look(ref faction, "faction");
        Scribe_Values.Look(ref childhood, "childhood");
        Scribe_Values.Look(ref adulthood, "adulthood");
        Scribe_Values.Look(ref skinColor, "skinColor");
        Scribe_Values.Look(ref hairColor, "hairColor");
        Scribe_Values.Look(ref traits, "traits");
        Scribe_Values.Look(ref dead, "dead");
        Scribe_Values.Look(ref destroyed, "destroyed");
    }

    public bool Equals(VampireRecord other)
    {
        return id == other.ID;
    }

    public Pawn GenerateForWorld()
    {
        var sire = sireId == "" ? null : Find.World.GetComponent<WorldComponent_VampireTracker>().GetVampire(sireId);
        var generatedPawn = VampireGen.GenerateVampire(generation, bloodline, sire, faction, generation == 1);
        if (destroyed || dead) generatedPawn.health.SetDead();
        return generatedPawn;
    }
}