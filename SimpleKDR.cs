using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using Steamworks.ServerList;

namespace Oxide.Plugins
{
    [Info("SimpleKDR", "Wolfleader101", "0.1.0")]
    [Description("Display your KDR and leaderboard of kills")]
    public class SimpleKDR : CovalencePlugin
    {
        #region variables

        private DynamicConfigFile _dataFile = Interface.Oxide.DataFileSystem.GetDatafile("KDRData");
        private StoredData _storedData;

        #endregion

        #region Hooks

        private void Init()
        {
            _storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>("KDRData");
        }

        void OnPlayerConnected(BasePlayer player)
        {
            Puts($"{player.displayName}");
            PlayerKDR playerKdr = new PlayerKDR(player);
            if (_storedData.Players.Contains(playerKdr)) return;

            Interface.Oxide.DataFileSystem.WriteObject("KDRData", _storedData);
        }


        void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            if (player == null) return;
            if (info == null) return;
            if (player == info.InitiatorPlayer) return;

            // RE ENABLE LATER
           // if (player.inventory.FindItemID("rifle.ak") == null ||
           //     player.inventory.FindItemID("lmg.M249") == null) return;


            IncreaseKills(info.InitiatorPlayer);
            IncreaseDeaths(player);
        }

        #endregion
        
        #region Methods

        void IncreaseKills(BasePlayer player)
        {
            if(player == null) return;
            
            var foundPlayer = _storedData.Players.Find(item => item.name == player.displayName);
            foundPlayer.kills++;
            UpdateRatio(foundPlayer);
        }

        void IncreaseDeaths(BasePlayer player)
        {
            if(player == null) return;
            var foundPlayer = _storedData.Players.Find(item => item.name == player.displayName);
            if(foundPlayer == null) return;
            
            foundPlayer.deaths++;
            UpdateRatio(foundPlayer);
        }

        void UpdateRatio(PlayerKDR player)
        {
            float updatedRatio;
            if (player.kills == 0) return;
            if (player.deaths == 0)
            {
                player.ratio = player.kills;
            }
            else
            {
                player.ratio = (float) player.kills / player.deaths;
            }

            Interface.Oxide.DataFileSystem.WriteObject("KDRData", _storedData);
        }

        #endregion

        #region Commands

        [Command("kd")]
        private void KDCommand(IPlayer player, string command, string[] args)
        {
            //BasePlayer basePlayer = player.Object as BasePlayer;
            var foundPlayer = _storedData.Players.Find(item => item.name == player.Name);
            
            player.Reply($"<align=center><color=red><b>Your KDR is:</b></color> <color=green>{foundPlayer.ratio} </color>\n"+   
                            $"<color=green><b>Kills:</b> {foundPlayer.kills} </color>  \n"+
                            $"<color=red><b>Deaths:</b> {foundPlayer.deaths} </color> \n"
                            );
            
            
                
            if (args.Length > 0)
            {
                
            }
        }

        #endregion
        
        #region Data Class

        private class StoredData
        {
            public List<PlayerKDR> Players = new List<PlayerKDR>();
        }


        private class PlayerKDR
        {
            public string name { get; set; }
            public int kills { get; set; }
            public int deaths { get; set; }
            public float ratio { get; set; }

            public PlayerKDR()
            {
            }

            public PlayerKDR(BasePlayer player)
            {
                name = player.displayName;
                kills = 0;
                deaths = 0;
                ratio = 0;
            }
        }

        #endregion
    }
}