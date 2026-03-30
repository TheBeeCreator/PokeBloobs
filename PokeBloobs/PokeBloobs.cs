using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using PokeBloobs.Classes;
using PokeBloobs.Patches;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PokeBloobs.Classes.MenuModVersion;
using static PokeBloobs.PokeBloobs;
using ScriptableObject = UnityEngine.ScriptableObject;

namespace PokeBloobs
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class PokeBloobs : BaseUnityPlugin
    {
        //Save keys and the bits
        public enum ModVersionMode
        {
            Cosmetic,
            Normal,
            Chaotic
        }

        public static class ModSettings
        {
            public const string VersionMode = "PokeBloobs_VersionMode";
            public const string StarterChosen = "PokeBloobs_StarterChosen";

            public static bool HasChosenVersion;
            public static bool HasChosenStarter;

            public static ModVersionMode SelectedVersion = ModVersionMode.Cosmetic;
        }

        //The other stuff
        public static readonly Dictionary<ulong, string> special = new Dictionary<ulong, string>
        {
            { 76561198274546625, "Bloobs" },
            { 76561198062315301, "SKOM" }
        };

        public static bool spet = false;
        public static bool OpenedFromF9 = false;

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
            public string PrimaryType;
        }

        public class PetDefinition
        {
            public string name;
            public List<string> SteamIDs;
        }

        public class PlayerData
        {
            public string SteamID = "";
        }

        public static Item soulinfo;

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
        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite[]> _animationCache = new Dictionary<string, Sprite[]>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> _firstFrameCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

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
            LogStartup();

            //Check for our resources
            if (!EnsureAssetZipReady())
                return;

            // Load our saved info later, after the game is ready
            ModSettings.HasChosenVersion = false;
            ModSettings.HasChosenStarter = false;
            ModSettings.SelectedVersion = ModVersionMode.Cosmetic;
            MenuModVersion.VersionSelectUI.ShowPrompt = false;
            PetManagerP.showPrompt = false;

            //Creat the dispatcher
            CreateDispatcher();
            
            //Log embeds
            //LogEmbeddedResources();
            
            //Load our souls
            SoulLoader.Load();
            
            //Patch it
            ApplyHarmonyPatches();
            
            LogPluginLoaded();
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.F9))
            //{
            //    if (GameObject.Find("VersionSelectUI") == null)
            //        new GameObject("VersionSelectUI").AddComponent<VersionSelectUI>();

            //    OpenedFromF9 = true;
            //    VersionSelectUI.ShowPrompt = !VersionSelectUI.ShowPrompt;
            //}
        }

        //Awake helpers
        private void LogStartup()
        {
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} {MyPluginInfo.PLUGIN_VERSION} is installed and starting");
        }

        private bool EnsureAssetZipReady()
        {
            if (!ZipAssetLoader.ZipExists())
            {
                Debug.LogError("[PokeBloobs] Missing PokeBloobs.assets.zip next to the mod DLL.");
                return false ;
            }

            global::PokeBloobs.Classes.ZipAssetLoader.BuildIndex();
            Debug.Log("[PokeBloobs] Asset zip found.");
            return true ;
        }

        private void CreateDispatcher()
        {
            var dispatcherObj = new GameObject("PokeBloobs_Dispatcher");
            dispatcherObj.AddComponent<TaskDispatcher>();
        }

        private void LogEmbeddedResources()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                Logger.LogInfo("Found resources: " + resourceName);
            }
        }

        private void ApplyHarmonyPatches()
        {
            var harmony = new Harmony("com.SKOM.PokeBloobs");
            harmony.PatchAll();

            foreach (var method in Harmony.GetAllPatchedMethods())
            {
                Logger.LogInfo("Patched method: " + method.Module + " : " + method.Name);
            }
        }

        private void LogPluginLoaded()
        {
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        //Embedded Resources
        public string GetJsonFromResources(string resourceName)
        {
            using (Stream stream = global::PokeBloobs.Classes.ZipAssetLoader.OpenAssetStream("Data", resourceName, "json"))
            {
                if (stream == null) return null;

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

       

        //Sprite grabber
        public static Sprite GetSprite(string resourceName)
        {
            if (_spriteCache.TryGetValue(resourceName, out Sprite cached))
                return cached;

            string[] candidateFolders = { "Sprites", "Gen1", "Gen2" };

            foreach (string folder in candidateFolders)
            {
                string assetPath = ZipAssetLoader.FindAssetPath(folder, resourceName, "png");
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                byte[] bytes = ZipAssetLoader.LoadAssetBytes(assetPath);
                if (bytes == null)
                    continue;

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;

                if (!ImageConversion.LoadImage(texture, bytes))
                {
                    UnityEngine.Object.Destroy(texture);
                    continue;
                }

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    50f
                );

                sprite.name = resourceName;
                _spriteCache[resourceName] = sprite;
                return sprite;
            }

            Debug.LogWarning($"[PokeBloobs] Sprite not found in zip: {resourceName}");
            return null;
        }

        //First Framer
        public static Sprite GetFirstAnimationFrame(string petName)
        {
            if (_firstFrameCache.TryGetValue(petName, out var cached))
                return cached;

            string[] folders = { "Gen1", "Gen2" };

            foreach (string folder in folders)
            {
                using (Stream stream = global::PokeBloobs.Classes.ZipAssetLoader.OpenAssetStream(folder, petName, "gif"))
                {
                    if (stream == null)
                        continue;

                    using (System.Drawing.Image gifImage = System.Drawing.Image.FromStream(stream))
                    {
                        var d = new System.Drawing.Imaging.FrameDimension(gifImage.FrameDimensionsList[0]);
                        gifImage.SelectActiveFrame(d, 0);

                        using (Bitmap frameBitmap = new Bitmap(gifImage))
                        {
                            Sprite sprite = BitmapToSprite(frameBitmap, $"{petName}_0");
                            if (sprite != null)
                            {
                                _firstFrameCache[petName] = sprite;
                                Debug.Log($"[PokeBloobs] Loaded first frame only for {petName} from {folder}");
                                return sprite;
                            }
                        }
                    }
                }
            }

            return null;
        }

        //The animation frame getter
        public static Sprite[] GetAnimationFrames(string petName)
        {
            Sprite[] frames = GetAnimationFramesFromFolder("Gen1", petName);
            if (frames != null && frames.Length > 0)
            {
                Debug.Log($"[PokeBloobs] Loaded {frames.Length} Gen1 frames for {petName}");
                return frames;
            }

            frames = GetAnimationFramesFromFolder("Gen2", petName);
            if (frames != null && frames.Length > 0)
            {
                Debug.Log($"[PokeBloobs] Loaded {frames.Length} Gen2 frames for {petName}");
                return frames;
            }

            Debug.LogWarning($"[PokeBloobs] No pet animation frames found for {petName}");
            return null;
        }

        //The new improved animation frame getter
        public static Sprite[] GetAnimationFramesFromFolder(string folderHint, string animationName)
        {
            string cacheKey = folderHint + "/" + animationName;

            if (_animationCache.TryGetValue(cacheKey, out var cached))
                return cached;

            string assetPath = ZipAssetLoader.FindAssetPath(folderHint, animationName, "gif");

            if (string.IsNullOrEmpty(assetPath))
                return null;

            byte[] bytes = ZipAssetLoader.LoadAssetBytes(assetPath);
            if (bytes == null)
                return null;

            List<Sprite> frames = new List<Sprite>();

            using (MemoryStream stream = new MemoryStream(bytes))
            using (System.Drawing.Image gifImage = System.Drawing.Image.FromStream(stream))
            {
                var dimension = new System.Drawing.Imaging.FrameDimension(gifImage.FrameDimensionsList[0]);
                int frameCount = gifImage.GetFrameCount(dimension);

                for (int i = 0; i < frameCount; i++)
                {
                    gifImage.SelectActiveFrame(dimension, i);

                    using (Bitmap frameBitmap = new Bitmap(gifImage))
                    {
                        frames.Add(BitmapToSprite(frameBitmap, $"{animationName}_{i}"));
                    }
                }
            }

            Sprite[] result = frames.ToArray();
            _animationCache[cacheKey] = result;
            return result;
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
                    Sprite s = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f),
                        50f
                    );
                    s.name = name;
                    return s;
                }

                UnityEngine.Object.Destroy(tex);
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
                    //Debug.Log($"[PokeBloobs] petObj is null");
                    petObj = GameObject.Find(petItem.itemName) ?? GameObject.Find(petItem.itemName + "(Clone)");
                }

                if (petObj != null)
                {
                    //Debug.Log($"[PokeBloobs] petObj loaded");
                    var anim = petObj.GetComponent<PokeBloobs.DynamicPetAnimator>() ?? petObj.AddComponent<PokeBloobs.DynamicPetAnimator>();
                    anim.frames = frames;

                    var unityAnimator = petObj.GetComponent<Animator>();
                    if (unityAnimator != null) unityAnimator.enabled = false;
                }
            }
        }

        //Projectile Helper
        public class RuntimePetProjectile : MonoBehaviour
        {
            public BasicEnemy target;
            public float speed = 8f;
            public float hitDistance = 0.2f;
            public float lifeTime = 3f;
            public float damage = 3f;
            public float accuracy = .5f;

            private float timer;
            public bool willHit = true;

            public void Initialize(BasicEnemy enemy, float moveSpeed, float damage, float accuracy)
            {
                target = enemy;
                speed = moveSpeed;
                this.damage = damage;
                this.accuracy = Mathf.Clamp01(accuracy);
                willHit = UnityEngine.Random.value < this.accuracy;

                Vector3 pos = transform.position;
                pos.z = 0f;
                transform.position = pos;
            }

            void Update()
            {
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
                    if (!IsTargetInvalid(target) && damage > 0f)
                    {
                        float min = Mathf.Floor(damage * 0.85f);
                        float max = Mathf.Ceil(damage * 1.15f);
                        float finalDamage = UnityEngine.Random.Range(min, max + 1f);

                        Debug.Log($"Damage {finalDamage}");

                        if (willHit)
                        { target.TakePureDamage(finalDamage, UnityEngine.Color.cyan); } 
                        else
                        { target.TakeDamage(0, UnityEngine.Color.white); }
                    }

                    Destroy(gameObject);
                }
            }

            private bool IsTargetInvalid(BasicEnemy enemy)
            {
                return enemy == null || enemy.GetCurrentHealth() <= 0f || enemy.IsRespawning;
            }
        }

        //The SpriteMan
        public class DynamicSpriteAnimator : MonoBehaviour
        {
            public Sprite[] frames;
            public float frameRate = 0.12f;
            public bool loop = true;
            public bool destroyOnFinish = false;

            private SpriteRenderer sr;
            private int currentFrame;
            private float timer;
            private bool initialized;

            void Awake()
            {
                sr = GetComponent<SpriteRenderer>();
            }

            public void Initialize(Sprite[] newFrames, float newFrameRate = 0.12f, bool shouldLoop = true, bool shouldDestroyOnFinish = false)
            {
                frames = newFrames;
                frameRate = newFrameRate;
                loop = shouldLoop;
                destroyOnFinish = shouldDestroyOnFinish;
                currentFrame = 0;
                timer = 0f;
                initialized = true;

                if (sr == null)
                    sr = GetComponent<SpriteRenderer>();

                if (sr != null && frames != null && frames.Length > 0)
                    sr.sprite = frames[0];
            }

            void Update()
            {
                if (!initialized || frames == null || frames.Length <= 1 || sr == null)
                    return;

                timer += Time.deltaTime;
                if (timer < frameRate)
                    return;

                timer = 0f;
                currentFrame++;

                if (currentFrame >= frames.Length)
                {
                    if (loop)
                    {
                        currentFrame = 0;
                    }
                    else
                    {
                        currentFrame = frames.Length - 1;
                        sr.sprite = frames[currentFrame];

                        if (destroyOnFinish)
                            Destroy(gameObject);
                        else
                            enabled = false;

                        return;
                    }
                }

                sr.sprite = frames[currentFrame];
            }
        }

        //Projectile Factory
        public static class PetProjectileFactory
        {
            public static GameObject SpawnAnimatedProjectile(
                string animationName,
                Vector3 startPos,
                BasicEnemy target,
                float speed = 8f,
                float damage = 0.25f,
                float accuracy = 1f,
                int sortingOrderOffset = 5)
            {
                if (target == null)
                    return null;

                Sprite[] frames = PokeBloobs.GetAnimationFramesFromFolder("Attacks", animationName);
                if (frames == null || frames.Length == 0)
                {
                    if (animationName != "")
                    {
                        Debug.LogWarning($"[PokeBloobs] No projectile frames found for '{animationName}'");
                    }
                    return null;
                }

                startPos.z = 0f;

                GameObject go = new GameObject($"PetProjectile_{animationName}");
                go.transform.position = startPos;
                go.transform.localScale = Vector3.one * 2f;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = frames[0];
                sr.color = UnityEngine.Color.white;

                var petSr = PetManager.Instance?.activePets?
                    .FirstOrDefault(p => p != null)?
                    .GetComponent<SpriteRenderer>();

                if (petSr != null)
                {
                    sr.sortingLayerID = petSr.sortingLayerID;
                    sr.sortingOrder = petSr.sortingOrder + sortingOrderOffset;
                }
                else
                {
                    sr.sortingLayerName = "Default";
                    sr.sortingOrder = 999;
                }

                DynamicSpriteAnimator animator = go.AddComponent<DynamicSpriteAnimator>();
                animator.Initialize(frames, 0.08f, true, false);

                RuntimePetProjectile projectile = go.AddComponent<RuntimePetProjectile>();
                projectile.Initialize(target, speed, damage, accuracy);

                //Debug.Log($"[PokeBloobs] Spawned projectile '{go.name}' at {go.transform.position} with {frames.Length} frames");

                return go;
            }
        }

        //Force save things
        private void SavePokeBloobs()
        {
            var tr = Traverse.Create(PetManager.Instance);
            tr.Method("SavePets").GetValue();
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

            //foreach (var entry in DictionaryEvos.evolutionChains)
            //{
            //    string[] chain = entry.Value;
            //    int targetIndex = Array.IndexOf(chain, petName);

            //    if (targetIndex > 0)
            //    {
            //        for (int i = 0; i < targetIndex; i++)
            //        {
            //            requirements.Add(chain[i]);
            //        }
            //        return requirements;
            //    }
            //}
            //return requirements;

            foreach (var entry in DictionaryEvos.evolutionChains)
            {
                // entry.Value is a List<string[]>
                foreach (string[] chain in entry.Value)
                {
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
            }

            return requirements;
        }

        //Pet attack helper
        public static string GetAttackAnimationForPet(string petName)
        {
            var mon = SoulsDatabase.LoadedSouls.FirstOrDefault(s =>
                string.Equals(s.soulName, petName, StringComparison.OrdinalIgnoreCase));

            if (mon == null)
                return "BasicOrb";

            if (!string.IsNullOrEmpty(mon.PrimaryType))
                return mon.PrimaryType;

            return "BasicOrb";
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

        //Zip loader
        //public static class ZipAssetLoader
        //{
        //    private static string ZipPath =>
        //        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PokeBloobs.assets.zip");

        //    public static bool ZipExists()
        //    {
        //        return File.Exists(ZipPath);
        //    }

        //    public static byte[] LoadAssetBytes(string relativePath)
        //    {
        //        if (!File.Exists(ZipPath))
        //        {
        //            Debug.LogWarning($"[PokeBloobs] Asset zip not found at: {ZipPath}");
        //            return null;
        //        }

        //        using (ZipArchive zip = ZipFile.OpenRead(ZipPath))
        //        {
        //            string normalizedPath = relativePath.Replace("\\", "/");

        //            ZipArchiveEntry entry = zip.Entries.FirstOrDefault(e =>
        //                string.Equals(e.FullName, normalizedPath, StringComparison.OrdinalIgnoreCase));

        //            if (entry == null)
        //            {
        //                Debug.LogWarning($"[PokeBloobs] Asset not found in zip: {normalizedPath}");
        //                return null;
        //            }

        //            using (Stream stream = entry.Open())
        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                stream.CopyTo(ms);
        //                return ms.ToArray();
        //            }
        //        }
        //    }

        //    public static string FindAssetPath(string folderHint, string assetName, string extension)
        //    {
        //        if (!File.Exists(ZipPath))
        //        {
        //            Debug.LogWarning($"[PokeBloobs] Asset zip not found at: {ZipPath}");
        //            return null;
        //        }

        //        using (ZipArchive zip = ZipFile.OpenRead(ZipPath))
        //        {
        //            string ext = "." + extension.TrimStart('.');

        //            ZipArchiveEntry entry = zip.Entries.FirstOrDefault(e =>
        //                e.FullName.IndexOf(folderHint, StringComparison.OrdinalIgnoreCase) >= 0 &&
        //                Path.GetFileNameWithoutExtension(e.FullName).Equals(assetName, StringComparison.OrdinalIgnoreCase) &&
        //                e.FullName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));

        //            return entry?.FullName;
        //        }
        //    }

        //    public static string LoadJson(string name)
        //    {
        //        using (Stream stream = global::PokeBloobs.Classes.ZipAssetLoader.OpenAssetStream("Data", name, "json"))
        //        {
        //            if (stream == null)
        //                return null;

        //            using (StreamReader reader = new StreamReader(stream))
        //                return reader.ReadToEnd();
        //        }
        //    }
        //}
    }
}