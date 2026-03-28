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
    internal class Cooking
    {
        public static List<Item> _cachedCookingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedCookingSouls != null) yield break;

            _cachedCookingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Cooking", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = SoulBuilder.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedCookingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(CookingSkill), "DropPets")]
        public class Patch_CookingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(20) && PokeBloobs.patchSkillrun[20])
                {
                    return;
                }

                if (_cachedCookingSouls == null)
                {
                    Debug.Log("Cooking souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedCookingSouls)
                    {
                        Debug.Log($"Cooking soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[20] = true;
                }

                Debug.Log($"Cooking soul drop patches have been applied");

            }
        }
    }
}