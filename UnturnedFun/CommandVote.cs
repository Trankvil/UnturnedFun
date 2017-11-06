using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;

namespace UnturnedFun {
    public class CommandVote : IRocketCommand {
        public bool AllowFromConsole {
            get {
                return false;
            }
        }

        public List<string> Permissions {
            get {
                return new List<string> {
                    "vote"
                };
            }
        }

        public string Name {
            get {
                return "vote";
            }
        }

        public string Syntax {
            get {
                return "";
            }
        }

        public AllowedCaller AllowedCaller {
            get {
                return AllowedCaller.Player;
            }
        }

        public string Help {
            get {
                return "Link URL for voted server https://unturned.FUN";
            }
        }

        public List<string> Aliases {
            get {
                return new List<string>();
            }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)caller;
            unturnedPlayer.Player.channel.send("askBrowserRequest", unturnedPlayer.CSteamID, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[] {
                Plugin.Instance.Configuration.Instance.TextURL,
                "https://unturned.fun/server/" + Plugin.Instance.Configuration.Instance.IDServer.ToString()
            });
        }
    }
}
