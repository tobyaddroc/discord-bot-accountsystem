using DiscordBotAccountSystem.Auth.Prototypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DiscordBotAccountSystem.Auth
{
    public class CDataBase
    {
        public string DataBaseFileName { get; set; }
        public class CPGroups
        {
            [JsonPropertyName("admin")]
            public CPPerms Administrator { get; set; }

            [JsonPropertyName("premium")]
            public CPPerms Premium { get; set; }

            [JsonPropertyName("user")]
            public CPPerms User { get; set; }
        }
        public async Task<List<CPAccount>> GetUserList()
        {
            string rawdb = File.ReadAllText(DataBaseFileName);
            CPDB db = JsonSerializer.Deserialize<CPDB>(rawdb);
            return new List<CPAccount>(db.Accounts);
        }
        public async Task<CPAccount[]> GetUserArray()
        {
            string rawdb = File.ReadAllText(DataBaseFileName);
            CPDB db = JsonSerializer.Deserialize<CPDB>(rawdb);
            return db.Accounts;
        }
        public CPDB GetDatabase()
        {
            string rawdb = File.ReadAllText(DataBaseFileName);
            CPDB db = JsonSerializer.Deserialize<CPDB>(rawdb);
            return db;
        }
        public async Task<CPAccount> FindProfile(string Username)
        {
            CPDB db = GetDatabase();
            foreach (CPAccount profile in db.Accounts)
                if (profile.Username == Username)
                    return profile;
            return null;
        }
        public async Task<CPAccount> FindProfile(ulong Id)
        {
            CPDB db = GetDatabase();
            foreach (CPAccount profile in db.Accounts)
                if (profile.Id == Id)
                    return profile;
            return null;
        }
        public async Task CreateProfile(CPAccount Profile)
        {
            List<CPAccount> userlist = await GetUserList();
            CPDB db = GetDatabase();
            userlist.Add(Profile);
            db.Accounts = userlist.ToArray();
            File.WriteAllText(DataBaseFileName, JsonSerializer.Serialize(db));
            return;
        }
        public async Task<bool> ChangeGroup(ulong Id, ulong LocalId, string NewGroup)
        {
            CPDB db = GetDatabase();
            if (Id != LocalId)
            {
                for (int i = 0; i < db.Accounts.Length; i++)
                {
                    if (db.Accounts[i].Id == Id && !db.Accounts[i].Immunity)
                    {
                        db.Accounts[i].Group = NewGroup;
                        File.WriteAllText(DataBaseFileName, JsonSerializer.Serialize(db));
                        return true;
                    }
                }
            }
            return false;
        }
        public async Task<bool> ChangeGroup(string Username, string Localusername, string NewGroup)
        {
            CPDB db = GetDatabase();
            if (Username != Localusername)
            {
                for (int i = 0; i < db.Accounts.Length; i++)
                {
                    if (db.Accounts[i].Username == Username && !db.Accounts[i].Immunity)
                    {
                        db.Accounts[i].Group = NewGroup;
                        File.WriteAllText(DataBaseFileName, JsonSerializer.Serialize(db));
                        return true;
                    }
                }
            }
            return false;
        }
        public async Task<bool> BanProfile(string Username, string Localusername, ulong Bantime = 0, string Reason = "No reason")
        {
            CPDB db = GetDatabase();
            if (Username != Localusername)
            {
                for (int i = 0; i < db.Accounts.Length; i++)
                {
                    if (db.Accounts[i].Username == Username)
                    {
                        if (!db.Accounts[i].Immunity)
                        {
                            db.Accounts[i].BanInfo = new CPBan()
                            {
                                BannedAt = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(),
                                UnbannedAt = Bantime == 0 ? 2473986640 : (ulong)DateTimeOffset.Now.ToUnixTimeSeconds() + Bantime,
                                Reason = Reason
                            };
                            File.WriteAllText(DataBaseFileName, JsonSerializer.Serialize(db));
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }
        public async Task<bool> EditProfile(string Username, CPProfile Profile)
        {
            CPDB db = GetDatabase();
            for (int i = 0; i < db.Accounts.Length; i++)
            {
                if (db.Accounts[i].Username == Username)
                {
                    db.Accounts[i].Profile = Profile;
                    File.WriteAllText(DataBaseFileName, JsonSerializer.Serialize(db));
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> EditProfile(ulong Id, CPProfile Profile)
        {
            CPDB db = GetDatabase();
            for (int i = 0; i < db.Accounts.Length; i++)
            {
                if (db.Accounts[i].Id == Id)
                {
                    db.Accounts[i].Profile = Profile;
                    File.WriteAllText(DataBaseFileName, JsonSerializer.Serialize(db));
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> BanProfile(ulong Id, ulong LocalId, ulong Bantime = 0, string Reason = "No reason")
        {
            CPDB db = GetDatabase();
            if (Id != LocalId)
            {
                for (int i = 0; i < db.Accounts.Length; i++)
                {
                    if (db.Accounts[i].Id == Id)
                    {
                        if (!db.Accounts[i].Immunity)
                        {
                            db.Accounts[i].BanInfo = new CPBan()
                            {
                                BannedAt = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(),
                                UnbannedAt = Bantime == 0 ? 2473986640 : (ulong)DateTimeOffset.Now.ToUnixTimeSeconds() + Bantime,
                                Reason = Reason
                            };
                            File.WriteAllText(DataBaseFileName, JsonSerializer.Serialize(db));
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }
        public async Task<bool> UnbanProfile(string Username)
        {
            CPDB db = GetDatabase();
            for (int i = 0; i < db.Accounts.Length; i++)
            {
                if (db.Accounts[i].Username == Username && db.Accounts[i].BanInfo != null)
                {
                    db.Accounts[i].BanInfo = null;
                    File.WriteAllText(DataBaseFileName, JsonSerializer.Serialize(db));
                    return true;
                }
                    
            }
            return false;
        }
        public async Task<bool> UnbanProfile(ulong Id)
        {
            CPDB db = GetDatabase();
            for (int i = 0; i < db.Accounts.Length; i++)
            {
                if (db.Accounts[i].Id == Id && db.Accounts[i].BanInfo != null)
                {
                    db.Accounts[i].BanInfo = null;
                    File.WriteAllText(DataBaseFileName, JsonSerializer.Serialize(db));
                    return true;
                }
            }
            return false;
        }
        public CPGroups GetGroups() => JsonSerializer.Deserialize<CPGroups>(File.ReadAllText("groups.json"));
        public async Task<CPPerms> FetchPermissions(string Username)
        {
            List<CPAccount> accounts = await GetUserList();
            CPGroups groups = GetGroups();
            foreach (CPAccount acc in accounts)
            {
                if (acc.Username == Username)
                {
                    switch (acc.Group)
                    {
                        case "admin":
                            return groups.Administrator;
                        case "premium":
                            return groups.Premium;
                        case "user":
                            return groups.User;
                        default:
                            return null;
                    }
                }
            }
            return null;
        }
        public async Task<CPPerms> FetchPermissions(ulong Id)
        {
            List<CPAccount> accounts = await GetUserList();
            CPGroups groups = GetGroups();
            foreach (CPAccount acc in accounts)
            {
                if (acc.Id == Id)
                {
                    switch (acc.Group)
                    {
                        case "admin":
                            return groups.Administrator;
                        case "premium":
                            return groups.Premium;
                        case "user":
                            return groups.User;
                        default:
                            return null;
                    }
                }
            }
            return null;
        }

        public CDataBase(string dbName)
        {
            DataBaseFileName = dbName;
        }
    }
}
