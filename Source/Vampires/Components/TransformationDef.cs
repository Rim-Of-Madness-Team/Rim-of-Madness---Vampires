using System.Collections.Generic;
using Verse;

namespace Vampire
{
    public class TransformationDef : Def
    {
        public float baseBodySize = 1.0f;
        public float baseHealthScale = 1.0f;
	    public List<CompProperties> comps = new List<CompProperties>();		
	    public GraphicData bodyGraphicData = null;

	    public T GetCompProperties<T>() where T : CompProperties
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				T t = this.comps[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}
    }
}