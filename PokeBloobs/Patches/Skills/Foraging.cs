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
    internal class Foraging
    {
        public static List<Item> _cachedForagingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedForagingSouls != null) yield break;

            _cachedForagingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Foraging", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = PokeBloobs.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedForagingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(ForagingSkill), "DropPets")]
        public class Patch_ForagingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(10) && PokeBloobs.patchSkillrun[10])
                {
                    return;
                }

                if (_cachedForagingSouls == null)
                {
                    Debug.Log("Foraging souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedForagingSouls)
                    {
                        Debug.Log($"Foraging soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[10] = true;
                }

                Debug.Log($"Foraging soul drop patches have been applied");
            }
        }
    }
}
