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
    internal class Thieving
    {
        public static List<Item> _cachedThievingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedThievingSouls != null) yield break;

            _cachedThievingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Thieving", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = SoulBuilder.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedThievingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(ThievingSkill), "DropPets")]
        public class Patch_ThievingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(15) && PokeBloobs.patchSkillrun[15])
                {
                    return;
                }

                if (_cachedThievingSouls == null)
                {
                    Debug.Log("Thieving souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedThievingSouls)
                    {
                        Debug.Log($"Thieving soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[15] = true;
                }

                Debug.Log($"Thieving soul drop patches have been applied");
            }
        }
    }
}
