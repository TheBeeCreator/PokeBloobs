using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Classes
{
    public static class SaveHelpers
    {
        //public static void ApplyVersionSelection(ModVersionMode mode)
        //{
        //    ModSettings.SelectedVersion = mode;

        //    ES3.Save(ModSettings.VersionMode, mode.ToString());
        //    //ES3.Save(ModSettings.ver, true);

        //    ModSettings.ShowVersionPrompt = false;

        //    Debug.Log($"[PokeBloobs] Saved version selection: {mode}");
        //}

        //public static void ApplyStarterRun(bool firstrun)
        //{
        //    ES3.Save(ModSettings.FirstRun, firstrun.ToString());

        //    ModSettings.HasFirstRun = true;

        //    Debug.Log($"[PokeBloobs] Saved starter selection");
        //}

        public static void LoadVersionSetting()
        {
            string savedMode = ES3.Load<string>(ModSettings.VersionMode, defaultValue: "");

            if (!string.IsNullOrEmpty(savedMode) &&
                Enum.TryParse(savedMode, out ModVersionMode loadedMode))
            {
                ModSettings.SelectedVersion = loadedMode;
                ModSettings.HasChosenVersion = true;
            }
            else
            {
                ModSettings.SelectedVersion = ModVersionMode.Cosmetic;
                ModSettings.HasChosenVersion = false;
            }
        }

        public static void LoadStarterSetting()
        {
            ModSettings.HasChosenStarter = ES3.Load<bool>(ModSettings.StarterChosen, defaultValue: false);
        }

        public static void RefreshCustomPetBonuses()
        {
            Debug.Log("[PokeBloobs] RefreshCustomPetBonuses entered");

            if (PetManager.Instance == null)
            {
                Debug.Log("[PokeBloobs] Refresh aborted: PetManager.Instance is null");
                return;
            }

            if (PetManager.Instance.collectedPets == null)
            {
                Debug.Log("[PokeBloobs] Refresh aborted: collectedPets is null");
                return;
            }

            Debug.Log($"[PokeBloobs] Refresh running on {PetManager.Instance.collectedPets.Count} pets");

            for (int i = 0; i < PetManager.Instance.collectedPets.Count; i++)
            {
                Item existing = PetManager.Instance.collectedPets[i];
                if (existing == null || string.IsNullOrWhiteSpace(existing.itemName))
                    continue;

                Debug.Log($"[PokeBloobs] Checking pet {existing.itemName}");

                SoulsData soulData = SoulsDatabase.LoadedSouls
                    .FirstOrDefault(s => s.soulName.Equals(existing.itemName, StringComparison.OrdinalIgnoreCase));

                if (soulData == null)
                    continue;

                Debug.Log($"[PokeBloobs] Rebuilding {existing.itemName}");

                Item rebuilt = SoulBuilder.BuildSoul(soulData);
                if (rebuilt == null)
                    continue;

                CopyCustomSoulStats(rebuilt, existing);

                Debug.Log($"[PokeBloobs] Refreshed {existing.itemName}");
            }
        }

        private static void CopyCustomSoulStats(Item source, Item target)
        {
            if (source == null || target == null)
                return;

            target.information = source.information;
            target.commonImage = source.commonImage;

            target.hitPointsBonusXp = source.hitPointsBonusXp;
            target.attackBonusXP = source.attackBonusXP;
            target.strengthBonusXp = source.strengthBonusXp;
            target.defenceBonusXP = source.defenceBonusXP;
            target.rangeBonusXP = source.rangeBonusXP;
            target.magicBonusXP = source.magicBonusXP;
            target.devotionBonusXp = source.devotionBonusXp;
            target.beastMateryBonusXp = source.beastMateryBonusXp;
            target.dexterityBonusXp = source.dexterityBonusXp;
            target.foragingBonusXp = source.foragingBonusXp;
            target.herbologyBonusXp = source.herbologyBonusXp;
            target.craftingBonusXp = source.craftingBonusXp;
            target.bowCraftingBonusXp = source.bowCraftingBonusXp;
            target.imbuingBonusXp = source.imbuingBonusXp;
            target.thievingBonusXp = source.thievingBonusXp;
            target.soulBindingBonusXp = source.soulBindingBonusXp;
            target.miningBonusXp = source.miningBonusXp;
            target.smithingBonusXp = source.smithingBonusXp;
            target.fishingBonusXp = source.fishingBonusXp;
            target.cookingBonusXp = source.cookingBonusXp;
            target.woodcuttingBonusXp = source.woodcuttingBonusXp;
            target.firemakingBonusXp = source.firemakingBonusXp;
            target.trackingBonusXp = source.trackingBonusXp;
            target.homesteadingBonusXp = source.homesteadingBonusXp;

            target.meleeSoulDamage = source.meleeSoulDamage;
            target.rangedSoulDamage = source.rangedSoulDamage;
            target.magicSoulDamage = source.magicSoulDamage;
            target.accuracy = source.accuracy;
            target.rangeAccuracy = source.rangeAccuracy;
            target.magicAccuracy = source.magicAccuracy;
            target.critalChance = source.critalChance;
            target.doubleTrackingLoot = source.doubleTrackingLoot;
        }

        public static void RefreshActivePetInstances()
        {
            if (PetManager.Instance == null)
                return;

            var activeNames = new List<string>();

            for (int i = 0; i < PetManager.Instance.activePets.Count; i++)
            {
                GameObject activePet = PetManager.Instance.activePets[i];
                activeNames.Add(activePet != null ? activePet.name : null);
            }

            for (int i = 0; i < activeNames.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(activeNames[i]))
                    continue;

                PetManager.Instance.UnSummonPetByIndex(i);
            }

            for (int i = 0; i < activeNames.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(activeNames[i]))
                    continue;

                PetManager.Instance.SummonPetBySlot(activeNames[i], i);
            }
        }
    }
}
