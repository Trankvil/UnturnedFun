using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using System;
using System.Text.RegularExpressions;

namespace UnturnedFun
{
    public class Database {
        
        public Database() {
            new I18N.West.CP1250();
        }

        private MySqlConnection createConnection() {
            MySqlConnection connection = null;
            try
            {
                if (Plugin.Instance.Configuration.Instance.DatabasePort == 0) Plugin.Instance.Configuration.Instance.DatabasePort = 3306;
                connection = new MySqlConnection(String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", Plugin.Instance.Configuration.Instance.DatabaseAddress, Plugin.Instance.Configuration.Instance.DatabaseName, Plugin.Instance.Configuration.Instance.DatabaseUsername, Plugin.Instance.Configuration.Instance.DatabasePassword, Plugin.Instance.Configuration.Instance.DatabasePort));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return connection;
        }

        public bool IsSiteSteamID(string steamId) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                string table = Plugin.Instance.Configuration.Instance.DatabaseTableName;
                string steamid = Plugin.Instance.Configuration.Instance.DatabaseNameSteamID;
                command.CommandText = "select 1 from `" + Plugin.Instance.Configuration.Instance.DatabaseTableName + "` where `"+ steamid + "` = '" + steamId + "';";
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null) return true;
                connection.Close();
            } catch (Exception ex) {
                Logger.LogException(ex);
            }
            return false;
        }

        public void AddRealMoneySteamID(string steamId, int money) {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            string table = Plugin.Instance.Configuration.Instance.DatabaseTableName;
            string steamid = Plugin.Instance.Configuration.Instance.DatabaseNameSteamID;
            string balance = Plugin.Instance.Configuration.Instance.DatabaseNameBalance;
            command.CommandText = "update `" + table + "` set `"+ balance + "` = " + balance + " + (" + money.ToString() + ") where `"+ steamid + "` = '" + steamId + "';";
            connection.Open();
            command.ExecuteScalar();
            connection.Close();
        }

    }
}