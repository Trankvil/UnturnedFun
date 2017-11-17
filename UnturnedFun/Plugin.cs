using System;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.API;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.IO;

namespace UnturnedFun {
    public class Plugin : RocketPlugin<Configuration> {
        public static Plugin Instance = null;
        public Database Database;
        public override TranslationList DefaultTranslations {
            get {
                return new TranslationList() {
                    { "vote_yes_reply", "Вы уже голосовали за сервер, и получили бонусные вещи." },
                    { "vote_exp", "Вы получили [{0}] опыта."},
                    { "vote_uconomy", "Вы получили [{0}] {1}"},
                    { "vote_realmoney", "Вы получили на сайт [{0}] рублей."},
                    { "vote_give_reward", "{0} за голосование получил набор \"{1}\""},
                    { "vote_not_vote", "Вы не голосовали за сервер, чтобы получить бонусные вещи."},
                    { "vote_error", "Ошибка в получении данных от мониторинга!"},
                };
            }
        }

        protected override void Load() {
            Instance = this;
            Database = new Database();
            if (IsDependencyLoaded("Uconomy")) {
                Rocket.Core.Logging.Logger.Log("Optional dependency Uconomy is present.");
            } else {
                Rocket.Core.Logging.Logger.Log("Optional dependency Uconomy is not present.");
            }
        }

        public string ApiVotePlayer(string steamid) {
            string GET = ReturnHttpGET("https://unturned.fun/api?api=" + Configuration.Instance.APIKey + "&steamid=" + steamid);
            return GET;
        }

        public string ReturnHttpGET(string URL) {
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
            //req.Timeout = 50;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }

        public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            bool isOk = true;
            if (sslPolicyErrors != SslPolicyErrors.None) {
                for (int i = 0; i < chain.ChainStatus.Length; i++) {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid) {
                            isOk = false;
                        }
                    }
                }
            }
            return isOk;
        }
    }

    public class Configuration : IRocketPluginConfiguration
    {
        public string APIKey;
        public string TextURL;
        public int IDServer;
        [XmlArray("RewardBundles")]
        [XmlArrayItem(ElementName = "RewardBundle")]
        public List<RewardBundle> RewardBundles;
        //для получения RealMoney
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public string DatabaseNameSteamID;
        public string DatabaseNameBalance;
        public int DatabasePort;

        public void LoadDefaults() {
            APIKey = "2131sr213f2134d23344s23";
            TextURL = "Уважаемый игрок, для того чтобы проголосовать за сервер, нажмите на кнопку Accept!";
            IDServer = 1;
            RewardBundles = new List<RewardBundle>() {
                new RewardBundle() { Name="Survival", Exp = 100, Rewards = new List<Reward>() { new Reward(245, 1), new Reward(81, 2), new Reward(16, 1) }, Probability = 33 },
                new RewardBundle() { Name="Brute Force", Exp = 150, Uconomy = 1000, Rewards = new List<Reward>() { new Reward(112, 1), new Reward(113, 3), new Reward(254, 3) }, Probability = 33 },
                new RewardBundle() { Name="Watcher", Exp = 150, Uconomy = 1000, RealMoney = 5, Rewards = new List<Reward>() { new Reward(109, 1), new Reward(111, 3), new Reward(236, 1) }, Probability = 33 }
            };
            DatabaseAddress = "localhost";
            DatabaseUsername = "unturned";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabaseTableName = "users";
            DatabaseNameSteamID = "steamId";
            DatabaseNameBalance = "balance";
            DatabasePort = 3306;
        }
    }

    public class RewardBundle  {
        public RewardBundle() { }

        public int Probability;
        public string Name;
        public ushort Exp;
        public int Uconomy;
        public int RealMoney;

        [XmlArrayItem(ElementName = "Reward")]
        public List<Reward> Rewards;
    }

    public class Reward {
        public Reward() { }

        public Reward(ushort itemId, byte amount) {
            ItemId = itemId;
            Amount = amount;
        }

        [XmlAttribute("itemid")]
        public ushort ItemId;

        [XmlAttribute("amount")]
        public byte Amount;
    }
}