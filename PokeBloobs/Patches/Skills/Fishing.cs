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
    internal class Fishing
    {
        public static List<Item> _cachedFishingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedFishingSouls != null) yield break;

            _cachedFishingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Fishing", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = SoulBuilder.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedFishingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(FishingSkill), "DropPets")]
        public class Patch_FishingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(19) && PokeBloobs.patchSkillrun[19])
                {
                    return;
                }

                if (_cachedFishingSouls == null)
                {
                    Debug.Log("Fishing souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedFishingSouls)
                    {
                        Debug.Log($"Fishing soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[19] = true;
                }

                Debug.Log($"Fishing soul drop patches have been applied");

            }
        }
    }
}