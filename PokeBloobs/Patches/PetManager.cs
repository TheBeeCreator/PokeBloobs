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
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Patches
{
    internal class PetManagerP
    {
        internal static bool showPrompt = false;

        [HarmonyPatch(typeof(PetManager), "Start")]
        internal class Patch_Start
        {
            private const string SaveKey = "PokeBloobs_FirstRun";

            static void Postfix()
            {
                // Check save file
                if (ES3.Load<bool>(SaveKey, false))
                    return;

                // Mark as run
                ES3.Save(SaveKey, true);

                if (GameObject.Find("PetPromptUI") == null)
                {
                    new GameObject("PetPromptUI").AddComponent<PetPromptUI>();
                }

                showPrompt = true;

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
                    SoulBuilder.BuildSoul(soul);

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

    //ITS ABOUT TO GO DOWN
    internal class PetPromptUI : MonoBehaviour
    {
        private Vector2 scrollPos = Vector2.zero;
        private List<(string name, Texture2D tex)> starters = new();

        private bool initialized = false;
        private string pendingPet = null;
        private bool showConfirm = false;
        private bool showCloseConfirm = false;

        Texture2D LoadTexture(string fileName)
        {
            string path = Path.Combine(BepInEx.Paths.PluginPath, "PokeBloobs", "Sprites", "Gen1", fileName);

            if (!File.Exists(path))
            {
                Debug.LogError($"[PokeBloobs] Missing sprite: {path}");
                return Texture2D.whiteTexture;
            }

            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            return tex;
        }

        void Init()
        {
            if (initialized) return;

            starters = new List<(string, Texture2D)>
            {
                ("Bulbasaur", LoadTextureFromResource("Bulbasaur")),
                ("Charmander", LoadTextureFromResource("Charmander")),
                ("Squirtle", LoadTextureFromResource("Squirtle")),
                ("Chikorita", LoadTextureFromResource("Chikorita")),
                ("Cyndaquil", LoadTextureFromResource("Cyndaquil")),
                ("Totodile", LoadTextureFromResource("Totodile"))
            };

            initialized = true;
        }

        void OnGUI()
        {
            if (!PetManagerP.showPrompt) return;

            //if (PetManagerP.showPrompt)
            //{
            //    GUI.FocusControl(null); // remove focus from game

            //    if (Event.current.type != EventType.Repaint &&
            //        Event.current.type != EventType.Layout)
            //    {
            //        Event.current.Use();
            //    }
            //}

            Init();

            // Block background input
            GUI.depth = -1000;

            // Dark overlay
            GUI.color = new Color(0, 0, 0, 0.75f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            // Invisible input blocker (absorbs clicks)
            //GUI.Button(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, GUIStyle.none);

            // Panel setup
            int columns = 3;
            int rows = Mathf.CeilToInt(starters.Count / (float)columns);

            Rect panel = new Rect(
                Screen.width / 2 - 250,
                Screen.height / 2 - 200,
                500,
                400
            );

            // Border
            Rect borderRect = new Rect(panel.x - 2, panel.y - 2, panel.width + 4, panel.height + 4);
            GUI.color = new Color(0.2f, 0.12f, 0.05f);
            GUI.DrawTexture(borderRect, Texture2D.whiteTexture);

            // Fill
            GUI.color = new Color(0.55f, 0.38f, 0.22f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);

            GUI.color = Color.white;

            // Title
            GUI.Label(new Rect(panel.x + 150, panel.y + 20, 200, 30), "Choose your starter!");

            // Grid settings
            float spacingX = 150f;
            float spacingY = 170f;

            // 🪟 Scroll view
            Rect viewRect = new Rect(
                panel.x + 20,
                panel.y + 60,
                panel.width - 40,
                panel.height - 120
            );

            float contentHeight = rows * spacingY;

            Rect contentRect = new Rect(0, 0, viewRect.width - 20, contentHeight);

            scrollPos = GUI.BeginScrollView(viewRect, scrollPos, contentRect);

            // Draw starters
            for (int i = 0; i < starters.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;

                float x = col * spacingX + 20;
                float yPos = row * spacingY;

                var (name, tex) = starters[i];
                DrawStarter(x, yPos, tex, name);
            }

            GUI.EndScrollView();

            // Close button
            if (GUI.Button(new Rect(panel.x + 200, panel.y + panel.height - 50, 100, 30), "Close"))
            {
                showCloseConfirm = true;
            }

            // Close button
            if (showCloseConfirm)
            {
                Rect confirmRect = new Rect(panel.x + 50, panel.y + 50, 200, 100);
                GUI.Box(confirmRect, "Are you sure?");

                if (GUI.Button(new Rect(confirmRect.x + 10, confirmRect.y + 50, 80, 30), "Yes"))
                {
                    PetManagerP.showPrompt = false;
                    showCloseConfirm = false;
                }

                if (GUI.Button(new Rect(confirmRect.x + 110, confirmRect.y + 50, 80, 30), "No"))
                {
                    showCloseConfirm = false;
                }
            }

            // Confirmation dialog
            if (showConfirm && pendingPet != null)
            {
                DrawConfirmation();
            }
        }
        void DrawStarter(float x, float y, Texture2D tex, string name)
        {
            Rect rect = new Rect(x, y, 120, 120);

            // Hover effect
            bool hover = rect.Contains(Event.current.mousePosition);
            GUI.color = hover ? Color.gray : Color.white;

            if (GUI.Button(rect, tex))
            {
                //GivePet(name);
                pendingPet = name;
                showConfirm = true;
            }

            GUI.color = Color.white;

            // Name label
            GUI.Label(new Rect(x, y + 125, 120, 20), name);

            if (pendingPet == name)
            {
                GUI.color = Color.yellow;
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
        }

        void GivePet(string petName)
        {
            PetManagerP.showPrompt = false;

            SoulsData s = new SoulsData
            {
                soulName = petName,
                soulCategory = "event souls",
                skillName = "Homesteading",
                rarity = 5
            };

            var item = SoulBuilder.BuildSoul(s);

            if (item != null)
            {
                PetManager.Instance.AddPet(item);
            }

            PokeBloobs.spet = true;
        }

        void DrawConfirmation()
        {
            Rect box = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 75, 300, 150);

            // Darker overlay on top
            GUI.color = new Color(0, 0, 0, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Box(box, "");

            GUI.Label(new Rect(box.x + 30, box.y + 20, 240, 30),
                $"Are you sure you want {pendingPet}?");

            // YES button
            if (GUI.Button(new Rect(box.x + 40, box.y + 80, 80, 30), "Yes"))
            {
                GivePet(pendingPet);
                pendingPet = null;
                showConfirm = false;
            }

            // NO button
            if (GUI.Button(new Rect(box.x + 180, box.y + 80, 80, 30), "No"))
            {
                pendingPet = null;
                showConfirm = false;
            }
        }

        Texture2D LoadTextureFromResource(string resourceName)
        {
            var assembly = typeof(PokeBloobs).Assembly;

            string manifestName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.IndexOf("Sprites", StringComparison.OrdinalIgnoreCase) >= 0
                                    && n.IndexOf(resourceName, StringComparison.OrdinalIgnoreCase) >= 0
                                    && n.EndsWith(".png", StringComparison.OrdinalIgnoreCase));

            if (manifestName == null)
            {
                Debug.LogError($"[PokeBloobs] Resource not found: {resourceName}");
                return Texture2D.whiteTexture;
            }

            using (var stream = assembly.GetManifestResourceStream(manifestName))
            {
                if (stream == null)
                {
                    Debug.LogError($"[PokeBloobs] Failed to load stream: {manifestName}");
                    return Texture2D.whiteTexture;
                }

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(buffer);
                return tex;
            }
        }
    }
}
