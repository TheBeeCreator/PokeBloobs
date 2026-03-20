using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft;
using Newtonsoft.Json;
using Pathfinding;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using static PokeBloobs.PokeBloobs;
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
        public class SoulsData
        {
            public string soulName;
            public string soulCategory;
            public string skillName;
            public int rarity;
        }
        public static Item soulinfo;

        //Cause ya know
        private Dictionary<string, string[]> evolutionChains = new Dictionary<string, string[]>
        {
            { "Bulbasaur", new[] { "Bulbasaur", "Ivysaur", "Venusaur" } },
            { "Charmander", new[] { "Charmander", "Charmeleon", "Charizard" } },
            { "Squirtle", new[] { "Squirtle", "Wartortle", "Blastoise" } },

            { "Pikachu", new[] { "Pikachu", "Raichu" } },

            { "Magikarp", new[] { "Magikarp", "Gyarados" } },

            // etc...
        };

        public static Dictionary<int, bool> patchSkillrun = new Dictionary<int, bool>
        {
            {1, false }, //Hitpoints
            {2, false }, //Attack
            {3, false }, //Strength
            {4, false }, //Defense
            {5, false }, //Ranged
            {6, false }, //Magic
            {7, false }, //Devo
            {8, false }, //Beastmastery

            {9, false }, //Dex
            {10, false }, //Foraging
            {11, false }, //Herb
            {12, false }, //Crafting
            {13, false }, //Bow Crafting
            {14, false }, //Imbuing
            {15, false }, //Thieving
            {16, false }, //Soulbinding

            {17, false }, //Mining
            {18, false }, //Smithing
            {19, false }, //Fishing
            {20, false }, //Cooking
            {21, false }, //Woodcutting
            {22, false }, //Firemaking
            {23, false }, //Tracking
            {24, false }, //Homesteading
        };

        //Sprite cache
        private static Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        //Caches
        public static List<Item> _cachedWoodcuttingSouls;

        public static Item mon = ScriptableObject.CreateInstance<Item>();


        //Commons
        public static string[] pCommon;
        //Uncommons (slightly more rare than commons)
        public static string[] pUncommon;
        //Rares (should be rare, but not hard)
        public static string[] pRare;
        //Ultra rares (should be hard but obtainable)
        public static string[] pUltrarare;
        //Mythic mons (should be hard to get)
        public static string[] pMythic;
        //Hidden mons (basically god tier hidden)
        public static string[] pGodTier;

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
                PokeBloobs.PopulateRarityArrays();
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
            Harmony.CreateAndPatchAll(typeof(Patch_CompendiumSpriteFix));
            Harmony.CreateAndPatchAll(typeof(Patch_SummonPetAnimator));
            Harmony.CreateAndPatchAll(typeof(Patch_SummonPetBySlot));

            //Patch individual skills for there respective pet drops
            Harmony.CreateAndPatchAll(typeof(Patch_HitPointsSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_AttackSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_StrengthSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_DefenseSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_RangedSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_MagicSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_DevotionSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_BeastMasterySkill));

            Harmony.CreateAndPatchAll(typeof(Patch_Woodcuttingskill));

            var patchedMethods = Harmony.GetAllPatchedMethods();
            //Make sure that harmory actually patched
            foreach (var method in patchedMethods)
            {
                Logger.LogInfo("Patched method: " + method.Module + " : " + method.Name);
            }
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (PetManager.Instance != null)
                {

                }
            }

            //SavePokeBloobs();
            if (SoulCompendiumManager.Instance != null && SoulCompendiumManager.Instance.IsInitialized)
            {
                SoulCompendiumManager.Instance.UpdateSinglePetUI("Bulbasaur");
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

            //Set default XP bonus
            float xpDefaultxp = 0.025f; //2.5%
            float xpNew = 0f;
            var item = ScriptableObject.CreateInstance<Item>();
            item.name = s.soulName;
            item.itemName = s.soulName;
            item.information = "pet";
            int multiplier = s.rarity switch
            {
                1 => 2, //5%
                2 => 4, //10%
                3 => 6, //15%
                4 => 8, //20%
                5 => 10, //25%
                _ => 1
            };

            xpNew = xpDefaultxp * multiplier;

            switch (s.skillName)
            {
                case "Hitpoints":
                    item.hitPointsBonusXp = xpNew; break;
                case "Attack":
                    item.attackBonusXP = xpNew; break;
                case "Strength":
                    item.strengthBonusXp = xpNew;
                    item.critalChance = multiplier / 2; break;
                case "Defense":
                    item.defenceBonusXP = xpNew; break;
                case "Ranged":
                    item.rangeBonusXP = xpNew; break;
                case "Magic":
                    item.magicBonusXP = xpNew; break;
                case "Devotion":
                    item.devotionBonusXp = xpNew; break;
                case "Beastmastery":
                    item.beastMateryBonusXp = xpNew; break;
                case "Dexterity":
                    item.dexterityBonusXp = xpNew; break;
                case "Foraging":
                    item.foragingBonusXp = xpNew; break;
                case "Herblore":
                    item.herbologyBonusXp = xpNew; break;
                case "Crafting":
                    item.craftingBonusXp = xpNew; break;
                case "Fletching":
                    item.bowCraftingBonusXp = xpNew; break;
                case "Imbuing":
                    item.imbuingBonusXp = xpNew; break;
                case "Thieving":
                    item.thievingBonusXp = xpNew; break;
                case "Soulbinding":
                    item.soulBindingBonusXp = xpNew; break;
                case "Mining":
                    item.miningBonusXp = xpNew; break;
                case "Smithing":
                    item.smithingBonusXp = xpNew; break;
                case "Fishing":
                    item.fishingBonusXp = xpNew; break;
                case "Cooking":
                    item.cookingBonusXp = xpNew; break;
                case "Woodcutting":
                    item.woodcuttingBonusXp = xpNew;
                    item.firemakingBonusXp = xpNew / 2; break;
                case "Firemaking":
                    item.firemakingBonusXp = xpNew;
                    item.woodcuttingBonusXp = xpNew / 2; break;
                case "Tracking":
                    item.trackingBonusXp = xpNew;
                    item.doubleTrackingLoot = multiplier / 2; break;
                case "Homesteading":
                    item.homesteadingBonusXp = xpNew;
                    item.woodcuttingBonusXp = xpNew / 3;
                    item.miningBonusXp = xpNew / 3;
                    item.fishingBonusXp = xpNew / 3;
                    item.foragingBonusXp = xpNew / 3;
                    break;
            }

            Sprite[] animFrames = GetAnimationFrames(s.soulName);
            if (animFrames != null && animFrames.Length > 0)
            {
                item.commonImage = animFrames[0];
            }
            else
            {
                item.commonImage = GetSprite(s.soulName);
            }

            return item;
        }

        //Skill bonus builder
        public static void BuildBonus(string s)
        {
            if (s == null) return;
        }

        //Sprite grabber
        public static Sprite GetSprite(string resourceName)
        {
            if (_spriteCache.ContainsKey(resourceName)) return _spriteCache[resourceName];

            var anim = GetAnimationFrames(resourceName);
            if (anim != null && anim.Length > 0)
            {
                _spriteCache[resourceName] = anim[0];
                return anim[0];
            }

            var assembly = Assembly.GetExecutingAssembly();
            string manifestName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.IndexOf("Sprites", StringComparison.OrdinalIgnoreCase) >= 0
                                    && n.IndexOf(resourceName, StringComparison.OrdinalIgnoreCase) >= 0
                                    && n.EndsWith(".png", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(manifestName))
            {
                using (Stream stream = assembly.GetManifestResourceStream(manifestName))
                {
                    if (stream != null)
                    {
                        byte[] ba = new byte[stream.Length];
                        stream.Read(ba, 0, ba.Length);
                        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        if (ImageConversion.LoadImage(texture, ba))
                        {
                            texture.filterMode = FilterMode.Point;
                            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 50f);
                            _spriteCache[resourceName] = newSprite;
                            return newSprite;
                        }
                    }
                }
            }
            return null;
        }

        public static Sprite[] GetAnimationFrames(string petName)
        {
            var a = Assembly.GetExecutingAssembly();

            string manifestName = a.GetManifestResourceNames().FirstOrDefault(n => n.IndexOf("Sprites", StringComparison.OrdinalIgnoreCase) >= 0 && n.IndexOf(petName, StringComparison.OrdinalIgnoreCase) >= 0 && n.EndsWith(".gif", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(manifestName)) return null;

            List<Sprite> frames = new List<Sprite>();

            using (Stream stream = a.GetManifestResourceStream(manifestName))
            {
                using (System.Drawing.Image gifImage = System.Drawing.Image.FromStream(stream))
                {
                    var d = new System.Drawing.Imaging.FrameDimension(gifImage.FrameDimensionsList[0]);
                    int framecount = gifImage.GetFrameCount(d);

                    for (int i = 0; i < framecount; i++)
                    {
                        gifImage.SelectActiveFrame(d, i);

                        using (Bitmap frameBitmap = new Bitmap(gifImage))
                        {
                            frames.Add(BitmapToSprite(frameBitmap, $"{petName}_{i}"));
                        }
                    }
                }
            }
            return frames.ToArray();
        }

        private static Sprite BitmapToSprite(Bitmap bitmap, string name)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] buffer = ms.ToArray();

                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;

                if (ImageConversion.LoadImage(tex, buffer))
                {
                    Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 50f);
                    s.name = name;
                    return s;
                }
            }
            return null;
        }

        //The animator
        public class DynamicPetAnimator : MonoBehaviour
        {
            public Sprite[] frames;
            public float frameRate = 0.12f; // ~8 FPS, adjust as needed
            private SpriteRenderer sr;
            private int currentFrame;
            private float timer;

            void Awake()
            {
                sr = GetComponent<SpriteRenderer>();
            }

            void Update()
            {
                if (frames == null || frames.Length <= 1 || sr == null) return;

                timer += Time.deltaTime;
                if (timer >= frameRate)
                {
                    timer = 0;
                    currentFrame = (currentFrame + 1) % frames.Length;
                    sr.sprite = frames[currentFrame];
                }
            }
        }

        //The animator helper
        public void AnimatorHelper(PetManager __instance, Item petItem)
        {
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

        //Force save things
        private void SavePokeBloobs()
        {
            var tr = Traverse.Create(PetManager.Instance);
            tr.Method("SavePets").GetValue();
        }

        //The rarity populator
        public static void PopulateRarityArrays()
        {
            if (SoulsDatabase.LoadedSouls == null || SoulsDatabase.LoadedSouls.Count == 0)
            {
                Debug.LogError("[PokeBloobs] Cannot populate arrays: LoadedSouls is empty!");
                return;
            }

            pCommon = SoulsDatabase.LoadedSouls.Where(s => s.rarity == 0).Select(s => s.soulName).ToArray();
            pUncommon = SoulsDatabase.LoadedSouls.Where(s => s.rarity == 1).Select(s => s.soulName).ToArray();
            pRare = SoulsDatabase.LoadedSouls.Where(s => s.rarity == 2).Select(s => s.soulName).ToArray();
            pUltrarare = SoulsDatabase.LoadedSouls.Where(s => s.rarity == 3).Select(s => s.soulName).ToArray();
            pMythic = SoulsDatabase.LoadedSouls.Where(s => s.rarity == 4).Select(s => s.soulName).ToArray();
            pGodTier = SoulsDatabase.LoadedSouls.Where(s => s.rarity == 5).Select(s => s.soulName).ToArray();

            Debug.Log("[PokeBloobs] Rarity arrays successfully populated from JSON.");
        }

        //Skill soul rarity helper
        public static float GetDropChance(int rarity)
        {
            return rarity switch
            {
                0 => 0.01f,         //1% Chance
                1 => 0.005f,        //0.5% Chance
                2 => 0.0001f,       //0.01%
                3 => 0.00001f,      //0.001%
                4 => 0.000001f,     //0.00001%
                5 => 0.00000001f,   //0.0000001%
                _ => 0.01f
            };
        }

        //Evo helper

    }

    [HarmonyPatch(typeof(HitPointsSkill), "DropPets")]
    public class Patch_HitPointsSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(1) && PokeBloobs.patchSkillrun[1])
            {
                return;
            }

            if (_cachedWoodcuttingSouls == null)
            {
                _cachedWoodcuttingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Hitpoints"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedWoodcuttingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedWoodcuttingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Hitpoints soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Hitpoints soul drop patches have been applied");
            PokeBloobs.patchSkillrun[1] = true;
        }
    }

    [HarmonyPatch(typeof(AttackSkill), "DropPets")]
    public class Patch_AttackSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(2) && PokeBloobs.patchSkillrun[2])
            {
                return;
            }

            if (_cachedWoodcuttingSouls == null)
            {
                _cachedWoodcuttingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Attack"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedWoodcuttingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedWoodcuttingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Attack soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Attack soul drop patches have been applied");
            PokeBloobs.patchSkillrun[2] = true;
        }
    }

    [HarmonyPatch(typeof(StrengthSkill), "DropPets")]
    public class Patch_StrengthSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(3) && PokeBloobs.patchSkillrun[3])
            {
                return;
            }

            if (_cachedWoodcuttingSouls == null)
            {
                _cachedWoodcuttingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Strength"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedWoodcuttingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedWoodcuttingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Strength soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Strength soul drop patches have been applied");
            PokeBloobs.patchSkillrun[3] = true;
        }
    }

    [HarmonyPatch(typeof(DefenceSkill), "DropPets")]
    public class Patch_DefenseSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(4) && PokeBloobs.patchSkillrun[4])
            {
                return;
            }

            if (_cachedWoodcuttingSouls == null)
            {
                _cachedWoodcuttingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Defense"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedWoodcuttingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedWoodcuttingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Defense soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Defense soul drop patches have been applied");
            PokeBloobs.patchSkillrun[4] = true;
        }
    }

    [HarmonyPatch(typeof(RangeSkill), "DropPets")]
    public class Patch_RangedSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(5) && PokeBloobs.patchSkillrun[5])
            {
                return;
            }

            if (_cachedWoodcuttingSouls == null)
            {
                _cachedWoodcuttingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Ranged"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedWoodcuttingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedWoodcuttingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Ranged soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Ranged soul drop patches have been applied");
            PokeBloobs.patchSkillrun[5] = true;
        }
    }

    [HarmonyPatch(typeof(MagicSkill), "DropPets")]
    public class Patch_MagicSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(6) && PokeBloobs.patchSkillrun[6])
            {
                return;
            }

            if (_cachedWoodcuttingSouls == null)
            {
                _cachedWoodcuttingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Magic"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedWoodcuttingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedWoodcuttingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Magic soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Magic soul drop patches have been applied");
            PokeBloobs.patchSkillrun[6] = true;
        }
    }

    [HarmonyPatch(typeof(DevotionSkill), "DropPets")]
    public class Patch_DevotionSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(7) && PokeBloobs.patchSkillrun[7])
            {
                return;
            }

            if (_cachedWoodcuttingSouls == null)
            {
                _cachedWoodcuttingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Devotion"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedWoodcuttingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedWoodcuttingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Devotion soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Devotion soul drop patches have been applied");
            PokeBloobs.patchSkillrun[7] = true;
        }
    }

    [HarmonyPatch(typeof(BeastMasterySkill), "DropPets")]
    public class Patch_BeastMasterySkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(8) && PokeBloobs.patchSkillrun[8])
            {
                return;
            }

            if (_cachedWoodcuttingSouls == null)
            {
                _cachedWoodcuttingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("BeastMastery"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedWoodcuttingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedWoodcuttingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"BeastMastery soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"BeastMastery soul drop patches have been applied");
            PokeBloobs.patchSkillrun[8] = true;
        }
    }

    [HarmonyPatch(typeof(WoodcuttingSkill), "DropPets")]
    public class Patch_Woodcuttingskill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(21) && PokeBloobs.patchSkillrun[21])
            {
                return;
            }

            if (_cachedWoodcuttingSouls == null)
            {
                _cachedWoodcuttingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Woodcutting"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance =  PokeBloobs.GetDropChance(soul.rarity);
                    _cachedWoodcuttingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedWoodcuttingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Woodcutting soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Woodcutting soul drop patches have been applied");
            PokeBloobs.patchSkillrun[21] = true;
        }
    }

    //General Patches
    [HarmonyPatch(typeof(Item), "GetAllItems")]
    public static class Patch_ItemRegistry
    {
        static void Postfix(ref List<Item> __result)
        {
            foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls)
            {
                //Check for dupes
                if (!__result.Any(i => i.itemName == soul.soulName))
                {
                    Item i = PokeBloobs.BuildSoul(soul);
                    __result.Add(i);
                }
            }
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
                Debug.Log("Unowned Pet: " + soul.soulName);
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

                    __instance.UpdateSinglePetUI(soul.soulName);

                    Debug.Log($"[PokeBloobs] Successfully injected {soul.soulName} into Compendium.");
                }

                Debug.Log($"[PokeBloobs] Added {soul.soulName} to Soul Compendium.");
            }
        }
    }



    [HarmonyPatch(typeof(SoulCompendiumManager), "GetPetSprite")]
    public static class Patch_CompendiumSpriteFix
    {
        static bool Prefix(string petName, ref Sprite __result)
        {
            var customSoul = PokeBloobs.SoulsDatabase.LoadedSouls
                .FirstOrDefault(s => s.soulName.Equals(petName, StringComparison.OrdinalIgnoreCase));

            if (customSoul != null)
            {
                __result = PokeBloobs.GetSprite(customSoul.soulName);

                return false;
            }
            return true;
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