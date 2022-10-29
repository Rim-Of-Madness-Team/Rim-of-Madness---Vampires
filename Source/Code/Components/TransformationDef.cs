using System.Collections.Generic;
using Verse;

namespace Vampire;

public class TransformationDef : Def
{
    public float baseBodySize = 1.0f;
    public float baseHealthScale = 1.0f;
    public GraphicData bodyGraphicData = null;
    public List<CompProperties> comps = new();

    public T GetCompProperties<T>() where T : CompProperties
    {
        for (var i = 0; i < comps.Count; i++)
        {
            var t = comps[i] as T;
            if (t != null) return t;
        }

        return null;
    }
}