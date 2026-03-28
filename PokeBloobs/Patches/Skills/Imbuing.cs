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
    internal class Imbuing
    {
        public static List<Item> _cachedImbuingSouls;

        public static IEnumerator CacheSoulsCoroutine()
        {
            if (_cachedImbuingSouls != null) yield break;

            _cachedImbuingSouls = new List<Item>();
            var Souls = PokeBloobs.SoulsDatabase.LoadedSouls
                            .Where(n => n.skillName.IndexOf("Imbuing", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var soul in Souls)
            {
                Item c = SoulBuilder.BuildSoul(soul);
                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                _cachedImbuingSouls.Add(c);

                yield return null;
            }
            Debug.Log("Cache Complete!");
        }

        [HarmonyPatch(typeof(ImbuingSkill), "DropPets")]
        public class Patch_ImbuingSkill
        {
            static void Postfix(ref List<Item> ___petDrops)
            {
                if (PokeBloobs.patchSkillrun.ContainsKey(14) && PokeBloobs.patchSkillrun[14])
                {
                    return;
                }

                if (_cachedImbuingSouls == null)
                {
                    Debug.Log("Imbuing souls not cached yet. Starting background cache...");
                    TaskDispatcher.RunCoroutine(CacheSoulsCoroutine());

                    //_ = PreCacheHitpointsSouls();
                    //return;
                }
                else
                {
                    // Inject the cached items if they are ready
                    foreach (var soulItem in _cachedImbuingSouls)
                    {
                        Debug.Log($"Imbuing soul pet {soulItem.name} with rarity: {soulItem.dropChance} has been added to the queue and drop table");
                        ___petDrops.Add(soulItem);
                        TaskDispatcher.Enqueue(() => PokeBloobs.UpdateSinglePet(soulItem.name));
                    }
                    PokeBloobs.patchSkillrun[14] = true;
                }

                Debug.Log($"Imbuing soul drop patches have been applied");
            }
        }
    }
}
