using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace RFJob.Utils
{
    public static class PermissionUtil
    {
        public static void AddPlayerToGroup(IRocketPlayer rPlayer, string groupId)
        {
            R.Permissions.AddPlayerToGroup(groupId, rPlayer);
        }
        
        public static void RemovePlayerFromGroup(IRocketPlayer rPlayer, string groupId)
        {
            R.Permissions.RemovePlayerFromGroup(groupId, rPlayer);
        }
        
        public static bool HasGroup(IRocketPlayer rPlayer, string groupId)
        {
            var group = R.Permissions.GetGroup(groupId);
            if (group == null)
                return false;
            
            return group.Members.Any(x => x == rPlayer.Id);
        }

        public static void ClearGroup(string groupId)
        {
            var group = R.Permissions.GetGroup(groupId);
            if (group == null)
                return;
            
            var members = new List<string>();
            members.AddRange(group.Members);
            foreach (var member in members)
            {
                R.Permissions.RemovePlayerFromGroup(groupId, new RocketPlayer(member));
                var player = PlayerTool.getPlayer(new CSteamID(ulong.Parse(member)));
                if (player != null)
                {
                    JobUtil.SalaryStop(UnturnedPlayer.FromPlayer(player));
                }
            }
        }
    }
}