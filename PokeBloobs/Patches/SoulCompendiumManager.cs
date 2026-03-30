using HarmonyLib;
using PokeBloobs.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Patches
{
    internal class SoulCompendiumManagerP
    {
        [HarmonyPatch(typeof(SoulCompendiumManager), "GetPetSprite")]
        public static class Patch_CompendiumSpriteFix
        {
            static bool Prefix(string petName, ref Sprite __result)
            {
                var customSoul = PokeBloobs.SoulsDatabase.LoadedSouls
                    .FirstOrDefault(s => s.soulName.Equals(petName, StringComparison.OrdinalIgnoreCase));

                if (customSoul != null)
                {
                    var sprite = PokeBloobs.GetSprite(customSoul.soulName);
                    __result = ResizeSprite(sprite, 2f);
                    return false;
                }
                return true;
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
                SaveHelpers.RefreshCustomPetBonuses();
                var tr = Traverse.Create(__instance);

                var petNames = tr.Field("petNames").GetValue<List<string>>();
                var petNameLookup = tr.Field("petNameLookup").GetValue<Dictionary<string, string>>();
                var categorizedBlocks = tr.Field("categorizedBlocks").GetValue<Dictionary<string, List<string>>>();

                if (petNames == null || petNameLookup == null || categorizedBlocks == null)
                    return;

                foreach (SoulsData soul in PokeBloobs.SoulsDatabase.LoadedSouls)
                {
                    if (!petNames.Contains(soul.soulName))
                    {
                        petNames.Add(soul.soulName);
                        petNameLookup[soul.soulName] = soul.soulName;

                        if (categorizedBlocks.ContainsKey(soul.soulCategory))
                            categorizedBlocks[soul.soulCategory].Add(soul.soulName);
                        else
                            categorizedBlocks["uncategorized"].Add(soul.soulName);

                        __instance.UpdateSinglePetUI(soul.soulName);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SoulCompendiumManager), "AddTooltip")]
        public static class Patch_SoulCompendiumAddTooltip
        {
            private static readonly HashSet<string> _customSoulNames =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            private static readonly MethodInfo _openSummonMenuMethod =
                AccessTools.Method(typeof(SoulCompendiumManager), "OpenSummonMenu", new[] { typeof(string) });

            private static readonly MethodInfo _showTooltipMethod =
                AccessTools.Method(typeof(SoulCompendiumManager), "ShowTooltip", new[] { typeof(string) });

            private static readonly MethodInfo _hideTooltipMethod =
                AccessTools.Method(typeof(SoulCompendiumManager), "HideTooltip", Type.EmptyTypes);

            static void Postfix(SoulCompendiumManager __instance, GameObject entry, string petName)
            {
                if (__instance == null || entry == null || string.IsNullOrEmpty(petName))
                    return;

                RefreshCustomSoulCache();

                if (!_customSoulNames.Contains(petName))
                    return;

                if (!entry.TryGetComponent<Button>(out var button))
                    button = entry.AddComponent<Button>();

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    _openSummonMenuMethod?.Invoke(__instance, new object[] { petName });
                });

                if (!entry.TryGetComponent<EventTrigger>(out var trigger))
                    trigger = entry.AddComponent<EventTrigger>();

                trigger.triggers ??= new List<EventTrigger.Entry>();
                trigger.triggers.Clear();

                var enter = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                enter.callback.AddListener(_ =>
                {
                    _showTooltipMethod?.Invoke(__instance, new object[] { petName });
                });
                trigger.triggers.Add(enter);

                var exit = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerExit
                };
                exit.callback.AddListener(_ =>
                {
                    _hideTooltipMethod?.Invoke(__instance, null);
                });
                trigger.triggers.Add(exit);
            }

            private static void RefreshCustomSoulCache()
            {
                if (_customSoulNames.Count == PokeBloobs.SoulsDatabase.LoadedSouls.Count)
                    return;

                _customSoulNames.Clear();

                foreach (var soul in PokeBloobs.SoulsDatabase.LoadedSouls)
                {
                    if (!string.IsNullOrEmpty(soul?.soulName))
                        _customSoulNames.Add(soul.soulName);
                }
            }
        }

        [HarmonyPatch(typeof(SoulCompendiumManager), "ShowTooltip")]
        public static class Patch_SoulCompendiumShowTooltip
        {
            static bool Prefix(SoulCompendiumManager __instance, string petName)
            {
                if (string.IsNullOrEmpty(petName))
                    return true;

                var customSoul = PokeBloobs.SoulsDatabase.LoadedSouls
                    .FirstOrDefault(s => s.soulName.Equals(petName, StringComparison.OrdinalIgnoreCase));

                if (customSoul == null)
                    return true;

                if (!PetManager.Instance.HasPet(petName))
                    return false;

                Item item = PetManager.Instance.collectedPets.Find(p => p.itemName == petName);
                if (item == null)
                    return false;

                string tooltipText = BuildCustomTooltipText(customSoul, item);
                string coloredText = Regex.Replace(tooltipText, "(\\d+(\\.\\d+)?%?)", "<color=yellow>$1</color>");

                Traverse.Create(__instance)
                    .Field("tooltipUI")
                    .GetValue<TooltipUI>()
                    .SetAndShowTooltip(coloredText);

                return false;
            }

            private static string BuildCustomTooltipText(SoulsData soul, Item item)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(soul.soulName);

                string description = GetSoulDescription(soul, item);
                if (!string.IsNullOrEmpty(description))
                {
                    sb.AppendLine(description);
                }

                AppendStatLines(sb, item);

                return sb.ToString().TrimEnd();
            }

            private static string GetSoulDescription(SoulsData soul, Item item)
            {
                if (!string.IsNullOrWhiteSpace(item.information) &&
                    !string.Equals(item.information, "Pet", StringComparison.OrdinalIgnoreCase))
                {
                    return item.information;
                }

                switch (ModSettings.SelectedVersion)
                {
                    case ModVersionMode.Cosmetic:
                        return "A collectible companion with no gameplay bonuses.";

                    case ModVersionMode.Normal:
                        return $"Grants bonuses tied to {soul.skillName}.";

                    case ModVersionMode.Chaotic:
                        return $"A combat-ready soul tied to {soul.skillName}.";

                    default:
                        return "A custom companion on your journey.";
                }
            }

            private static void AppendStatLines(StringBuilder sb, Item item)
            {
                // Combat Damage
                if (item.meleeSoulDamage > 0f)
                    sb.AppendLine($"Soul awards {item.meleeSoulDamage:0.####} bonus Melee Damage.");

                if (item.rangedSoulDamage > 0f)
                    sb.AppendLine($"Soul awards {item.rangedSoulDamage:0.####} bonus Ranged Damage.");

                if (item.magicSoulDamage > 0f)
                    sb.AppendLine($"Soul awards {item.magicSoulDamage:0.####} bonus Magic Damage.");

                // Accuracy
                if (item.accuracy > 0f)
                    sb.AppendLine($"Soul awards {item.accuracy * 100f:0.##}% bonus Melee Accuracy.");

                if (item.rangeAccuracy > 0f)
                    sb.AppendLine($"Soul awards {item.rangeAccuracy * 100f:0.##}% bonus Ranged Accuracy.");

                if (item.magicAccuracy > 0f)
                    sb.AppendLine($"Soul awards {item.magicAccuracy * 100f:0.##}% bonus Magic Accuracy.");

                // Crit / Special
                if (item.critalChance > 0f)
                    sb.AppendLine($"Soul awards {item.critalChance * 100f:0.####}% bonus Crit Chance.");

                if (item.doubleTrackingLoot > 0f)
                    sb.AppendLine($"Tracking Soul awards {item.doubleTrackingLoot * 100f:0.####}% bonus Double Resource Chance.");

                // Combat XP
                if (item.attackBonusXP > 0f)
                    sb.AppendLine($"Attack Soul awards {item.attackBonusXP * 100f:0.####}% bonus experience.");

                if (item.strengthBonusXp > 0f)
                    sb.AppendLine($"Strength Soul awards {item.strengthBonusXp * 100f:0.####}% bonus experience.");

                if (item.defenceBonusXP > 0f)
                    sb.AppendLine($"Defense Soul awards {item.defenceBonusXP * 100f:0.####}% bonus experience.");

                if (item.hitPointsBonusXp > 0f)
                    sb.AppendLine($"Hitpoints Soul awards {item.hitPointsBonusXp * 100f:0.####}% bonus experience.");

                if (item.rangeBonusXP > 0f)
                    sb.AppendLine($"Ranged Soul awards {item.rangeBonusXP * 100f:0.####}% bonus experience.");

                if (item.magicBonusXP > 0f)
                    sb.AppendLine($"Magic Soul awards {item.magicBonusXP * 100f:0.####}% bonus experience.");

                // Gathering XP
                if (item.woodcuttingBonusXp > 0f)
                    sb.AppendLine($"Woodcutting Soul awards {item.woodcuttingBonusXp * 100f:0.####}% bonus experience.");

                if (item.miningBonusXp > 0f)
                    sb.AppendLine($"Mining Soul awards {item.miningBonusXp * 100f:0.####}% bonus experience.");

                if (item.fishingBonusXp > 0f)
                    sb.AppendLine($"Fishing Soul awards {item.fishingBonusXp * 100f:0.####}% bonus experience.");

                if (item.foragingBonusXp > 0f)
                    sb.AppendLine($"Foraging Soul awards {item.foragingBonusXp * 100f:0.####}% bonus experience.");

                if (item.trackingBonusXp > 0f)
                    sb.AppendLine($"Tracking Soul awards {item.trackingBonusXp * 100f:0.####}% bonus experience.");

                // Crafting / Processing
                if (item.craftingBonusXp > 0f)
                    sb.AppendLine($"Crafting Soul awards {item.craftingBonusXp * 100f:0.####}% bonus experience.");

                if (item.bowCraftingBonusXp > 0f)
                    sb.AppendLine($"Bowcrafting Soul awards {item.bowCraftingBonusXp * 100f:0.####}% bonus experience.");

                if (item.smithingBonusXp > 0f)
                    sb.AppendLine($"Smithing Soul awards {item.smithingBonusXp * 100f:0.####}% bonus experience.");

                if (item.firemakingBonusXp > 0f)
                    sb.AppendLine($"Firemaking Soul awards {item.firemakingBonusXp * 100f:0.####}% bonus experience.");

                if (item.cookingBonusXp > 0f)
                    sb.AppendLine($"Cooking Soul awards {item.cookingBonusXp * 100f:0.####}% bonus experience.");

                // Utility / Misc
                if (item.dexterityBonusXp > 0f)
                    sb.AppendLine($"Dexterity Soul awards {item.dexterityBonusXp * 100f:0.####}% bonus experience.");

                if (item.thievingBonusXp > 0f)
                    sb.AppendLine($"Thieving Soul awards {item.thievingBonusXp * 100f:0.####}% bonus experience.");

                if (item.imbuingBonusXp > 0f)
                    sb.AppendLine($"Imbuing Soul awards {item.imbuingBonusXp * 100f:0.####}% bonus experience.");

                if (item.herbologyBonusXp > 0f)
                    sb.AppendLine($"Herblore Soul awards {item.herbologyBonusXp * 100f:0.####}% bonus experience.");

                if (item.soulBindingBonusXp > 0f)
                    sb.AppendLine($"Soulbinding Soul awards {item.soulBindingBonusXp * 100f:0.####}% bonus experience.");

                if (item.devotionBonusXp > 0f)
                    sb.AppendLine($"Devotion Soul awards {item.devotionBonusXp * 100f:0.####}% bonus experience.");

                if (item.beastMateryBonusXp > 0f)
                    sb.AppendLine($"Beastmastery Soul awards {item.beastMateryBonusXp * 100f:0.####}% bonus experience.");

                if (item.homesteadingBonusXp > 0f)
                    sb.AppendLine($"Homesteading Soul awards {item.homesteadingBonusXp * 100f:0.####}% bonus experience.");
            }
        }

        private static Sprite ResizeSprite(Sprite original, float scale)
        {
            if (original == null) return null;

            return Sprite.Create(
                original.texture,
                original.rect,
                original.pivot,
                original.pixelsPerUnit / scale
            );
        }
    }
}