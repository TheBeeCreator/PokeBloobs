using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Classes
{
    internal class SoulBuilder
    {
        public static Item BuildSoul(SoulsData s)
        {
            if (s == null) return null;

            var item = CreateBaseSoulItem(s);

            float xp = CalculateSoulXP(s.rarity);

            ApplySkillBonus(item, s.skillName, xp, s.rarity);

            item.commonImage = GetSprite(s.soulName)
                               ?? GetFirstAnimationFrame(s.soulName);

            return item;
        }

        private static Item CreateBaseSoulItem(SoulsData s)
        {
            var item = ScriptableObject.CreateInstance<Item>();

            item.name = s.soulName;
            item.itemName = s.soulName;
            item.information = "pet";

            return item;
        }

        private static float CalculateSoulXP(int rarity)
        {
            float baseXp = 0.025f;

            int multiplier = rarity switch
            {
                1 => 2,
                2 => 3,
                3 => 4,
                4 => 4,
                5 => 10,
                _ => 1
            };

            float xp = baseXp * multiplier;

            float prestige = PlayerDataManager.Instance.GetSoulBindingPrestigeLevel();

            xp *= prestige switch
            {
                1 => 1.3f,
                2 => 1.5f,
                3 => 1.7f,
                4 => 1.9f,
                >= 5 => 2.0f,
                _ => 1f
            };

            return Mathf.Clamp(xp, 0.025f, 0.10f);
        }

        private static void ApplySkillBonus(Item item, string skill, float xp, int rarity)
        {
            if (SkillBonusMap.TryGetValue(skill, out var action))
            {
                action(item, xp, rarity);
            }
        }

        //Skill bonus builder
        public static void BuildBonus(string s)
        {
            if (s == null) return;
        }

        private static readonly Dictionary<string, Action<Item, float, int>> SkillBonusMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Hitpoints"] = (item, xp, rarity) =>
            {
                item.hitPointsBonusXp = xp;
                item.defenceBonusXP = xp / 2;
            },

            ["Attack"] = (item, xp, rarity) =>
            {
                item.attackBonusXP = xp;
                item.accuracy = xp / 5;
                item.critalChance = xp / 5;
            },

            ["Strength"] = (item, xp, rarity) =>
            {
                item.strengthBonusXp = xp;
                item.meleeSoulDamage = xp / 2;
                item.accuracy = xp / 5;
            },

            ["Defense"] = (item, xp, rarity) =>
            {
                item.defenceBonusXP = xp;
                item.hitPointsBonusXp = xp / 2;
            },

            ["Ranged"] = (item, xp, rarity) =>
            {
                item.rangeBonusXP = xp;
                item.rangedSoulDamage = xp / 2;
                item.rangeAccuracy = xp / 5;
            },

            ["Magic"] = (item, xp, rarity) =>
            {
                item.magicBonusXP = xp;
                item.magicSoulDamage = xp / 2;
                item.magicAccuracy = xp / 5;
            },

            ["Devotion"] = (item, xp, rarity) =>
            {
                item.devotionBonusXp = xp;
                item.beastMateryBonusXp = xp / 2;
            },

            ["Beastmastery"] = (item, xp, rarity) =>
            {
                item.beastMateryBonusXp = xp;
                item.attackBonusXP = xp / 5;
                item.defenceBonusXP = xp / 5;
                item.strengthBonusXp = xp / 5;
                item.rangeBonusXP = xp / 5;
                item.magicBonusXP = xp / 5;
            },

            ["Dexterity"] = (item, xp, rarity) =>
            {
                item.dexterityBonusXp = xp;
                item.thievingBonusXp = xp / 3;
            },

            ["Foraging"] = (item, xp, rarity) =>
            {
                item.foragingBonusXp = xp;
                item.herbologyBonusXp = xp / 3;
            },

            ["Herblore"] = (item, xp, rarity) =>
            {
                item.herbologyBonusXp = xp;
                item.foragingBonusXp = xp / 2;
            },

            ["Crafting"] = (item, xp, rarity) =>
            {
                item.craftingBonusXp = xp;
                item.bowCraftingBonusXp = xp;
            },

            ["Bowcrafting"] = (item, xp, rarity) =>
            {
                item.bowCraftingBonusXp = xp;
                item.craftingBonusXp = xp;
            },

            ["Imbuing"] = (item, xp, rarity) =>
            {
                item.imbuingBonusXp = xp;
                item.magicBonusXP = xp / 5;
            },

            ["Thieving"] = (item, xp, rarity) =>
            {
                item.thievingBonusXp = xp;
                item.dexterityBonusXp = xp / 5;
            },

            ["Soulbinding"] = (item, xp, rarity) =>
            {
                item.soulBindingBonusXp = xp * 2;
            },

            ["Mining"] = (item, xp, rarity) =>
            {
                item.miningBonusXp = xp;
                item.smithingBonusXp = xp;
            },

            ["Smithing"] = (item, xp, rarity) =>
            {
                item.smithingBonusXp = xp;
                item.miningBonusXp = xp;
            },

            ["Fishing"] = (item, xp, rarity) =>
            {
                item.fishingBonusXp = xp;
                item.cookingBonusXp = xp / 5;
            },

            ["Cooking"] = (item, xp, rarity) =>
            {
                item.cookingBonusXp = xp;
                item.fishingBonusXp = xp / 5;
            },

            ["Woodcutting"] = (item, xp, rarity) =>
            {
                item.woodcuttingBonusXp = xp;
                item.firemakingBonusXp = xp / 2;
            },

            ["Firemaking"] = (item, xp, rarity) =>
            {
                item.firemakingBonusXp = xp;
                item.woodcuttingBonusXp = xp / 2;
            },

            ["Tracking"] = (item, xp, rarity) =>
            {
                item.trackingBonusXp = xp;
                item.doubleTrackingLoot = rarity / 2;
            },

            ["Homesteading"] = (item, xp, rarity) =>
            {
                item.homesteadingBonusXp = xp;
                item.woodcuttingBonusXp = xp / 3;
                item.miningBonusXp = xp / 3;
                item.fishingBonusXp = xp / 3;
                item.foragingBonusXp = xp / 3;
            }
        };
    }
}
