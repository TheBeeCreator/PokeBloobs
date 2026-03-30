using ES3Types;
using HarmonyLib;
using Pathfinding.Util;
using PokeBloobs.Classes;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.Classes.MenuModVersion;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Patches
{
    internal class PetManagerP
    {
        internal static bool showPrompt = false;

        [HarmonyPatch(typeof(PetManager), "Start")]
        internal class Patch_Start
        {
            static void Postfix()
            {
                Debug.Log("[PokeBloobs] PetManager.Start postfix fired");

                SaveHelpers.LoadVersionSetting();
                Debug.Log("[PokeBloobs] Loaded version setting");



                SaveHelpers.RefreshCustomPetBonuses();
                SaveHelpers.RefreshActivePetInstances();

                if (PetManager.Instance != null)
                {
                    PetManager.Instance.ReapplyAllPetBonuses();
                    PetManager.Instance.UpdatePetBuffsDisplay();
                }

                SoulCompendiumManager.Instance?.UpdateActivePetsUI();
                SoulCompendiumManager.Instance?.UpdateCompendiumUI();
                Debug.Log("[PokeBloobs] Called RefreshCustomPetBonuses");

                SaveHelpers.LoadStarterSetting();
                Debug.Log("[PokeBloobs] Loaded starter setting");

                if (!ModSettings.HasChosenVersion)
                {
                    if (GameObject.Find("VersionSelectUI") == null)
                        new GameObject("VersionSelectUI").AddComponent<VersionSelectUI>();

                    VersionSelectUI.ShowPrompt = true;
                    return;
                }

                if (!ModSettings.HasChosenStarter)
                {
                    if (GameObject.Find("PetPromptUI") == null)
                        new GameObject("PetPromptUI").AddComponent<PetPromptUI>();

                    PetManagerP.showPrompt = true;

                    if (!PetManager.Instance.HasPet("BloobsDev"))
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
                SoulsData s = SoulsDatabase.LoadedSouls
                    .FirstOrDefault(x => x.soulName.Equals("BloobsDev", StringComparison.OrdinalIgnoreCase));

                if (s == null)
                {
                    Debug.LogWarning("[PokeBloobs] BloobsDev soul missing from JSON.");
                    return;
                }

                var item = SoulBuilder.BuildSoul(s);

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
                Debug.Log("[PokeBloobs] LoadPets postfix fired");

                foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls)
                {
                    if (__instance.HasPet(soul.soulName))
                        continue;

                    Item item = SoulBuilder.BuildSoul(soul);
                    if (item == null)
                        continue;

                    __instance.collectedPets.AddItem(item);
                }

                Debug.Log("[PokeBloobs] Calling refresh after LoadPets");
                SaveHelpers.RefreshCustomPetBonuses();
            }
        }

        [HarmonyPatch(typeof(PetManager), "LoadPets")]
        public static class Patch_PetManager_LoadPets_Resummon
        {
            static void Postfix(PetManager __instance)
            {
                __instance.StartCoroutine(PetRefreshHelper.DelayedStartupResummon());
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
