using PokeBloobs.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Classes
{
    internal class MenuModVersion
    {
        internal class VersionSelectUI : MonoBehaviour
        {
            private bool showConfirm = false;
            private ModVersionMode pendingSelection;
            public static bool ShowPrompt = false;

            void OnGUI()
            {
                if (!ShowPrompt) return;

                GUI.depth = -1000;

                // Background dim
                GUI.color = new Color(0f, 0f, 0f, 0.8f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;

                Rect panel = new Rect(
                    Screen.width / 2 - 340,
                    Screen.height / 2 - 220,
                    680,
                    440
                );

                DrawPanel(panel);

                GUI.Label(
                    new Rect(panel.x + 210, panel.y + 20, 260, 30),
                    "Choose Your Mod Version"
                );

                GUI.Label(
                    new Rect(panel.x + 120, panel.y + 50, 450, 25),
                    "Pick the version of the mod you want to play."
                );

                float optionWidth = 180f;
                float optionHeight = 50f;
                float descHeight = 150f;
                float spacing = 25f;
                float startX = panel.x + 35f;
                float topY = panel.y + 100f;

                DrawOption(
                    new Rect(startX, topY, optionWidth, optionHeight),
                    new Rect(startX, topY + 60f, optionWidth, descHeight),
                    "Cosmetic",
                    "This version offers no bonuses.\n\nBest for players who just want to collect them all",
                    ModVersionMode.Cosmetic
                );

                DrawOption(
                    new Rect(startX + optionWidth + spacing, topY, optionWidth, optionHeight),
                    new Rect(startX + optionWidth + spacing, topY + 60f, optionWidth, descHeight),
                    "Normal",
                    "Pets give XP bonuses and small bonuses here and there.\n\nBest for players who want to collect them and add some bonuses to there experience",
                    ModVersionMode.Normal
                );

                DrawOption(
                    new Rect(startX + (optionWidth + spacing) * 2, topY, optionWidth, optionHeight),
                    new Rect(startX + (optionWidth + spacing) * 2, topY + 60f, optionWidth, descHeight),
                    "Chaotic",
                    "This is where the fun begins.\n\nSouls have attacks, and will help you with your skills via experience.",
                    ModVersionMode.Chaotic
                );

                if (showConfirm)
                {
                    DrawConfirmBox();
                }

                // Prevent click-through after menu has processed input
                if (Event.current.type == EventType.MouseDown ||
                    Event.current.type == EventType.MouseUp ||
                    Event.current.type == EventType.MouseDrag ||
                    Event.current.type == EventType.ScrollWheel ||
                    Event.current.isKey)
                {
                    Event.current.Use();
                }
            }

            void DrawPanel(Rect panel)
            {
                Rect borderRect = new Rect(panel.x - 2, panel.y - 2, panel.width + 4, panel.height + 4);

                GUI.color = new Color(0.2f, 0.12f, 0.05f);
                GUI.DrawTexture(borderRect, Texture2D.whiteTexture);

                GUI.color = new Color(0.55f, 0.38f, 0.22f);
                GUI.DrawTexture(panel, Texture2D.whiteTexture);

                GUI.color = Color.white;
            }

            void DrawOption(Rect buttonRect, Rect descRect, string label, string description, ModVersionMode mode)
            {
                bool isCurrent = ModSettings.SelectedVersion == mode;
                bool isPending = showConfirm && pendingSelection == mode;

                Color oldColor = GUI.color;

                if (isCurrent)
                    GUI.color = new Color(0.85f, 0.95f, 0.4f);
                else if (buttonRect.Contains(Event.current.mousePosition))
                    GUI.color = Color.gray;
                else
                    GUI.color = Color.white;

                if (GUI.Button(buttonRect, label))
                {
                    pendingSelection = mode;
                    showConfirm = true;
                }

                GUI.color = new Color(0.18f, 0.18f, 0.18f);
                GUI.DrawTexture(descRect, Texture2D.whiteTexture);

                GUI.color = isPending ? Color.yellow : Color.white;
                GUI.Box(descRect, "");

                GUI.color = Color.white;
                GUI.Label(
                    new Rect(descRect.x + 10, descRect.y + 10, descRect.width - 20, descRect.height - 20),
                    description
                );

                GUI.color = oldColor;
            }

            void DrawConfirmBox()
            {
                Rect box = new Rect(Screen.width / 2 - 170, Screen.height / 2 - 80, 340, 160);

                GUI.color = new Color(0f, 0f, 0f, 0.9f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUI.Box(box, "");

                GUI.Label(
                    new Rect(box.x + 30, box.y + 25, 280, 30),
                    $"Play {pendingSelection} mode?"
                );

                if (GUI.Button(new Rect(box.x + 40, box.y + 90, 100, 35), "Yes"))
                {
                    ConfirmVersionSelection(pendingSelection);
                    showConfirm = false;
                }

                if (GUI.Button(new Rect(box.x + 200, box.y + 90, 100, 35), "No"))
                {
                    showConfirm = false;
                }
            }

            void ConfirmVersionSelection(ModVersionMode mode)
            {
                ModSettings.SelectedVersion = mode;
                ModSettings.HasChosenVersion = true;

                ES3.Save(ModSettings.VersionMode, mode.ToString());
                StartCoroutine(PetRefreshHelper.ResummonDelayed());

                ShowPrompt = false;

                if (!OpenedFromF9)
                {
                    if (GameObject.Find("PetPromptUI") == null)
                        new GameObject("PetPromptUI").AddComponent<PetPromptUI>();

                    PetManagerP.showPrompt = true;
                }
                else
                {
                    PetManagerP.showPrompt = false;
                }

                OpenedFromF9 = false;
            }
        }
    }
}
