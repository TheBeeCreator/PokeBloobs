using Newtonsoft.Json;
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
    public static class SoulLoader
    {
        public static void Load()
        {
            LoadSoulsData();
            PopulateRarityArrays();
        }
        public static void LoadSoulsData()
        {
            string soulsJson = LoadSoulsJson();

            if (string.IsNullOrWhiteSpace(soulsJson))
            {
                Debug.Log("Failed to load PokeBloobsSouls.json from zip!");
                SoulsDatabase.LoadedSouls = new List<SoulsData>();
                return;
            }

            SoulsDatabase.LoadedSouls =
                JsonConvert.DeserializeObject<List<SoulsData>>(soulsJson) ?? new List<SoulsData>();

            PopulateRarityArrays();
            Debug.Log($"Successfully loaded {SoulsDatabase.LoadedSouls.Count} souls from JSON.");
        }

        public static string LoadSoulsJson()
        {
            using (Stream stream = global::PokeBloobs.Classes.ZipAssetLoader.OpenAssetStream("Data", "PokeBloobsSouls", "json"))
            {
                if (stream == null)
                    return null;

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
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
    }
}
