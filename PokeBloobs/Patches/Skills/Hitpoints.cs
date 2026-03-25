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
    internal class Hitpoints
    {
        public static List<Item> _cachedHitpointsSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedHitpointsSouls != null) yield break;

            _cachedHitpointsSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Hitpoints", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = PokeBloobs.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedHitpointsSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(HitPointsSkill), "DropPets")]
        public class Patch_HitPointsSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(1) && PokeBloobs.patchSkillrun[1])
                {
                    return;
                }

                if (_cachedHitpointsSouls == null)
                {
                    Debug.Log("Hitpoints souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedHitpointsSouls)
                    {
                        Debug.Log($"Hitpoints soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[1] = true;
                }

                Debug.Log($"Hitpoints soul drop patches have been applied");
            }
        }
    }
}
