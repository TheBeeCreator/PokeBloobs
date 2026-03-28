using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Patches
{
    internal class SoulCompendiumManagerP
    {
        [HarmonyPatch(typeof(SoulCompendiumManager), "GetPetSprite")]
        public static class Patch_CompendiumSpriteFix
        {
            static bool Prefix(string petName, ref Sprite __result)
            {
                var customSoul = PokeBloobs.SoulsDatabase.LoadedSouls
                    .FirstOrDefault(s => s.soulName.Equals(petName, StringComparison.OrdinalIgnoreCase));

                if (customSoul != null)
                {
                    var sprite = PokeBloobs.GetSprite(customSoul.soulName);
                    __result = ResizeSprite(sprite, 2f);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SoulCompendiumManager), "LoadPetList")]
        public static class Patch_SoulCompendiumPatch
        {
            public static void Postfix(SoulCompendiumManager __instance,
                ref List<string> ___petNames,
                ref Dictionary<string, string> ___petNameLookup,
                ref Dictionary<string, List<string>> ___categorizedBlocks)
            {

                var tr = Traverse.Create(__instance);

                var petNames = tr.Field("petNames").GetValue<List<string>>();
                var petNameLookup = tr.Field("petNameLookup").GetValue<Dictionary<string, string>>();
                var categorizedBlocks = tr.Field("categorizedBlocks").GetValue<Dictionary<string, List<string>>>();

                if (petNames == null || petNameLookup == null || categorizedBlocks == null)
                {
                    return;
                }

                foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls)
                {
                    //Prevent dupes
                    if (!petNames.Contains(soul.soulName))
                    {
                        petNames.Add(soul.soulName);
                        petNameLookup[soul.soulName] = soul.soulName;

                        // Add to category for UI filtering
                        if (categorizedBlocks.ContainsKey(soul.soulCategory))
                        {
                            categorizedBlocks[soul.soulCategory].Add(soul.soulName);
                        }
                        else
                        {
                            // Fallback to uncategorized if the key doesn't exist
                            categorizedBlocks["uncategorized"].Add(soul.soulName);
                        }

                        __instance.UpdateSinglePetUI(soul.soulName);

                        //Debug.Log($"[PokeBloobs] Successfully injected {soul.soulName} into Compendium.");
                    }

                    //Debug.Log($"[PokeBloobs] Added {soul.soulName} to Soul Compendium.");
                }
            }
        }

        private static Sprite ResizeSprite(Sprite original, float scale)
        {
            if (original == null) return null;

            return Sprite.Create(
                original.texture,
                original.rect,
                original.pivot,
                original.pixelsPerUnit / scale
            );
        }
    }
}
