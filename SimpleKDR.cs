using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Cui;
using Steamworks.ServerList;
using UnityEngine;

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
            if (player == null) return;

            var foundPlayer = _storedData.Players.Find(item => item.name == player.displayName);
            foundPlayer.kills++;
            UpdateRatio(foundPlayer);
        }

        void IncreaseDeaths(BasePlayer player)
        {
            if (player == null) return;
            var foundPlayer = _storedData.Players.Find(item => item.name == player.displayName);
            if (foundPlayer == null) return;

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
            var foundPlayer = _storedData.Players.Find(item => item.name == player.Name);

            player.Reply(
                $"<align=center><color=red><b>Your KDR is:</b></color> <color=green>{foundPlayer.ratio} </color>\n" +
                $"<color=green><b>Kills:</b> {foundPlayer.kills} </color>  \n" +
                $"<color=red><b>Deaths:</b> {foundPlayer.deaths} </color> \n"
            );


            if (args.Length > 0)
            {
            }
        }

        [Command("top")]
        private void TopCommand(IPlayer player, string commmand, string[] args)
        {
            var topPlayers = _storedData.Players.OrderByDescending(item => item.kills).Take(10).ToList();
            //Puts($"{topPlayers[0].kills}");

            BasePlayer basePlayer = player.Object as BasePlayer;
            var container = new CuiElementContainer();
            var mainName = container.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.39 0.39 0.39 0.75"
                },
                RectTransform =
                {
                    AnchorMin = "0.05 0.4",
                    AnchorMax = "0.6 0.85",
                    OffsetMin = "-50 -50",
                    OffsetMax = "50 75"
                },
                CursorEnabled = true
            }, "Overlay", "BackgroundName");
            var closeButton = new CuiButton
            {
                Button =
                {
                    Close = mainName,
                    Color = "1 0 0 0.78"
                },
                RectTransform =
                {
                    AnchorMin = "0.930 0.940",
                    AnchorMax = "1 1",
                    OffsetMin = "-1 -1",
                    OffsetMax = "1 1"
                },
                Text =
                {
                    Text = "X",
                    FontSize = 14,
                    Align = TextAnchor.MiddleCenter
                }
            };
            container.Add(closeButton, mainName);

            int i = 1;
            foreach (var topPlayer in topPlayers)
            {
                var playerItem = new CuiLabel
                {
                    Text =
                    {
                        Text = $"{i} {topPlayer.name}",
                        FontSize = 18,
                        Align = TextAnchor.MiddleCenter
                    },
                    RectTransform =
                    {
                        AnchorMin = $"0.06 {1 - 0.07 * i + 0.006}",
                        AnchorMax = $"0.75 {1 - 0.07 * (i - 1)}"
                    }
                };
                container.Add(playerItem, mainName);
                i++;
            }

            CuiHelper.AddUi(basePlayer, container);
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