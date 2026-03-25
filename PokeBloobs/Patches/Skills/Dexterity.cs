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
    internal class Dexterity
    {
        public static List<Item> _cachedDexteritySouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedDexteritySouls != null) yield break;

            _cachedDexteritySouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Dexterity", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = PokeBloobs.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedDexteritySouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(DexteritySkill), "DropPets")]
        public class Patch_DexteritySkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(9) && PokeBloobs.patchSkillrun[9])
                {
                    return;
                }

                if (_cachedDexteritySouls == null)
                {
                    Debug.Log("Dexterity souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedDexteritySouls)
                    {
                        Debug.Log($"Dexterity soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[9] = true;
                }

                Debug.Log($"Dexterity soul drop patches have been applied");
            }
        }
    }
}
