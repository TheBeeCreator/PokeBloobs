using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Classes
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

                //const float dropRate = 0.01f; // 1%
                const float dropRate = 1.00f; // 100%
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
        public class RuntimePetProjectile : MonoBehaviour
        {
            public BasicEnemy target;
            public float speed = 8f;
            public float hitDistance = 0.2f;
            public float lifeTime = 3f;
            public float damage = 0f;
            public float accuracy = 1f;

            private float timer;
            private bool hasHit;

            public void Initialize(BasicEnemy enemy, float moveSpeed = 8f, float damage = 0f, float accuracy = 1f)
            {
                target = enemy;
                speed = moveSpeed;
                this.damage = damage;
                this.accuracy = Mathf.Clamp01(accuracy);

                Vector3 pos = transform.position;
                pos.z = 0f;
                transform.position = pos;
            }

            void Update()
            {
                if (hasHit)
                    return;

                timer += Time.deltaTime;
                if (timer >= lifeTime)
                {
                    Destroy(gameObject);
                    return;
                }

                if (target == null || IsTargetInvalid(target))
                {
                    Destroy(gameObject);
                    return;
                }

                Vector3 targetPos = target.transform.position;
                targetPos.z = 0f;

                Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                newPos.z = 0f;
                transform.position = newPos;

                Vector3 dir = targetPos - transform.position;
                if (dir.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }

                if (Vector3.Distance(transform.position, targetPos) <= hitDistance)
                {
                    hasHit = true;

                    if (!IsTargetInvalid(target) && damage > 0f)
                    {
                        bool landed = UnityEngine.Random.value <= accuracy;

                        if (landed)
                        {
                            target.TakeDamage(damage, Color.cyan);
                        }
                        else
                        {
                            DamageNumberController.Instance.CreateNumber(0f, Color.white, target.transform.position);
                        }
                    }

                    Destroy(gameObject);
                }
            }

            private bool IsTargetInvalid(BasicEnemy enemy)
            {
                return enemy == null || enemy.GetCurrentHealth() <= 0f || enemy.IsRespawning;
            }
        }
    }
}
