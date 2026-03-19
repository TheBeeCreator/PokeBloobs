using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObject = UnityEngine.ScriptableObject;

namespace PokeBloobs
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class PokeBloobs : BaseUnityPlugin
    {
        public static Item Bulbasuar;


        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} {MyPluginInfo.PLUGIN_VERSION} is installed");

            //harmony.PatchAll();
            Harmony.CreateAndPatchAll(typeof(Patch_ItemRegistry));
            Harmony.CreateAndPatchAll(typeof(Patch_AddCustomPet));
            Harmony.CreateAndPatchAll(typeof(Patch_Woodcuttingskill));
            Harmony.CreateAndPatchAll(typeof(Patch_SoulCompendiumPatch));

            var patchedMethods = Harmony.GetAllPatchedMethods();
            //Make sure that harmory actually patched
            foreach (var method in patchedMethods)
            {
                Logger.LogInfo("Patched method: " + method.Name);
            }
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (PetManager.Instance != null)
                {
                    Logger.LogInfo("Attempting Bulb Add");
                    PetManager.Instance.AddPet(PokeBloobs.Bulbasuar);
                    Logger.LogInfo("Finished Bulb Add");
                }
            }
        }
    }

    //[cite_start]
    [HarmonyPatch(typeof(Item), "GetAllItems")]
    public static class Patch_ItemRegistry
    {
        static void Postfix(ref List<Item> __result)
        {
            if (PokeBloobs.Bulbasuar != null && !__result.Contains(PokeBloobs.Bulbasuar))
            {
                __result.Add(PokeBloobs.Bulbasuar);
            }
        }
    }

    [HarmonyPatch(typeof(WoodcuttingSkill), "DropPets")] // Replace with actual method name
    public class Patch_Woodcuttingskill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            // Check if our custom item exists and isn't already in the list
            if (PokeBloobs.Bulbasuar != null && !___petDrops.Contains(PokeBloobs.Bulbasuar))
            {
                ___petDrops.Add(PokeBloobs.Bulbasuar);
            }
        }
    }


    [HarmonyPatch(typeof(PetManager), "LoadPets")]
    public static class Patch_AddCustomPet
    {
        static void Postfix(PetManager __instance)
        {
            // Create custom pet
            Item bulbasuar = ScriptableObject.CreateInstance<Item>();
            bulbasuar.itemName = "Bulbasuar";
            bulbasuar.commonImage = GetSprite();

            // Example bonus
            bulbasuar.miningBonusXp = 0.10f;

            //Prevent duplicate
            if (__instance.HasPet(bulbasuar.itemName))
                return;

            // Add to collection
            __instance.collectedPets.Add(bulbasuar);

            Debug.Log("[Mod] Added custom pet: " + bulbasuar.itemName);

            // Summon it if possible
            __instance.SummonPet(bulbasuar);
        }

        static Sprite GetSprite()
        {
            // TEMP: return null OR load from Resources if you want
            return null;
        }
    }

    [HarmonyPatch(typeof(SoulCompendiumManager), "LoadPetList")]
    public static class Patch_SoulCompendiumPatch
    {
        public static void Postfix(SoulCompendiumManager __instance, 
            ref List<string> ___petNames, 
            ref Dictionary<string, string> ___petNameLookup,
            ref Dictionary<string, List<string>> ___categorizedBlocks)
        {

            var tr = Traverse.Create(__instance);

            var petNames = tr.Field("petNames").GetValue<List<string>>();
            var petNameLookup = tr.Field("petNameLookup").GetValue<Dictionary<string, string>>();
            var categorizedBlocks = tr.Field("categorizedBlocks").GetValue<Dictionary<string, List<string>>>();

            string entry = "999 - Bulbasuar";
            string petName = "Bulbasuar";
            string category = "event souls";

            if (petNames == null || petNameLookup == null || categorizedBlocks == null)
            {
                return;
            }

            //Prevent dupes
            if (!petNames.Contains(entry))
            {
                petNames.Add(entry);
                petNameLookup[entry] = petName;

                // Add to category for UI filtering
                if (categorizedBlocks.ContainsKey(category))
                {
                    categorizedBlocks[category].Add(entry);
                }
                else
                {
                    // Fallback to uncategorized if the key doesn't exist
                    categorizedBlocks["uncategorized"].Add(entry);
                }

                Debug.Log($"[PokeBloobs] Successfully injected {petName} into Compendium.");
            }

            Debug.Log($"[PokeBloobs] Added {petName} to Soul Compendium.");
        }
    }
}