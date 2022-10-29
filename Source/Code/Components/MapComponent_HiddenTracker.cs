using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Vampire;

public class MapComponent_HiddenTracker : MapComponent
{
    public HashSet<Pawn> hiddenCharacters = new();
    public HashSet<Pawn> toRemoveCharacters = new();

    public MapComponent_HiddenTracker(Map map) : base(map)
    {
    }

    public void Notify_AddHiddenCharacter(Pawn p)
    {
        if (hiddenCharacters.Contains(p)) return;
        hiddenCharacters.Add(p);
    }

    public void Notify_RemoveHiddenCharacter(Pawn p)
    {
        if (hiddenCharacters.Contains(p))
        {
            hiddenCharacters.Remove(p);
            p.Drawer.renderer.graphics.nakedGraphic = null;
            p.Drawer.renderer.graphics.ResolveAllGraphics();
            toRemoveCharacters.Add(p);
            FleckMaker.ThrowAirPuffUp(p.PositionHeld.ToVector3(), p.MapHeld);
        }
    }
}