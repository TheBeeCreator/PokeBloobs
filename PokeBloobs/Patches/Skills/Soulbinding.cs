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
    internal class Soulbinding
    {
        public static List<Item> _cachedSoulbindingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedSoulbindingSouls != null) yield break;

            _cachedSoulbindingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Soulbinding", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = SoulBuilder.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedSoulbindingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(SoulBinding), "DropPets")]
        public class Patch_SoulbindingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(16) && PokeBloobs.patchSkillrun[16])
                {
                    return;
                }

                if (_cachedSoulbindingSouls == null)
                {
                    Debug.Log("Soulbinding souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedSoulbindingSouls)
                    {
                        Debug.Log($"Soulbinding soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[16] = true;
                }

                Debug.Log($"Soulbinding soul drop patches have been applied");
            }
        }
    }
}
