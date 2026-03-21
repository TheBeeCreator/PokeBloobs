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
        public Dictionary<string, string[]> evolutionChains = new Dictionary<string, string[]>
        {
            { "Bulbasaur", new[] { "Bulbasaur", "Ivysaur", "Venusaur" } },
            { "Charmander", new[] { "Charmander", "Charmeleon", "Charizard" } },
            { "Squirtle", new[] { "Squirtle", "Wartortle", "Blastoise" } },

            { "Caterpie", new[] { "Caterpie", "Metapod", "Butterfree" } },
            { "Weedle", new[] { "Weedle", "Kakuna", "Beedrill" } },
            { "Pidgey", new[] { "Pidgey", "Pidgeotto", "Pidgeot" } },

            { "Rattata", new[] { "Rattata", "Raticate" } },
            { "Spearow", new[] { "Spearow", "Fearow" } },
            { "Ekans", new[] { "Ekans", "Arbok" } },
            { "Pikachu", new[] { "Pikachu", "Raichu" } },
            { "Sandshrew", new[] { "Sandshrew", "Sandslash" } },

            { "NidoranF", new[] { "NidoranF", "Nidorina", "Nidoqueen" } },
            { "NidoranM", new[] { "NidoranM", "Nidorino", "Nidoking" } },

            { "Clefairy", new[] { "Clefairy", "Clefable" } },
            { "Vulpix", new[] { "Vulpix", "Ninetales" } },
            { "Jigglypuff", new[] { "Jigglypuff", "Wigglytuff" } },
            { "Zubat", new[] { "Zubat", "Golbat" } },

            { "Oddish", new[] { "Oddish", "Gloom", "Vileplume" } },
            { "Paras", new[] { "Paras", "Parasect" } },
            { "Venonat", new[] { "Venonat", "Venomoth" } },
            { "Diglett", new[] { "Diglett", "Dugtrio" } },
            { "Meowth", new[] { "Meowth", "Persian" } },
            { "Psyduck", new[] { "Psyduck", "Golduck" } },
            { "Mankey", new[] { "Mankey", "Primeape" } },
            { "Growlithe", new[] { "Growlithe", "Arcanine" } },

            { "Poliwag", new[] { "Poliwag", "Poliwhirl", "Poliwrath" } },
            { "Abra", new[] { "Abra", "Kadabra", "Alakazam" } },
            { "Machop", new[] { "Machop", "Machoke", "Machamp" } },
            { "Bellsprout", new[] { "Bellsprout", "Weepinbell", "Victreebel" } },

            { "Tentacool", new[] { "Tentacool", "Tentacruel" } },
            { "Geodude", new[] { "Geodude", "Graveler", "Golem" } },
            { "Ponyta", new[] { "Ponyta", "Rapidash" } },
            { "Slowpoke", new[] { "Slowpoke", "Slowbro" } },
            { "Magnemite", new[] { "Magnemite", "Magneton" } },

            { "Farfetchd", new[] { "Farfetchd" } },

            { "Doduo", new[] { "Doduo", "Dodrio" } },
            { "Seel", new[] { "Seel", "Dewgong" } },
            { "Grimer", new[] { "Grimer", "Muk" } },
            { "Shellder", new[] { "Shellder", "Cloyster" } },

            { "Gastly", new[] { "Gastly", "Haunter", "Gengar" } },

            { "Onix", new[] { "Onix" } },

            { "Drowzee", new[] { "Drowzee", "Hypno" } },
            { "Krabby", new[] { "Krabby", "Kingler" } },
            { "Voltorb", new[] { "Voltorb", "Electrode" } },
            { "Exeggcute", new[] { "Exeggcute", "Exeggutor" } },
            { "Cubone", new[] { "Cubone", "Marowak" } },

            { "Hitmonlee", new[] { "Hitmonlee" } },
            { "Hitmonchan", new[] { "Hitmonchan" } },
            { "Lickitung", new[] { "Lickitung" } },

            { "Koffing", new[] { "Koffing", "Weezing" } },
            { "Rhyhorn", new[] { "Rhyhorn", "Rhydon" } },

            { "Chansey", new[] { "Chansey" } },
            { "Tangela", new[] { "Tangela" } },
            { "Kangaskhan", new[] { "Kangaskhan" } },

            { "Horsea", new[] { "Horsea", "Seadra" } },
            { "Goldeen", new[] { "Goldeen", "Seaking" } },
            { "Staryu", new[] { "Staryu", "Starmie" } },

            { "MrMime", new[] { "MrMime" } },
            { "Scyther", new[] { "Scyther" } },
            { "Jynx", new[] { "Jynx" } },
            { "Electabuzz", new[] { "Electabuzz" } },
            { "Magmar", new[] { "Magmar" } },
            { "Pinsir", new[] { "Pinsir" } },
            { "Tauros", new[] { "Tauros" } },

            { "Magikarp", new[] { "Magikarp", "Gyarados" } },
            { "Lapras", new[] { "Lapras" } },
            { "Ditto", new[] { "Ditto" } },

            { "Eevee", new[] { "Eevee", "Vaporeon", "Jolteon", "Flareon" } },

            { "Porygon", new[] { "Porygon" } },

            { "Omanyte", new[] { "Omanyte", "Omastar" } },
            { "Kabuto", new[] { "Kabuto", "Kabutops" } },

            { "Aerodactyl", new[] { "Aerodactyl" } },
            { "Snorlax", new[] { "Snorlax" } },

            { "Articuno", new[] { "Articuno" } },
            { "Zapdos", new[] { "Zapdos" } },
            { "Moltres", new[] { "Moltres" } },

            { "Dratini", new[] { "Dratini", "Dragonair", "Dragonite" } },

            { "Mewtwo", new[] { "Mewtwo" } },
            { "Mew", new[] { "Mew" } }
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
        public static List<Item> _cachedHitpointsSouls;
        public static List<Item> _cachedAttackSouls;
        public static List<Item> _cachedStrengthSouls;
        public static List<Item> _cachedDefenseSouls;
        public static List<Item> _cachedRangedSouls;
        public static List<Item> _cachedMagicSouls;
        public static List<Item> _cachedDevotionSouls;
        public static List<Item> _cachedBeastmasterSouls;

        public static List<Item> _cachedDexteritySouls;
        public static List<Item> _cachedForagingSouls;
        public static List<Item> _cachedHerbloreSouls;
        public static List<Item> _cachedCraftingSouls;
        public static List<Item> _cachedBowCraftingSouls;
        public static List<Item> _cachedImbuingSouls;
        public static List<Item> _cachedThievingSouls;
        public static List<Item> _cachedSoulBindingSouls;

        public static List<Item> _cachedMiningSouls;
        public static List<Item> _cachedSmithingSouls;
        public static List<Item> _cachedFishingsouls;
        public static List<Item> _cachedCookingSouls;
        public static List<Item> _cachedWoodcuttingSouls;
        public static List<Item> _cachedFiremakingSouls;
        public static List<Item> _cachedTrackingSouls;
        public static List<Item> _cachedHomesteadingSouls;

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

            Harmony.CreateAndPatchAll(typeof(Patch_DexteritySkill));
            Harmony.CreateAndPatchAll(typeof(Patch_ForagingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_HerbloreSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_CraftingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_BowcraftingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_ImbuingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_ThievingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_SoulbindingSkill));

            Harmony.CreateAndPatchAll(typeof(Patch_MiningSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_SmithingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_FishingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_CookingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_WoodcuttingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_FiremakingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_TrackingSkill));
            Harmony.CreateAndPatchAll(typeof(Patch_HomesteadingSkill));

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

            float multiplierSB = PlayerDataManager.Instance.GetSoulBindingPrestigeLevel();
            switch (multiplierSB)
            { 
                case 0:
                    break;
                case 1:
                    xpNew = xpNew * 1.3f; break;
                case 2:
                    xpNew = xpNew * 1.5f; break;
                case 3:
                    xpNew = xpNew * 1.7f; break;
                case 4:
                    xpNew = xpNew * 1.9f; break;
                case 5:
                    xpNew = xpNew * 2.0f; break;
                case 6:
                    xpNew = xpNew * 3.0f; break;
                case 7:
                    xpNew = xpNew * 4.0f; break;
                case 8:
                    xpNew = xpNew * 5.0f; break;
                case 9:
                    xpNew = xpNew * 6.0f; break;
                case 10:
                    xpNew = xpNew * 7.0f; break;
            }

            //Enforce a cap on possible bonus
            xpNew = Math.Min(xpNew, 35);

            switch (s.skillName)
            {
                case "Hitpoints":
                    item.hitPointsBonusXp = xpNew; break;
                case "Attack":
                    item.attackBonusXP = xpNew;
                    item.accuracy = multiplier / 2; break; 
                case "Strength":
                    item.strengthBonusXp = xpNew;
                    item.critalChance = multiplier / 2; break;
                case "Defense":
                    item.defenceBonusXP = xpNew; break;
                case "Ranged":
                    item.rangeBonusXP = xpNew;
                    item.critalChance = multiplier / 2; break;
                case "Magic":
                    item.magicBonusXP = xpNew;
                    item.critalChance = multiplier / 2; break;
                case "Devotion":
                    item.devotionBonusXp = xpNew;
                    item.beastMateryBonusXp = xpNew / 2; break;
                case "Beastmastery":
                    item.beastMateryBonusXp = xpNew;
                    item.attackBonusXP = xpNew / 25;
                    item.defenceBonusXP = xpNew / 25;
                    item.strengthBonusXp = xpNew / 25;
                    item.rangeBonusXP = xpNew / 25;
                    item.magicBonusXP = xpNew / 25; break;
                case "Dexterity":
                    item.dexterityBonusXp = xpNew;
                    item.thievingBonusXp = xpNew / 3; break;
                case "Foraging":
                    item.foragingBonusXp = xpNew; break;
                case "Herblore":
                    item.herbologyBonusXp = xpNew; break;
                case "Crafting":
                    item.craftingBonusXp = xpNew;
                    item.bowCraftingBonusXp = xpNew; break;
                case "Bowcrafting":
                    item.bowCraftingBonusXp = xpNew;
                    item.craftingBonusXp = xpNew; break;
                case "Imbuing":
                    item.imbuingBonusXp = xpNew;
                    item.magicBonusXP = xpNew / 2; break;
                case "Thieving":
                    item.thievingBonusXp = xpNew;
                    item.dexterityBonusXp = xpNew / 2; break;
                case "Soulbinding":
                    item.soulBindingBonusXp = xpNew * 2; break;
                case "Mining":
                    item.miningBonusXp = xpNew;
                    item.smithingBonusXp = xpNew; break;
                case "Smithing":
                    item.smithingBonusXp = xpNew;
                    item.miningBonusXp = xpNew; break;
                case "Fishing":
                    item.fishingBonusXp = xpNew;
                    item.cookingBonusXp = xpNew; break;
                case "Cooking":
                    item.cookingBonusXp = xpNew;
                    item.fishingBonusXp = xpNew; break;
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
                0 => 0.001f,        //0.01% Chance
                1 => 0.0005f,       //0.005% Chance
                2 => 0.00001f,      //0.0001%
                3 => 0.000001f,     //0.00001%
                4 => 0.0000001f,    //0.0000001%
                5 => 0.00000001f,   //0.00000001%
                _ => 0.01f
            };
        }

        //Evo helper
        public List<string> GetRequiredPreEvolutions(string petName)
        {
            List<string> requirements = new List<string>();

            foreach (var entry in evolutionChains)
            {
                string[] chain = entry.Value;
                int targetIndex = Array.IndexOf(chain, petName);

                if (targetIndex > 0)
                {
                    for (int i = 0; i < targetIndex; i++)
                    {
                        requirements.Add(chain[i]);
                    }
                    return requirements;
                }
            }
            return requirements;
        }
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

            if (_cachedHitpointsSouls == null)
            {
                _cachedHitpointsSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Hitpoints"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedHitpointsSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedHitpointsSouls)
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

            if (_cachedStrengthSouls == null)
            {
                _cachedStrengthSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Strength"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedStrengthSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedStrengthSouls)
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

            if (_cachedDefenseSouls == null)
            {
                _cachedDefenseSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Defense"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedDefenseSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedDefenseSouls)
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

            if (_cachedRangedSouls == null)
            {
                _cachedRangedSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Ranged"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedRangedSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedRangedSouls)
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

            if (_cachedMagicSouls == null)
            {
                _cachedMagicSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Magic"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedMagicSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedMagicSouls)
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

            if (_cachedDevotionSouls == null)
            {
                _cachedDevotionSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Devotion"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedDevotionSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedDevotionSouls)
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

            if (_cachedBeastmasterSouls == null)
            {
                _cachedBeastmasterSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Beastmastery"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedBeastmasterSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedBeastmasterSouls)
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

    [HarmonyPatch(typeof(DexteritySkill), "DropPets")]
    public class Patch_DexteritySkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(9) && PokeBloobs.patchSkillrun[9])
            {
                return;
            }

            if (_cachedDexteritySouls == null)
            {
                _cachedDexteritySouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Dexterity"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedDexteritySouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedDexteritySouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Dexterity soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Dexterity soul drop patches have been applied");
            PokeBloobs.patchSkillrun[9] = true;
        }
    }

    [HarmonyPatch(typeof(ForagingSkill), "DropPets")]
    public class Patch_ForagingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(10) && PokeBloobs.patchSkillrun[10])
            {
                return;
            }

            if (_cachedForagingSouls == null)
            {
                _cachedForagingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Foraging"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedForagingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedForagingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Foraging soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Foraging soul drop patches have been applied");
            PokeBloobs.patchSkillrun[10] = true;
        }
    }

    [HarmonyPatch(typeof(HerbologySkill), "DropPets")]
    public class Patch_HerbloreSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(11) && PokeBloobs.patchSkillrun[11])
            {
                return;
            }

            if (_cachedHerbloreSouls == null)
            {
                _cachedHerbloreSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Herblore"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedHerbloreSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedHerbloreSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Herblore soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Herblore soul drop patches have been applied");
            PokeBloobs.patchSkillrun[11] = true;
        }
    }

    [HarmonyPatch(typeof(CraftingSkill), "DropPets")]
    public class Patch_CraftingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(12) && PokeBloobs.patchSkillrun[12])
            {
                return;
            }

            if (_cachedCraftingSouls == null)
            {
                _cachedCraftingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Crafting"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedCraftingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedCraftingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Crafting soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Crafting soul drop patches have been applied");
            PokeBloobs.patchSkillrun[12] = true;
        }
    }

    [HarmonyPatch(typeof(BowCraftingSkill), "DropPets")]
    public class Patch_BowcraftingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(13) && PokeBloobs.patchSkillrun[13])
            {
                return;
            }

            if (_cachedBowCraftingSouls == null)
            {
                _cachedBowCraftingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Bowcrafting"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedBowCraftingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedBowCraftingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Bowcrafting soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Bowcrafting soul drop patches have been applied");
            PokeBloobs.patchSkillrun[13] = true;
        }
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
                _cachedImbuingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Imbuing"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedImbuingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedImbuingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Imbuing soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Imbuing soul drop patches have been applied");
            PokeBloobs.patchSkillrun[14] = true;
        }
    }

    [HarmonyPatch(typeof(ThievingSkill), "DropPets")]
    public class Patch_ThievingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(15) && PokeBloobs.patchSkillrun[15])
            {
                return;
            }

            if (_cachedThievingSouls == null)
            {
                _cachedThievingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Thieving"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedThievingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedThievingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Thieving soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Thieving soul drop patches have been applied");
            PokeBloobs.patchSkillrun[15] = true;
        }
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

            if (_cachedSoulBindingSouls == null)
            {
                _cachedSoulBindingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Soulbinding"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedSoulBindingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedSoulBindingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Soulbinding soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Soulbinding soul drop patches have been applied");
            PokeBloobs.patchSkillrun[16] = true;
        }
    }

    [HarmonyPatch(typeof(MiningSkill), "DropPets")]
    public class Patch_MiningSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(17) && PokeBloobs.patchSkillrun[17])
            {
                return;
            }

            if (_cachedMiningSouls == null)
            {
                _cachedMiningSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Mining"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedMiningSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedMiningSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Mining soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Mining soul drop patches have been applied");
            PokeBloobs.patchSkillrun[17] = true;
        }
    }

    [HarmonyPatch(typeof(SmithingSkill), "DropPets")]
    public class Patch_SmithingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(18) && PokeBloobs.patchSkillrun[18])
            {
                return;
            }

            if (_cachedSmithingSouls == null)
            {
                _cachedSmithingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Smithing"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedSmithingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedSmithingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Smithing soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Smithing soul drop patches have been applied");
            PokeBloobs.patchSkillrun[18] = true;
        }
    }

    [HarmonyPatch(typeof(FishingSkill), "DropPets")]
    public class Patch_FishingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(19) && PokeBloobs.patchSkillrun[19])
            {
                return;
            }

            if (_cachedFishingsouls == null)
            {
                _cachedFishingsouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Fishing"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedFishingsouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedFishingsouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Fishing soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Fishing soul drop patches have been applied");
            PokeBloobs.patchSkillrun[19] = true;
        }
    }

    [HarmonyPatch(typeof(CookingSkill), "DropPets")]
    public class Patch_CookingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(20) && PokeBloobs.patchSkillrun[20])
            {
                return;
            }

            if (_cachedCookingSouls == null)
            {
                _cachedCookingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Cooking"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedCookingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedCookingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Cooking soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Cooking soul drop patches have been applied");
            PokeBloobs.patchSkillrun[20] = true;
        }
    }

    [HarmonyPatch(typeof(WoodcuttingSkill), "DropPets")]
    public class Patch_WoodcuttingSkill
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
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
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

    [HarmonyPatch(typeof(FiremakingSkill), "DropPets")]
    public class Patch_FiremakingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(22) && PokeBloobs.patchSkillrun[22])
            {
                return;
            }

            if (_cachedFiremakingSouls == null)
            {
                _cachedFiremakingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Firemaking"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedFiremakingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedFiremakingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Firemaking soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Firemaking soul drop patches have been applied");
            PokeBloobs.patchSkillrun[22] = true;
        }
    }

    [HarmonyPatch(typeof(TrackingSkill), "DropPets")]
    public class Patch_TrackingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(23) && PokeBloobs.patchSkillrun[23])
            {
                return;
            }

            if (_cachedTrackingSouls == null)
            {
                _cachedTrackingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Tracking"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedTrackingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedTrackingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Tracking soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Tracking soul drop patches have been applied");
            PokeBloobs.patchSkillrun[23] = true;
        }
    }

    [HarmonyPatch(typeof(HomeSteadingSkill), "DropPets")]
    public class Patch_HomesteadingSkill
    {
        static void Postfix(ref List<Item> ___petDrops)
        {
            if (PokeBloobs.patchSkillrun.ContainsKey(24) && PokeBloobs.patchSkillrun[24])
            {
                return;
            }

            if (_cachedHomesteadingSouls == null)
            {
                _cachedHomesteadingSouls = new List<Item>();
                var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
                    .Where(n => n.skillName.Contains("Homesteading"));

                foreach (var soul in wcSouls)
                {
                    Item c = PokeBloobs.BuildSoul(soul);
                    c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
                    _cachedHomesteadingSouls.Add(c);
                }
            }

            var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

            foreach (var soulItem in _cachedHomesteadingSouls)
            {
                if (!existingNames.Contains(soulItem.name))
                {
                    Debug.Log($"Homesteading soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
                    ___petDrops.Add(soulItem);
                }
            }

            Debug.Log($"Homesteading soul drop patches have been applied");
            PokeBloobs.patchSkillrun[24] = true;
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

    [HarmonyPatch(typeof(PetManager), "AddPet")]
    public static class Patch_PetManagerAddPet
    {
        static bool Prefix(PetManager __instance, Item petItem)
        {
            PokeBloobs plugin = GameObject.FindObjectOfType<PokeBloobs>();
            if (plugin == null) return true;

            // Get the list of all pets required before this one
            string petName = petItem.itemName;
            Debug.Log($"[PokeBloobs] Attempting to unlock {petName}");
            List<string> requirements = plugin.GetRequiredPreEvolutions(petName);

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
}