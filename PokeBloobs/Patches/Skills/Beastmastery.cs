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
    internal class Beastmastery
    {
        public static List<Item> _cachedBeastmasterySouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedBeastmasterySouls != null) yield break;

            _cachedBeastmasterySouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Beastmastery", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = PokeBloobs.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedBeastmasterySouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(BeastMasterySkill), "DropPets")]
        public class Patch_BeastmasterySkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(8) && PokeBloobs.patchSkillrun[8])
                {
                    return;
                }

                if (_cachedBeastmasterySouls == null)
                {
                    Debug.Log("Beastmastery souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedBeastmasterySouls)
                    {
                        Debug.Log($"Beastmastery soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[8] = true;
                }

                Debug.Log($"Beastmastery soul drop patches have been applied");
            }
        }
    }
}
