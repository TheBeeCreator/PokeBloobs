using PokeBloobs.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Classes
{
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

            //ZipAssetLoader.BuildIndex();

            starters = new List<(string, Texture2D)>
            {
                ("Bulbasaur", LoadTextureFromZip("Sprites/Gen1", "Bulbasaur")),
                ("Charmander", LoadTextureFromZip("Sprites/Gen1", "Charmander")),
                ("Squirtle", LoadTextureFromZip("Sprites/Gen1", "Squirtle")),
                ("Chikorita", LoadTextureFromZip("Sprites/Gen2", "Chikorita")),
                ("Cyndaquil", LoadTextureFromZip("Sprites/Gen2", "Cyndaquil")),
                ("Totodile", LoadTextureFromZip("Sprites/Gen2", "Totodile"))
            };

            initialized = true;
        }

        void OnGUI()
        {
            if (!PetManagerP.showPrompt) return;

            //// Block all input behind UI
            //GUI.FocusControl(null);

            //if (Event.current.type != EventType.Repaint &&
            //    Event.current.type != EventType.Layout)
            //{
            //    Event.current.Use();
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

            // Block click-through AFTER UI has handled the event
            if (Event.current.type == EventType.MouseDown ||
                Event.current.type == EventType.MouseUp ||
                Event.current.type == EventType.MouseDrag ||
                Event.current.type == EventType.ScrollWheel)
            {
                Event.current.Use();
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

            SoulsData s = SoulsDatabase.LoadedSouls
                .FirstOrDefault(x => x.soulName.Equals(petName, StringComparison.OrdinalIgnoreCase));

            if (s == null)
            {
                Debug.LogWarning($"[PokeBloobs] Could not find soul data for {petName}");
                return;
            }

            var item = SoulBuilder.BuildSoul(s);

            if (item != null)
            {
                PetManager.Instance.AddPet(item);
            }

            PokeBloobs.spet = true;
        }

        void DrawConfirmation()
        {
            if (string.IsNullOrEmpty(pendingPet))
            {
                showConfirm = false;
                return;
            }

            Rect box = new Rect(Screen.width / 2f - 150f, Screen.height / 2f - 75f, 300f, 150f);

            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Box(box, string.Empty);
            GUI.Label(new Rect(box.x + 30f, box.y + 20f, 240f, 30f),
                $"Are you sure you want {pendingPet}?");

            if(GUI.Button(new Rect(box.x + 40f, box.y + 80f, 80f, 30f), "Yes"))
{
                string selectedPet = pendingPet;

                pendingPet = null;
                showConfirm = false;
                PetManagerP.showPrompt = false;

                ES3.Save(ModSettings.StarterChosen, true);
                ModSettings.HasChosenStarter = true;

                GivePet(selectedPet);
            }

            if (GUI.Button(new Rect(box.x + 180f, box.y + 80f, 80f, 30f), "No"))
            {
                pendingPet = null;
                showConfirm = false;
            }
        }

        Texture2D LoadTextureFromZip(string folderHint, string assetName)
        {
            using (Stream stream = ZipAssetLoader.OpenAssetStream(folderHint, assetName, ".png"))
            {
                if (stream == null)
                {
                    Debug.LogError($"[PokeBloobs] Zip texture not found: folder={folderHint}, asset={assetName}");
                    return Texture2D.whiteTexture;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    byte[] data = ms.ToArray();

                    Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    tex.LoadImage(data);
                    return tex;
                }
            }
        }

        void ConfirmStarterSelection()
        {
            if (string.IsNullOrEmpty(pendingPet))
                return;

            string selectedPet = pendingPet;

            pendingPet = null;
            showConfirm = false;
            PetManagerP.showPrompt = false;

            ModSettings.HasChosenStarter = true;
            ES3.Save(ModSettings.StarterChosen, true);

            Debug.Log($"[PokeBloobs] Giving starter: {selectedPet}");
            GivePet(selectedPet);
            Debug.Log("[PokeBloobs] Starter granted successfully");
        }
    }
}
