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
    internal class Firemaking
    {
        public static List<Item> _cachedFiremakingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedFiremakingSouls != null) yield break;

            _cachedFiremakingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Firemaking", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = PokeBloobs.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedFiremakingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(FiremakingSkill), "DropPets")]
        public class Patch_FiremakingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(22) && PokeBloobs.patchSkillrun[22])
                {
                    return;
                }

                if (_cachedFiremakingSouls == null)
                {
                    Debug.Log("Firemaking souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedFiremakingSouls)
                    {
                        Debug.Log($"Firemaking soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[22] = true;
                }

                Debug.Log($"Firemaking soul drop patches have been applied");

            }
        }
    }
}
