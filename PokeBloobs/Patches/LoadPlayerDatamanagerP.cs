using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PokeBloobs.PokeBloobs;

namespace PokeBloobs.Patches
{
    internal class LoadPlayerDatamanagerP
    {
        //LoadPlayerData
        [HarmonyPatch(typeof(PlayerDataManager), "LoadPlayerData")]
        public class PlayerDataManager_Patch
        {
            static void Postfix(PlayerDataManager playerDataManager)
            {
                //Special checks
                if (PetManager.Instance != null)
                {

                    if (!PetManager.Instance.HasPet("BloobsDev"))
                    {
                        bool t = PetManager.Instance.HasPet("BloobsDev");
                        Debug.Log($"{t}");
                        Debug.Log("Dev Pet");
                        var item = ScriptableObject.CreateInstance<Item>();
                        ulong cur = SteamClient.SteamId;
                        Debug.Log($"{cur}");

                        if (PokeBloobs.special.ContainsKey(cur))
                        {
                            string name = special[cur];
                            SoulsData s = new SoulsData
                            {
                                soulName = "BloobsDev",
                                soulCategory = "event souls",
                                skillName = "Homesteading",
                                rarity = 5
                            };

                            item = BuildSoul(s);

                            if (item != null)
                            {
                                PetManager.Instance.AddPet(item);
                            }
                            PokeBloobs.spet = true;
                        }
                    }
                }
            }
        }
    }
}
