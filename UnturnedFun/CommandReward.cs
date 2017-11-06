using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using Rocket.Core.Logging;
using System.Linq;
using fr34kyn01535.Uconomy;

namespace UnturnedFun {
    public class CommandReward : IRocketCommand {
        //Dictionary<CSteamID, DateTime> coolDown = new Dictionary<CSteamID, DateTime>();

        public string Name {
            get { return "reward"; }
        }

        public string Help {
            get { return "Added reward items for voted."; }
        }


        public AllowedCaller AllowedCaller {
            get {
                return AllowedCaller.Player;
            }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public List<string> Permissions {
            get {
                return new List<string>() { "reward" };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            string answer = Plugin.Instance.ApiVotePlayer(player.CSteamID.ToString());
            switch (answer) {
                case "0":
                    UnturnedChat.Say(player, "Вы уже голосовали за сервер, и получили бонусные вещи.");
                    break;
                case "1":
                    int propabilysum = Plugin.Instance.Configuration.Instance.RewardBundles.Sum(p => p.Probability);
                    RewardBundle bundle = new RewardBundle();
                    if (propabilysum != 0) {
                        Random r = new Random();
                        int i = 0, diceRoll = r.Next(0, propabilysum);
                        foreach (RewardBundle b in Plugin.Instance.Configuration.Instance.RewardBundles) {
                            if (diceRoll > i && diceRoll <= i + b.Probability) {
                                bundle = b;
                                break;
                            }
                            i = i + b.Probability;
                        }
                    } else {
                        Logger.Log("Rewards bundles not found!");
                        return;
                    }
                    if (bundle.Exp != 0) {
                        player.Experience += bundle.Exp;
                        UnturnedChat.Say(player, "Вы получили ["+bundle.Exp+"] опыта.");
                    }
                    Plugin.ExecuteDependencyCode("Uconomy", (IRocketPlugin plugin) =>
                    {
                        if (bundle.Uconomy != 0)
                        {   
                            Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), Convert.ToDecimal(bundle.Uconomy));
                        //ASPUnturnedMySQL.Plugin.Instance.IncreaseBalance(player.CSteamID.ToString(), Convert.ToDecimal(bundle.Uconomy));
                        UnturnedChat.Say(player, "Вы получили [" + bundle.Exp + "] " + Uconomy.Instance.Configuration.Instance.MoneyName);
                        }
                    });
                    if (bundle.RealMoney != 0) {
                        if(Plugin.Instance.Database.IsSiteSteamID(player.CSteamID.ToString())) {
                            Plugin.Instance.Database.AddRealMoneySteamID(player.CSteamID.ToString(), bundle.RealMoney);
                            UnturnedChat.Say(player, "Вы получили на сайт ["+bundle.RealMoney+"] рублей.");
                        }
                    }
                    foreach (Reward reward in bundle.Rewards) {
                        if (!player.GiveItem(reward.ItemId, reward.Amount)) {
                            Logger.Log("error add item "+ reward.ItemId + " to " + player.CSteamID.ToString());
                        }
                    }
                    UnturnedChat.Say(player.DisplayName + " за голосование получил набор \""+ bundle.Name + "\"");
                    break;
                case "2":
                    UnturnedChat.Say(player, "Вы не голосовали за сервер, чтобы получить бонусные вещи.");
                    break;
                default:
                    UnturnedChat.Say(player, "Ошибка в получении данных от мониторинга!", UnityEngine.Color.red);
                    break;
            }
        }
    }
}