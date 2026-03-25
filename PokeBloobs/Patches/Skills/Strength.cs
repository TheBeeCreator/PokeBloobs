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
    internal class Strength
    {
        public static List<Item> _cachedStrengthSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedStrengthSouls != null) yield break;

            _cachedStrengthSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Strength", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = PokeBloobs.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedStrengthSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(StrengthSkill), "DropPets")]
        public class Patch_StrengthSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(3) && PokeBloobs.patchSkillrun[3])
                {
                    return;
                }

                if (_cachedStrengthSouls == null)
                {
                    Debug.Log("Strength souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedStrengthSouls)
                    {
                        Debug.Log($"Strength soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[3] = true;
                }

                Debug.Log($"Strength soul drop patches have been applied");
            }
        }
    }
}
