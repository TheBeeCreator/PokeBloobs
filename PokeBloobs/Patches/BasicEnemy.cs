using HarmonyLib;
using PokeBloobs.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Patches
{
    internal class BasicEnemyP
    {
        [HarmonyPatch(typeof(BasicEnemy), "Start")]
        public static class AddUnownPetsPatch
        {
            static void Postfix(BasicEnemy __instance)
            {
                var component = __instance as Component;
                if (component == null)
                    return;

                if (!component.gameObject.name.Contains("Grim"))
                    return;

                if (SoulsDatabase.LoadedSouls == null)
                    return;

                if (__instance.petDrops == null)
                    __instance.petDrops = new List<Item>();

                foreach (var soul in SoulsDatabase.LoadedSouls)
                {
                    if (soul == null)
                        continue;

                    if (!soul.soulName.Contains("Unown"))
                        continue;

                    Item petItem = SoulBuilder.BuildSoul(soul);

                    if (petItem != null)
                    {
                        //const float dropRate = 0.01f; // 1%
                        //const float dropRate = 1.00f; // 100%

                        petItem.dropChance = 0.01f;
                        __instance.petDrops.Add(petItem);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(BasicEnemy), "DropPets")]
        public static class PetDropPatch
        {
            static void Postfix(object __instance)
            {
                var component = __instance as Component;
                if (component == null)
                    return;

                string enemyName = component.gameObject.name;
                if (!enemyName.Contains("Grim"))
                    return;

                const float dropRate = 0.01f; // 1%
                //const float dropRate = 1.00f; // 100%
                if (UnityEngine.Random.value > dropRate)
                    return;

                SoulsData soulData = FindSoulByName("Unown");
                if (soulData == null)
                {
                    Debug.LogError("[PetDropPatch] Could not find SoulsData for Unown");
                    return;
                }

                Item petItem = SoulBuilder.BuildSoul(soulData);
                if (petItem == null)
                {
                    Debug.LogError("[PetDropPatch] BuildSoul returned null for Unown");
                    return;
                }

                PetManager.Instance.AddPet(petItem);
                Debug.Log("[PetDropPatch] Awarded Unown pet");
            }

            private static SoulsData FindSoulByName(string soulName)
            {
                if (SoulsDatabase.LoadedSouls == null)
                    return null;

                foreach (var soul in SoulsDatabase.LoadedSouls)
                {
                    if (soul == null)
                        continue;

                    if (soul.soulName == soulName)
                        return soul;
                }

                return null;
            }
        }
    }
}
