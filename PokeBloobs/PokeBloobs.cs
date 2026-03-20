using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using ScriptableObject = UnityEngine.ScriptableObject;

namespace PokeBloobs
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class PokeBloobs : BaseUnityPlugin
    {

        public class SoulsDatabase
        {
            public static List<SoulsData> LoadedSouls = new List<SoulsData>();
        }
        public static Item Bulbasuar;
        public static Item soulinfo;

        private void Awake()
        {
            //Plugin startup logic
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} {MyPluginInfo.PLUGIN_VERSION} is installed and starting");

            var assembly = Assembly.GetExecutingAssembly();
            foreach (var name in assembly.GetManifestResourceNames())
            {
                Logger.LogInfo("Found resources: " + name);
            }

            //Load our resources
            string JSONContent = GetJsonFromResources("PokeBloobs.PokeBloobsSouls.json");

            if (!string.IsNullOrEmpty(JSONContent))
            {
                SoulsDatabase.LoadedSouls = JsonConvert.DeserializeObject<List<SoulsData>>(JSONContent);
                Logger.LogInfo($"Successfully loaded {SoulsDatabase.LoadedSouls.Count} souls from JSON.");
            }
            else
            {
                Logger.LogError("Failed to load PokeBloobSouls.json from resources!");
            }

            //harmony.PatchAll();
            Harmony.CreateAndPatchAll(typeof(Patch_ItemRegistry));
            Harmony.CreateAndPatchAll(typeof(Patch_AddCustomPet));
            Harmony.CreateAndPatchAll(typeof(Patch_SoulCompendiumPatch));

            //Patch individual skills for there respective pet drops
            Harmony.CreateAndPatchAll(typeof(Patch_Woodcuttingskill));
            Harmony.CreateAndPatchAll(typeof(Patch_Firemakingskill));
            Harmony.CreateAndPatchAll(typeof(Patch_Fishingskill));

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

        //Embedded Resources
        public string GetJsonFromResources(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            //More checks cause magic unicorns
            string manifest = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(resourceName));
            if (manifest == null) return null;


            using (Stream stream = assembly.GetManifestResourceStream(manifest))
            {
                if (stream == null) return null;
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        //Souls data builder
        public static Item BuildSoul(SoulsData s)
        {
            if (s == null) return null;
            var item = new Item();
            item.itemName = s.soulName;
            item.commonImage = GetSprite(s.soulName);
            
            return item;
        }

        //Skill bonus builder
        public static void BuildBonus(string s)
        { 
            if (s == null) return;
            //to finish/todo (special case for hard to obtain things)
            string[] uniques = { "Zapdos", "Articuno", "Birthday Creator" };
            if (uniques.Contains(s))
            {
                switch (s)
                {
                    case "":
                        PokeBloobs.soulinfo.hitPointsBonusXp = 0; break;
                }
            }
            else
            {
                switch (s)
                {
                    case "Hitpoints":
                        PokeBloobs.soulinfo.hitPointsBonusXp = 0.10f; break;
                    case "Attack":
                        PokeBloobs.soulinfo.attackBonusXP = 0.10f; break;
                    case "Strength":
                        PokeBloobs.soulinfo.strengthBonusXp = 0.10f; break;
                    case "Defense":
                        PokeBloobs.soulinfo.defenceBonusXP = 0.10f; break;
                    case "Ranged":
                        PokeBloobs.soulinfo.rangeBonusXP = 0.10f; break;
                    case "Magic":
                        PokeBloobs.soulinfo.magicBonusXP = 0.10f; break;
                    case "Devotion":
                        PokeBloobs.soulinfo.devotionBonusXp = 0.10f; break;
                    case "Beastmastery":
                        PokeBloobs.soulinfo.beastMateryBonusXp = 0.10f; break;
                    case "Dexterity":
                        PokeBloobs.soulinfo.dexterityBonusXp = 0.10f; break;
                    case "Foraging":
                        PokeBloobs.soulinfo.foragingBonusXp = 0.10f; break;
                    case "Herblore":
                        PokeBloobs.soulinfo.herbologyBonusXp = 0.10f; break;
                    case "Crafting":
                        PokeBloobs.soulinfo.craftingBonusXp = 0.10f; break;
                    case "Fletching":
                        PokeBloobs.soulinfo.bowCraftingBonusXp = 0.10f; break;
                    case "Imbuing":
                        PokeBloobs.soulinfo.imbuingBonusXp = 0.10f; break;
                    case "Thieving":
                        PokeBloobs.soulinfo.thievingBonusXp = 0.10f; break;
                    case "Soulbinding":
                        PokeBloobs.soulinfo.soulBindingBonusXp = 0.10f; break;
                    case "Mining":
                        PokeBloobs.soulinfo.miningBonusXp = 0.10f; break;
                    case "Smithing":
                        PokeBloobs.soulinfo.smithingBonusXp = 0.10f; break;
                    case "Fishing":
                        PokeBloobs.soulinfo.fishingBonusXp = 0.10f; break;
                    case "Cooking":
                        PokeBloobs.soulinfo.cookingBonusXp = 0.10f; break;
                    case "Woodcutting":
                        PokeBloobs.soulinfo.woodcuttingBonusXp = 0.10f; break;
                    case "Firemaking":
                        PokeBloobs.soulinfo.firemakingBonusXp = 0.10f; break;
                    case "Tracking":
                        PokeBloobs.soulinfo.trackingBonusXp = 0.10f; break;
                    case "Homesteading":
                        PokeBloobs.soulinfo.homesteadingBonusXp = 0.10f; break;
                }
            }
        }

        //Sprite grabber
        public static Sprite GetSprite(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            //string manifestName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.Contains(".Sprites.") && n.Contains(resourceName));
            string manifestName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.Contains("Sprites") && n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
            Debug.LogWarning(manifestName);

            if (string.IsNullOrEmpty(manifestName))
            {
                Debug.LogError($"[PokeBloobs] Resource not found: {resourceName}");
                return null;
            }

            using (Stream stream = assembly.GetManifestResourceStream(manifestName))
            {
                if (stream == null) return null;

                byte[] ba = new byte[stream.Length];
                stream.Read(ba, 0, ba.Length);

                Texture2D texture = new Texture2D(2, 2);
                if (ImageConversion.LoadImage(texture, ba))
                {
                    // Create Sprite: (Texture, Rect, Pivot)
                    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
            // TEMP: return null OR load from Resources if you want
            return null;
        }
    }

    //Specific skill patches
    [HarmonyPatch(typeof(WoodcuttingSkill), "DropPets")] // Replace with actual method name
    public class Patch_Woodcuttingskill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            //// Check if our custom item exists and isn't already in the list
            //if (PokeBloobs.Bulbasuar != null && !___petDrops.Contains(PokeBloobs.Bulbasuar))
            //{
            //    ___petDrops.Add(PokeBloobs.Bulbasuar);
            //}
            foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls.Where(n => n.skillName.Contains("Woodcutting")))
            {

            }
        }
    }

    [HarmonyPatch(typeof(FiremakingSkill), "DropPets")] // Replace with actual method name
    public class Patch_Firemakingskill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            //// Check if our custom item exists and isn't already in the list
            //if (PokeBloobs.Bulbasuar != null && !___petDrops.Contains(PokeBloobs.Bulbasuar))
            //{
            //    ___petDrops.Add(PokeBloobs.Bulbasuar);
            //}
            foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls.Where(n => n.skillName.Contains("Firemaking")))
            {

            }
        }
    }

    [HarmonyPatch(typeof(FishingSkill), "DropPets")] // Replace with actual method name
    public class Patch_Fishingskill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            //// Check if our custom item exists and isn't already in the list
            //if (PokeBloobs.Bulbasuar != null && !___petDrops.Contains(PokeBloobs.Bulbasuar))
            //{
            //    ___petDrops.Add(PokeBloobs.Bulbasuar);
            //}
            foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls.Where(n => n.skillName.Contains("Fishing")))
            {
                PokeBloobs.soulinfo = PokeBloobs.BuildSoul(soul);
                //Check for already existing in the list
                if (!___petDrops.Contains(PokeBloobs.soulinfo))
                {
                    ___petDrops.Add(PokeBloobs.soulinfo);
                }
            }
        }
    }

    //General Patches
    [HarmonyPatch(typeof(Item), "GetAllItems")]
    public static class Patch_ItemRegistry
    {
        static void Postfix(ref List<Item> __result)
        {
            //Loop through each of our custom pets and add them
            //foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls.Where(n => n.skillName = "")
            //{

            //}
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
                //__instance.collectedPets.Add(s);
                Debug.Log("Added pet: " + soul.soulName);
            }
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

            if (petNames == null || petNameLookup == null || categorizedBlocks == null)
            {
                return;
            }

            foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls)
            {
                //Prevent dupes
                if (!petNames.Contains(soul.soulName))
                {
                    petNames.Add(soul.soulName);
                    petNameLookup[soul.soulName] = soul.soulName;

                    // Add to category for UI filtering
                    if (categorizedBlocks.ContainsKey(soul.soulCategory))
                    {
                        categorizedBlocks[soul.soulCategory].Add(soul.soulName);
                    }
                    else
                    {
                        // Fallback to uncategorized if the key doesn't exist
                        categorizedBlocks["uncategorized"].Add(soul.soulName);
                    }

                    Debug.Log($"[PokeBloobs] Successfully injected {soul.soulName} into Compendium.");
                }

                Debug.Log($"[PokeBloobs] Added {soul.soulName} to Soul Compendium.");
            }
        }
    }

    [System.Serializable]
    public class SoulsData
    {
        public string soulName;
        public string soulCategory;
        public string skillName;
    }
}