using System.Linq;
using System.Xml;
using Verse;

namespace Vampire
{
    public class PatchOperationModLoaded : PatchOperation
    {
        private string modName;
        
        protected override bool ApplyWorker(XmlDocument xml)
        {
            return !modName.NullOrEmpty() && ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == modName);
        }
    }
}