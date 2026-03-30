using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Classes
{
    public static class PetRefreshHelper
    {
        private static bool _didStartupResummon;

        public static IEnumerator DelayedStartupResummon()
        {
            if (_didStartupResummon)
                yield break;

            _didStartupResummon = true;

            yield return null;
            yield return null;

            var pm = PetManager.Instance;
            if (pm == null)
                yield break;

            List<string> activePetNames = new List<string>();

            for (int i = 0; i < pm.activePets.Count; i++)
            {
                if (pm.activePets[i] != null)
                    activePetNames.Add(pm.activePets[i].name);
            }

            for (int i = 0; i < pm.activePets.Count; i++)
                pm.UnSummonPetByIndex(i);

            yield return null;

            foreach (var petName in activePetNames)
            {
                var item = pm.collectedPets.Find(p => p.itemName == petName);
                if (item != null)
                    pm.SummonPet(item);
            }

            Debug.Log("[PokeBloobs] One-time startup resummon complete");
        }

        public static void ResetForNewSession()
        {
            _didStartupResummon = false;
        }

        public static IEnumerator ResummonDelayed()
        {
            yield return null;
            ResummonAllPets();
        }

        public static void ResummonAllPets()
        {
            var pm = PetManager.Instance;
            if (pm == null) return;

            // store active pet names
            List<string> activePetNames = new List<string>();

            for (int i = 0; i < pm.activePets.Count; i++)
            {
                var pet = pm.activePets[i];
                if (pet != null)
                    activePetNames.Add(pet.name);
            }

            // unsummon all
            for (int i = 0; i < pm.activePets.Count; i++)
            {
                pm.UnSummonPetByIndex(i);
            }

            // resummon
            foreach (var petName in activePetNames)
            {
                var item = pm.collectedPets.Find(p => p.itemName == petName);
                if (item != null)
                {
                    pm.SummonPet(item);
                }
            }

            pm.ReapplyAllPetBonuses();
            pm.UpdatePetBuffsDisplay();

            Debug.Log("[PokeBloobs] Resummoned all pets");
        }
        public static void ForceRefreshAllPetData()
        {
            SaveHelpers.RefreshCustomPetBonuses();
            SaveHelpers.RefreshActivePetInstances();

            if (PetManager.Instance != null)
            {
                PetManager.Instance.ReapplyAllPetBonuses();
                PetManager.Instance.UpdatePetBuffsDisplay();
            }

            if (SoulCompendiumManager.Instance != null)
            {
                SoulCompendiumManager.Instance.UpdateActivePetsUI();
                SoulCompendiumManager.Instance.UpdateCompendiumUI();
            }

            Debug.Log("[PokeBloobs] Forced full pet refresh");
        }

        public static IEnumerator RebuildPetsAfterModeApply()
        {
            yield return null;

            ApplyCurrentVersionMode();
            SaveHelpers.RefreshCustomPetBonuses();
            SaveHelpers.RefreshActivePetInstances();

            var pm = PetManager.Instance;
            if (pm == null) yield break;

            List<string> names = new List<string>();
            for (int i = 0; i < pm.activePets.Count; i++)
            {
                if (pm.activePets[i] != null)
                    names.Add(pm.activePets[i].name);
            }

            for (int i = 0; i < pm.activePets.Count; i++)
                pm.UnSummonPetByIndex(i);

            yield return null;

            foreach (var petName in names)
            {
                var item = pm.collectedPets.Find(p => p.itemName == petName);
                if (item != null)
                    pm.SummonPet(item);
            }

            RefreshActivePetVisuals();

            pm.ReapplyAllPetBonuses();
            pm.UpdatePetBuffsDisplay();
            pm.soulCompendiumManager?.UpdateActivePetsUI();
            pm.soulCompendiumManager?.UpdateCompendiumUI();

            Debug.Log("[PokeBloobs] Rebuilt pets after version apply");
        }

        public static void ApplyCurrentVersionMode()
        {
            var mode = ModSettings.SelectedVersion;

            foreach (var soul in SoulsDatabase.LoadedSouls)
            {
                // set soul/commonImage/petAnimatorController based on mode
                //ApplySoulVisualMode(soul, mode);
            }

            Debug.Log($"[PokeBloobs] Applied version mode: {mode}");
        }

        public static void RefreshActivePetVisuals()
        {
            var pm = PetManager.Instance;
            if (pm == null) return;

            for (int i = 0; i < pm.activePets.Count; i++)
            {
                var go = pm.activePets[i];
                if (go == null) continue;

                var item = pm.collectedPets.Find(p => p.itemName == go.name);
                if (item == null) continue;

                if (go.TryGetComponent<SpriteRenderer>(out var sr))
                    sr.sprite = item.commonImage;

                if (go.TryGetComponent<Animator>(out var anim) && item.petAnimatorController != null)
                    anim.runtimeAnimatorController = item.petAnimatorController;
            }

            pm.UpdatePetBuffsDisplay();
            pm.soulCompendiumManager?.UpdateActivePetsUI();
            pm.soulCompendiumManager?.UpdateCompendiumUI();

            Debug.Log("[PokeBloobs] Refreshed active pet visuals");
        }
    }
}
