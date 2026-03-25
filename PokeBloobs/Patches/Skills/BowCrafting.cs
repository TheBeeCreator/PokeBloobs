using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PokeBloobs.Patches.Skills
{
    internal class BowCrafting
    {
        public static List<Item> _cachedBowCraftingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedBowCraftingSouls != null) yield break;

            _cachedBowCraftingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("BowCrafting", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = PokeBloobs.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedBowCraftingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(BowCraftingSkill), "DropPets")]
        public class Patch_BowCraftingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(13) && PokeBloobs.patchSkillrun[13])
                {
                    return;
                }

                if (_cachedBowCraftingSouls == null)
                {
                    Debug.Log("BowCrafting souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedBowCraftingSouls)
                    {
                        Debug.Log($"BowCrafting soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[13] = true;
                }

                Debug.Log($"BowCrafting soul drop patches have been applied");
            }
        }
    }
}
