using HarmonyLib;
using Steamworks.Ugc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Patches
{
    internal class CombatManagerP
    {
        public static Dictionary<string, SoulsData> SoulLookup = new Dictionary<string, SoulsData>(StringComparer.OrdinalIgnoreCase);

        public static void BuildLookup()
        {
            if (SoulsDatabase.LoadedSouls == null || SoulsDatabase.LoadedSouls.Count == 0)
            {
                Debug.LogWarning("[PokeBloobs] BuildLookup called, but LoadedSouls is empty.");
                return;
            }

            SoulLookup = SoulsDatabase.LoadedSouls
                .Where(s => s != null && !string.IsNullOrEmpty(s.soulName))
                .ToDictionary(s => s.soulName, StringComparer.OrdinalIgnoreCase);

            Debug.Log($"[PokeBloobs] Built SoulLookup with {SoulLookup.Count} entries.");
        }

        public static string GetAttackAnimationForPet(string petName)
        {
            if (string.IsNullOrEmpty(petName))
                return "BasicOrb";

            petName = petName.Replace("(Clone)", "").Trim();

            if (SoulLookup == null || SoulLookup.Count == 0)
            {
                BuildLookup();
            }

            if (SoulLookup != null && SoulLookup.TryGetValue(petName, out var mon))
            {
                if (!string.IsNullOrEmpty(mon.PrimaryType))
                    return mon.PrimaryType;
            }

            return "BasicOrb";
        }

        [HarmonyPatch(typeof(CombatManager), "PerformAttack")]
        public static class CombatManager_PerformAttack_Patch
        {
            static void Postfix(CombatManager __instance)
            {
                if (__instance == null || __instance.currentTargetEnemy == null)
                    return;

                if (PetManager.Instance == null || PetManager.Instance.activePets == null)
                    return;

                BasicEnemy enemy = __instance.currentTargetEnemy;
                if (enemy.GetCurrentHealth() <= 0f || enemy.IsRespawning)
                    return;

                if (ModSettings.SelectedVersion == ModVersionMode.Chaotic)
                {
                    foreach (GameObject pet in PetManager.Instance.activePets)
                    {
                        if (pet == null)
                            continue;

                        Vector3 startPos = pet.transform.position + new Vector3(0.4f, 0.4f, 0f);
                        string attackAnim = GetAttackAnimationForPet(pet.name);

                        float highestDamage = Mathf.Max(
                            __instance.strengthSkill?.StrengthLevel ?? 0f,
                            __instance.rangeSkill?.RangeLevel ?? 0f,
                            __instance.magicSkill?.MagicLevel ?? 0f
                        );

                        //Default 10%
                        float petDamage = Mathf.Max(0.25f, highestDamage * 0.03f);

                        float highestAccuracy = Mathf.Max(
                            __instance.attackSkill?.accuracy ?? 0f,
                            __instance.rangeSkill?.accuracy ?? 0f,
                            __instance.magicSkill?.accuracy ?? 0f
                        );

                        //pets use 35% of your best combat accuracy, with a floor of 60%
                        float petAccuracy = Mathf.Clamp(highestAccuracy * 0.03f, 0.6f, 0.95f);

                        //Debug.Log($"Soul Damage {petDamage} Accuracy {petAccuracy}");

                        float delay = UnityEngine.Random.Range(0.05f, 0.35f);

                        __instance.StartCoroutine(DelayedPetAttack(
                            attackAnim,
                            startPos,
                            __instance.currentTargetEnemy,
                            10f,
                            petDamage,
                            petAccuracy,
                            delay
                        ));
                    }
                }
            }
        }

        private static IEnumerator DelayedPetAttack(
            string attackAnim,
            Vector3 startPos,
            BasicEnemy enemy,
            float speed,
            float damage,
            float accuracy,
            float delay)
        {
            yield return new WaitForSeconds(delay);

            if (enemy == null || enemy.GetCurrentHealth() <= 0f)
                yield break;

            PetProjectileFactory.SpawnAnimatedProjectile(
                attackAnim,
                startPos,
                enemy,
                speed,
                damage,
                accuracy
            );
        }
    }
}