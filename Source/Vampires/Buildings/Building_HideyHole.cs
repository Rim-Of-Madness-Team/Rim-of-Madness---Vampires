using RimWorld;

namespace Vampire.Buildings
{
    public class Building_HideyHole : Building_Grave
    {
        public override void Open()
        {
            base.Open();
            Destroy();
        }
    }
}
