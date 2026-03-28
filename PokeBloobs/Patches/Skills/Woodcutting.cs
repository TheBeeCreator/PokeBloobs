using HarmonyLib;
using PokeBloobs.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PokeBloobs.Patches.Skills
{
    internal class Woodcutting
    {
        public static List<Item> _cachedWoodcuttingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedWoodcuttingSouls != null) yield break;

            _cachedWoodcuttingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Woodcutting", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = SoulBuilder.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedWoodcuttingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(WoodcuttingSkill), "DropPets")]
        public class Patch_WoodcuttingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(21) && PokeBloobs.patchSkillrun[21])
                {
                    return;
                }

                if (_cachedWoodcuttingSouls == null)
                {
                    Debug.Log("Woodcutting souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedWoodcuttingSouls)
                    {
                        Debug.Log($"Woodcutting soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[21] = true;
                }

                Debug.Log($"Woodcutting soul drop patches have been applied");

            }
        }
    }
}
