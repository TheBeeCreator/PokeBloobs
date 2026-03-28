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
    internal class Homesteading
    {
        public static List<Item> _cachedHomesteadingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedHomesteadingSouls != null) yield break;

            _cachedHomesteadingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Homesteading", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = SoulBuilder.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedHomesteadingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(HomeSteadingSkill), "DropPets")]
        public class Patch_HomesteadingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(24) && PokeBloobs.patchSkillrun[24])
                {
                    return;
                }

                if (_cachedHomesteadingSouls == null)
                {
                    Debug.Log("Homesteading souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedHomesteadingSouls)
                    {
                        Debug.Log($"Homesteading soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[24] = true;
                }

                Debug.Log($"Homesteading soul drop patches have been applied");
            }
        }
    }
}
