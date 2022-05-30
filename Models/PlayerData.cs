using System;

namespace RFJob.Models
{
    public class PlayerData
    {
        public ulong SteamId { get; set; }
        public DateTime? LastActive { get; set; }
        public PlayerData()
        {
            
        }

        public override bool Equals(object obj)
        {
            if (obj is not PlayerData playerData)
                return false;

            return playerData.SteamId == SteamId;
        }

        public override int GetHashCode()
        {
            return SteamId.GetHashCode();
        }
    }
}