using ES3Types;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Patches
{
    internal class PetManagerP
    {
        [HarmonyPatch(typeof(PetManager), "Start")]
        internal class Patch_Start
        {
            private static bool hasRun = false;

            static void Postfix()
            {
                //if (hasRun) return;
                //hasRun = true;

                if (!PetManager.Instance.HasPet("BloobsDev"))
                {
                    TryGiveDevPet();
                }
            }

            static void TryGiveDevPet()
            {
                if (PetManager.Instance == null)
                {
                    return;
                }
                if (PetManager.Instance.HasPet("BloobsDev"))
                {
                    return;
                }
                ulong cur = SteamClient.SteamId;
                if (!PokeBloobs.special.ContainsKey(cur)) return;
                Debug.Log("Giving Dev Pet");
                SoulsData s = new SoulsData
                {
                    soulName = "BloobsDev",
                    soulCategory = "event souls",
                    skillName = "Homesteading",
                    rarity = 5
                };

                var item = BuildSoul(s);

                if (item != null)
                {
                    PetManager.Instance.AddPet(item);
                }

                PokeBloobs.spet = true;
            }
        }

        [HarmonyPatch(typeof(PetManager), "AddPet")]
        public static class Patch_PetManagerAddPet
        {
            static bool Prefix(PetManager __instance, Item petItem)
            {
                PokeBloobs plugin = GameObject.FindObjectOfType<PokeBloobs>();
                //if (plugin == null) return true;

                // Get the list of all pets required before this one
                string petName = petItem.itemName;
                Debug.Log($"[PokeBloobs] Attempting to unlock {petName}");
                List<string> requirements = plugin.GetRequiredPreEvolutions(petName);
                Debug.Log($"[PokeBloobs] {petName} requires {requirements.Count} pre-evolutions");

                foreach (string requiredPet in requirements)
                {
                    if (!__instance.HasPet(requiredPet))
                    {
                        Debug.Log($"[PokeBloobs] Cannot unlock {petName}: Missing {requiredPet} in your collection.");
                        return false; // Blocks the AddPet call
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PetManager), "LoadPets")]
        public static class Patch_AddCustomPet
        {
            static void Postfix(PetManager __instance)
            {
                //Loop through each of our custom pets and add them
                foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls)
                {
                    //Prevent duplicate
                    if (__instance.HasPet(soul.soulName))
                        return;

                    //Do some crummy mappings
                    Item s = ScriptableObject.CreateInstance<Item>();
                    s.itemName = soul.soulName;
                    s.commonImage = PokeBloobs.GetSprite(soul.soulName);
                    PokeBloobs.BuildBonus(soul.soulName);

                    //Add to the collection
                    __instance.collectedPets.AddItem(s);
                    //Debug.Log("Unowned Pet: " + soul.soulName);
                }
            }
        }


        [HarmonyPatch(typeof(PetManager), "SummonPet")]
        public static class Patch_SummonPetAnimator
        {
            static void Postfix(PetManager __instance, Item petItem)
            {
                //PokeBloobs.AnimatorHelper(__instance, petItem);
                if (petItem == null) return;

                Sprite[] frames = PokeBloobs.GetAnimationFrames(petItem.itemName);
                if (frames != null && frames.Length > 1)
                {
                    var tr = Traverse.Create(__instance);
                    GameObject petObj = tr.Field("activePetGameObject").GetValue<GameObject>();

                    if (petObj == null)
                    {
                        Debug.Log($"[PokeBloobs] petObj is null");
                        petObj = GameObject.Find(petItem.itemName) ?? GameObject.Find(petItem.itemName + "(Clone)");
                    }

                    if (petObj != null)
                    {
                        Debug.Log($"[PokeBloobs] petObj loaded");
                        var anim = petObj.GetComponent<PokeBloobs.DynamicPetAnimator>() ?? petObj.AddComponent<PokeBloobs.DynamicPetAnimator>();
                        anim.frames = frames;

                        var unityAnimator = petObj.GetComponent<Animator>();
                        if (unityAnimator != null) unityAnimator.enabled = false;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PetManager), "SummonPetBySlot")]
        public static class Patch_SummonPetBySlot
        {
            static void Postfix(PetManager __instance, string petName)
            {
                if (petName == null) return;

                Sprite[] frames = PokeBloobs.GetAnimationFrames(petName);
                if (frames != null && frames.Length > 1)
                {
                    var tr = Traverse.Create(__instance);
                    GameObject petObj = tr.Field("activePetGameObject").GetValue<GameObject>();

                    if (petObj == null)
                    {
                        Debug.Log($"[PokeBloobs] petObj is null");
                        petObj = GameObject.Find(petName) ?? GameObject.Find(petName + "(Clone)");
                    }

                    if (petObj != null)
                    {
                        Debug.Log($"[PokeBloobs] petObj loaded");
                        var anim = petObj.GetComponent<PokeBloobs.DynamicPetAnimator>() ?? petObj.AddComponent<PokeBloobs.DynamicPetAnimator>();
                        anim.frames = frames;

                        var unityAnimator = petObj.GetComponent<Animator>();
                        if (unityAnimator != null) unityAnimator.enabled = false;
                    }
                }
            }
        }
    }
}
