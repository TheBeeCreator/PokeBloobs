using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
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
        
        /* OLD
        //Caches
        //public static List<Item> _cachedHitpointsSouls;
        //public static List<Item> _cachedAttackSouls;
        */

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

            var dispatcherObj = new GameObject("PokeBloobs_Dispatcher");
                dispatcherObj.AddComponent<TaskDispatcher>();

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

            var harmony = new Harmony("com.SKOM.PokeBloobs");

            harmony.PatchAll();
            //OLD CODE
            //Harmony.CreateAndPatchAll(typeof(Patch_ItemRegistry));
            //Harmony.CreateAndPatchAll(typeof(Patch_AddCustomPet));

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


                //SoulCompendiumManager.Instance.UpdateSinglePetUI("Bulbasaur");
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
                2 => 3, //7.5%
                3 => 4, //10%
                4 => 4, //10%
                5 => 10, //25%
                _ => 1
            };

            xpNew = xpDefaultxp * multiplier;

            float multiplierSB = PlayerDataManager.Instance.GetSoulBindingPrestigeLevel();
            //Debug.LogError($"[PokeBloobs] {multiplierSB}!");
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
                case >= 5:
                    xpNew = xpNew * 2.0f; break;
            }

            //Enforce a cap on possible bonus
            xpNew = Mathf.Clamp(xpNew, 0.025f, 0.10f);
            //xpNew = Math.Min(xpNew, 10);

            switch (s.skillName)
            {
                case "Hitpoints":
                    item.hitPointsBonusXp = xpNew;
                    item.defenceBonusXP = xpNew / 2; break;
                case "Attack":
                    item.attackBonusXP = xpNew;
                    item.accuracy = xpNew / 5;
                    item.critalChance = xpNew / 5; break;
                case "Strength":
                    item.strengthBonusXp = xpNew;
                    item.meleeSoulDamage = xpNew / 2;
                    item.accuracy = xpNew / 5; break;
                case "Defense":
                    item.defenceBonusXP = xpNew;
                    item.hitPointsBonusXp = xpNew / 2; break;
                case "Ranged":
                    item.rangeBonusXP = xpNew;
                    item.rangedSoulDamage = xpNew / 2;
                    item.rangeAccuracy = xpNew / 5; break;
                case "Magic":
                    item.magicBonusXP = xpNew;
                    item.magicSoulDamage = xpNew / 2;
                    item.magicAccuracy = xpNew / 5; break;
                case "Devotion":
                    item.devotionBonusXp = xpNew;
                    item.beastMateryBonusXp = xpNew / 2; break;
                case "Beastmastery":
                    item.beastMateryBonusXp = xpNew;
                    item.attackBonusXP = xpNew / 5;
                    item.defenceBonusXP = xpNew / 5;
                    item.strengthBonusXp = xpNew / 5;
                    item.rangeBonusXP = xpNew / 5;
                    item.magicBonusXP = xpNew / 5; break;
                case "Dexterity":
                    item.dexterityBonusXp = xpNew;
                    item.thievingBonusXp = xpNew / 3; break;
                case "Foraging":
                    item.foragingBonusXp = xpNew;
                    item.herbologyBonusXp = xpNew / 3; break;
                case "Herblore":
                    item.herbologyBonusXp = xpNew;
                    item.foragingBonusXp = xpNew / 2; break;
                case "Crafting":
                    item.craftingBonusXp = xpNew;
                    item.bowCraftingBonusXp = xpNew; break;
                case "Bowcrafting":
                    item.bowCraftingBonusXp = xpNew;
                    item.craftingBonusXp = xpNew; break;
                case "Imbuing":
                    item.imbuingBonusXp = xpNew;
                    item.magicBonusXP = xpNew / 5; break;
                case "Thieving":
                    item.thievingBonusXp = xpNew;
                    item.dexterityBonusXp = xpNew / 5; break;
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
                    item.cookingBonusXp = xpNew / 5; break;
                case "Cooking":
                    item.cookingBonusXp = xpNew;
                    item.fishingBonusXp = xpNew / 5; break;
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

            //item.information = "";

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
                0 => 1f / 50000f,       // 1 in 50k
                1 => 1f / 75000f,       // 1 in 75k
                2 => 1f / 125000f,      // 1 in 125k
                3 => 1f / 250000f,      // 1 in 250k
                4 => 1f / 500000f,      // 1 in 500k
                5 => 1f / 100000000f,   // 1 in 100M
                _ => 1f / 50000f        // 1 in 50k
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

        //Call update single
        public static void UpdateSinglePet(string soulName)
        {
            //SavePokeBloobs();
            if (SoulCompendiumManager.Instance != null && SoulCompendiumManager.Instance.IsInitialized)
            {
                SoulCompendiumManager.Instance.UpdateSinglePetUI(soulName);
            }
        }
    }

    //Old Code for reference
    //[HarmonyPatch(typeof(HitPointsSkill), "DropPets")]
    //public class Patch_HitPointsSkill
    //{
    //    static void Postfix(ref List<Item> ___petDrops)
    //    {
    //        if (PokeBloobs.patchSkillrun.ContainsKey(1) && PokeBloobs.patchSkillrun[1])
    //        {
    //            return;
    //        }

    //        if (_cachedHitpointsSouls == null)
    //        {
    //            _cachedHitpointsSouls = new List<Item>();
    //            var wcSouls = PokeBloobs.SoulsDatabase.LoadedSouls
    //                .Where(n => n.skillName.Contains("Hitpoints"));

    //            foreach (var soul in wcSouls)
    //            {
    //                Item c = PokeBloobs.BuildSoul(soul);
    //                c.dropChance = PokeBloobs.GetDropChance(soul.rarity);
    //                _cachedHitpointsSouls.Add(c);
    //            }
    //        }

    //        var existingNames = new HashSet<string>(___petDrops.Select(d => d.name));

    //        foreach (var soulItem in _cachedHitpointsSouls)
    //        {
    //            if (!existingNames.Contains(soulItem.name))
    //            {
    //                Debug.Log($"Hitpoints soul drop {soulItem.name} with rarity: {soulItem.dropChance}");
    //                PokeBloobs.UpdateSinglePet(soulItem.name);
    //                ___petDrops.Add(soulItem);
    //            }
    //        }

            

    //        Debug.Log($"Hitpoints soul drop patches have been applied");
    //        PokeBloobs.patchSkillrun[1] = true;
    //    }
    //}
    //END OLD CODE FOR REFERENCE
}