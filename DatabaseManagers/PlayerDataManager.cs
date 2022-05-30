using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RFJob.Models;
using RFRocketLibrary.Storages;
using Rocket.Core.Logging;

namespace RFJob.DatabaseManagers
{
    public static class PlayerDataManager
    {
        private static HashSet<PlayerData> Json_Collection { get; set; } = new();

        private const string Json_FileName = "playerdata.json";
        private static JsonDataStore<HashSet<PlayerData>> Json_DataStore { get; set; }

        public static void Initialize()
        {
            try
            {
                Json_DataStore = new JsonDataStore<HashSet<PlayerData>>(Plugin.Inst.Directory, Json_FileName);
                JSON_Reload();
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] PlayerDataManager Initializing: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static void JSON_Reload()
        {
            try
            {
                Json_Collection = Json_DataStore.Load() ?? new HashSet<PlayerData>();
                Json_DataStore.Save(Json_Collection);
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] PlayerDataManager JSON_Reload: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        public static async Task AddAsync(PlayerData playerVault)
        {
            try
            {
                var flag = Json_Collection.Contains(playerVault);
                if (flag)
                    return;

                Json_Collection.Add(playerVault);
                await Json_DataStore.SaveAsync(Json_Collection);
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] PlayerDataManager AddAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        public static HashSet<PlayerData> Get()
        {
            try
            {
                return Json_Collection;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] PlayerDataManager Get: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return null;
            }
        }

        public static PlayerData Get(ulong steamId)
        {
            try
            {
                Json_Collection.TryGetValue(new PlayerData {SteamId = steamId},
                    out var result);
                return result;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] PlayerDataManager Get: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return null;
            }
        }

        public static async Task<bool> UpdateAsync(PlayerData playerData = null)
        {
            try
            {
                return await Json_DataStore.SaveAsync(Json_Collection);
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] PlayerDataManager UpdateAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return false;
            }
        }
    }
}