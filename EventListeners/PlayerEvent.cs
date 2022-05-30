using System;
using System.Threading.Tasks;
using RFJob.DatabaseManagers;
using RFJob.Models;
using RFJob.Utils;
using Rocket.Unturned.Player;

namespace RFJob.EventListeners
{
    internal static class PlayerEvent
    {
        internal static void OnConnected(UnturnedPlayer player)
        {
            var playerData = PlayerDataManager.Get(player.CSteamID.m_SteamID);
            if (playerData == null)
                Task.Run(async () =>
                    await PlayerDataManager.AddAsync(new PlayerData {SteamId = player.CSteamID.m_SteamID}));
            else
            {
                playerData.LastActive = null;
                Task.Run(async () => await PlayerDataManager.UpdateAsync(playerData));
            }
        }
        internal static void OnDisconnected(UnturnedPlayer player)
        {
            if (Plugin.Inst.JoinJobCooldown.ContainsKey(player.CSteamID.m_SteamID))
                Plugin.Inst.JoinJobCooldown.Remove(player.CSteamID.m_SteamID);
            var playerData = PlayerDataManager.Get(player.CSteamID.m_SteamID);
            playerData.LastActive = DateTime.Now;
            Task.Run(async () => await PlayerDataManager.UpdateAsync(playerData));
        }

        internal static void OnPlayerChat(UnturnedPlayer player, ref string iconurl)
        {
            var job = JobUtil.GetJob(player);
            if (job == null)
                return;

            iconurl = job.ChatIconUrl;
        }
    }
}