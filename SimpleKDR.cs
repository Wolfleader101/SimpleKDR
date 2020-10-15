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
            timer.Every(60f, () =>
            {
                _storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>("KDRData");
            });
        }

        private void OnServerSave()
        {
            Interface.Oxide.DataFileSystem.WriteObject("KDRData", _storedData);
        }

        private void Unload()
        {
            Interface.Oxide.DataFileSystem.WriteObject("KDRData", _storedData);
        }
        


        void OnPlayerConnected(BasePlayer player)
        {
            Puts($"{player.displayName}");
            PlayerKDR playerKdr = new PlayerKDR(player);
            if (_storedData.Players.Contains(playerKdr)) return;
            _storedData.Players.Add(playerKdr);
        }


        void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            if (player == null) return;
            if (info == null) return;
            if (player == info.InitiatorPlayer) return;

            // RE ENABLE LATER
             if (player.inventory.FindItemID("rifle.ak") == null ||
                 player.inventory.FindItemID("lmg.M249") == null) return;


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
            
        }

        #endregion

        #region Commands

        [Command("kd")]
        private void KDCommand(IPlayer player, string command, string[] args)
        {
            var foundPlayer = _storedData.Players.Find(item => item.name == player.Name);
            if (foundPlayer == null)
            {
                 player.Reply("You don't have a KD yet");
                 return;
            }

            string RatioText = foundPlayer.ratio >= 1
                ? $"<color=green>{foundPlayer.ratio} </color>"
                : $"<color=red>{foundPlayer.ratio} </color>";
            player.Reply(
                $"<align=center><color=orange><b>Your KDR is:</b></color> {RatioText}\n" +
                $"<color=orange><b>Kills:</b><color=green> {foundPlayer.kills} </color>  \n" +
                $"<color=orange><b>Deaths:</b><color=red> {foundPlayer.deaths} </color> \n"
            );


            if (args.Length > 0)
            {
            }
        }

        [Command("top")]
        private void TopCommand(IPlayer player, string commmand, string[] args)
        {
            var topPlayers = _storedData.Players.OrderByDescending(item => item.kills).Take(10).ToList();

            BasePlayer basePlayer = player.Object as BasePlayer;
            
            var foundPlayer = _storedData.Players.Find(item => item.name == player.Name);

            var container = new CuiElementContainer();
            var mainName = container.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.39 0.39 0.39 0.75"
                },
                RectTransform =
                {
                    AnchorMin = "0 0.5",
                    AnchorMax = "0.0 0.5",
                    OffsetMin = "25 -200",
                    OffsetMax = "500 350"
                },
                CursorEnabled = true
            }, "Overlay", "BackgroundName");

            var RankLabel = new CuiLabel
            {
                Text =
                {
                    Text = "Rank",
                    FontSize = 18,
                    Align = TextAnchor.MiddleCenter
                },
                RectTransform =
                {
                    AnchorMin = "0 0.9",
                    AnchorMax = "0.2 1",
                }
            };
            container.Add(RankLabel, mainName);

            var NameLabel = new CuiLabel
            {
                Text =
                {
                    Text = "Name",
                    FontSize = 18,
                    Align = TextAnchor.MiddleCenter
                },
                RectTransform =
                {
                    AnchorMin = "0.2 0.9",
                    AnchorMax = "0.4 1",
                }
            };
            container.Add(NameLabel, mainName);

            var KillsLabel = new CuiLabel
            {
                Text =
                {
                    Text = "Kills",
                    FontSize = 18,
                    Align = TextAnchor.MiddleCenter
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.9",
                    AnchorMax = "0.6 1",
                }
            };
            container.Add(KillsLabel, mainName);

            var DeathsLabel = new CuiLabel
            {
                Text =
                {
                    Text = "Deaths",
                    FontSize = 18,
                    Align = TextAnchor.MiddleCenter
                },
                RectTransform =
                {
                    AnchorMin = "0.6 0.9",
                    AnchorMax = "0.8 1",
                }
            };
            container.Add(DeathsLabel, mainName);

            var RatioLabel = new CuiLabel
            {
                Text =
                {
                    Text = "Ratio",
                    FontSize = 18,
                    Align = TextAnchor.MiddleCenter
                },
                RectTransform =
                {
                    AnchorMin = "0.8 0.9",
                    AnchorMax = "0.9 1",
                    OffsetMin = "-50 -50",
                    OffsetMax = "50 50"
                }
            };
            container.Add(RatioLabel, mainName);


            for (int i = 0; i < topPlayers.Count; i++)
            {
                float size = 0.03f;
                float n = 10;
                float borderOffset = 0.1f;
                float sizeLeft = 1f - size * n - borderOffset * 2;
                float gap = sizeLeft / ((n * 2) - 1);

                string TopPlayerColor = "1 0.78 0 1";
                string BackgroundDarkColor = "0.39 0.39 0.39 1";
                var backgroundDark = new CuiPanel
                {
                    Image =
                    {
                        Color = i == 0 ? TopPlayerColor : BackgroundDarkColor
                    },
                    RectTransform =
                    {
                        AnchorMin = $"0 {1 - (size * (i + 1) + i * gap + borderOffset)}",
                        AnchorMax = $"1 {1 - (i * size + i * gap + borderOffset)}",
                    },
                };
                if (i % 2 == 0)
                    container.Add(backgroundDark, mainName);
                var PlayerRank = new CuiLabel
                {
                    Text =
                    {
                        Text = $"{i + 1}",
                        FontSize = 14,
                        Align = TextAnchor.MiddleCenter
                    },
                    RectTransform =
                    {
                        AnchorMin =
                            $"0 {1 - (size * (i + 1) + i * gap + borderOffset)}", // 1 - (size * (i + 1) + i * gap + borderOffset)
                        AnchorMax =
                            $"0.2 {1 - (i * size + i * gap + borderOffset)}", //1 - (i * size + i * gap + borderOffset)
                    }
                };
                container.Add(PlayerRank, mainName);
                var PlayerName = new CuiLabel
                {
                    Text =
                    {
                        Text = $"{topPlayers[i].name}",
                        FontSize = 14,
                        Align = TextAnchor.MiddleCenter
                    },
                    RectTransform =
                    {
                        AnchorMin = $"0.2 {1 - (size * (i + 1) + i * gap + borderOffset)}",
                        AnchorMax = $"0.4 {1 - (i * size + i * gap + borderOffset)}",
                    }
                };
                container.Add(PlayerName, mainName);
                var PlayerKills = new CuiLabel
                {
                    Text =
                    {
                        Text = $"{topPlayers[i].kills}",
                        FontSize = 14,
                        Align = TextAnchor.MiddleCenter
                    },
                    RectTransform =
                    {
                        AnchorMin = $"0.4 {1 - (size * (i + 1) + i * gap + borderOffset)}",
                        AnchorMax = $"0.6 {1 - (i * size + i * gap + borderOffset)}",
                    }
                };
                container.Add(PlayerKills, mainName);
                var PlayerDeaths = new CuiLabel
                {
                    Text =
                    {
                        Text = $"{topPlayers[i].deaths}",
                        FontSize = 14,
                        Align = TextAnchor.MiddleCenter
                    },
                    RectTransform =
                    {
                        AnchorMin = $"0.6 {1 - (size * (i + 1) + i * gap + borderOffset)}",
                        AnchorMax = $"0.8 {1 - (i * size + i * gap + borderOffset)}",
                    }
                };
                container.Add(PlayerDeaths, mainName);

                var PlayerRatio = new CuiLabel
                {
                    Text =
                    {
                        Text = $"{topPlayers[i].ratio}",
                        FontSize = 14,
                        Align = TextAnchor.MiddleCenter
                    },
                    RectTransform =
                    {
                        AnchorMin = $"0.8 {1 - (size * (i + 1) + i * gap + borderOffset)}",
                        AnchorMax = $"0.9 {1 - (i * size + i * gap + borderOffset)}",
                    }
                };
                container.Add(PlayerRatio, mainName);
            }

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
            
            var UsersKills = new CuiLabel
            {
                Text =
                {
                    Text = $"Your Kills: {foundPlayer.kills}",
                    FontSize = 14,
                    Align = TextAnchor.MiddleCenter,
                    Color = "0 1 0 1"
                },
                RectTransform =
                {
                    AnchorMin = $"0 0",
                    AnchorMax = $"0.3 0.2",
                }
            };

            var UsersDeaths = new CuiLabel
            {
                Text =
                {
                    Text = $"Your Deaths: {foundPlayer.deaths}",
                    FontSize = 14,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 0 0 1"
                },
                RectTransform =
                {
                    AnchorMin = $"0.3 0",
                    AnchorMax = $"0.6 0.2",
                }
            };
           
            string userRatioColor = foundPlayer.ratio >= 1 ? "0 1 0 1" : "1 0 0 1";
            var UsersRatio = new CuiLabel
            {
                Text =
                {
                    Text = $"Your KDR: {foundPlayer.ratio}",
                    FontSize = 14,
                    Align = TextAnchor.MiddleCenter,
                    Color = userRatioColor
                },
                RectTransform =
                {
                    AnchorMin = $"0.6 0",
                    AnchorMax = $"0.9 0.2",
                }
            };
            
            if (foundPlayer != null)
            {
                container.Add(UsersKills, mainName);
                container.Add(UsersDeaths, mainName);
                container.Add(UsersRatio, mainName);
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