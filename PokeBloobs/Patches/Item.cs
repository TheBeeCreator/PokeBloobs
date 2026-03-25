using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Patches
{
    internal class Items
    {
        [HarmonyPatch(typeof(Item), "GetAllItems")]
        public static class Patch_ItemRegistry
        {
            static void Postfix(ref List<Item> __result)
            {
                foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls)
                {
                    //Check for dupes
                    if (!__result.Any(i => i.itemName == soul.soulName))
                    {
                        Item i = PokeBloobs.BuildSoul(soul);
                        __result.Add(i);
                    }
                }
            }
        }
    }
}
