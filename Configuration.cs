using System.Collections.Generic;
using RFJob.Enums;
using RFJob.Models;
using Rocket.API;

namespace RFJob
{
    public class Configuration : IRocketPluginConfiguration
    {
        public bool Enabled;
        public string MessageColor;
        public string MessageIconUrl;
        public EBalance Balance;
        public bool AnnounceOnJoinJob;
        public uint CooldownAfterLeaveJob;
        public bool KickInactivePlayer;
        public uint KickInactivePlayerAfterDays;
        public HashSet<Job> Jobs;

        public void LoadDefaults()
        {
            Enabled = true;
            MessageColor = "green";
            MessageIconUrl = "https://cdn.jsdelivr.net/gh/RiceField-Plugins/UnturnedImages@images/plugin/Announcer.png";
            Balance = EBalance.EXPERIENCE;
            AnnounceOnJoinJob = true;
            CooldownAfterLeaveJob = 300;
            KickInactivePlayer = true;
            KickInactivePlayerAfterDays = 7;
            Jobs = new HashSet<Job>
            {
                new()
                {
                    Type = EJobType.PUBLIC, JobName = "Taxi", PermissionGroup = "Taxi", Permission = "job.taxi",
                    Salary = 10, SalaryIntervalInMinutes = 10, RemoveFromJobOnDisconnect = true,
                    ChatIconUrl = "https://link-to-icon.com/icon.png", MaxMembers = 10
                },
                new() {Type = EJobType.PUBLIC, JobName = "Cook", PermissionGroup = "Cook", Permission = "job.cook",
                    Salary = 10, SalaryIntervalInMinutes = 10, RemoveFromJobOnDisconnect = true,
                    ChatIconUrl = "https://link-to-icon.com/icon.png", MaxMembers = 10},
                new()
                {
                    Type = EJobType.PUBLIC, JobName = "Trader", PermissionGroup = "Trader", Permission = "job.trader",
                    Salary = 10, SalaryIntervalInMinutes = 10, RemoveFromJobOnDisconnect = true,
                    ChatIconUrl = "https://link-to-icon.com/icon.png", MaxMembers = 10
                },
                new()
                {
                    Type = EJobType.PUBLIC, JobName = "Farmer", PermissionGroup = "Farmer", Permission = "job.farmer",
                    Salary = 10, SalaryIntervalInMinutes = 10, RemoveFromJobOnDisconnect = true,
                    ChatIconUrl = "https://link-to-icon.com/icon.png", MaxMembers = 10
                },
                new()
                {
                    Type = EJobType.PRIVATE, JobName = "Military", PermissionGroup = "Military",
                    LeaderPermissionGroup = "Military Leader", Permission = "job.military",
                    Salary = 10, SalaryIntervalInMinutes = 10, RemoveFromJobOnDisconnect = false,
                    ChatIconUrl = "https://link-to-icon.com/icon.png", MaxMembers = 10
                },
                new()
                {
                    Type = EJobType.PRIVATE, JobName = "Police", PermissionGroup = "Police",
                    LeaderPermissionGroup = "Police Leader", Permission = "job.police",
                    Salary = 10, SalaryIntervalInMinutes = 10, RemoveFromJobOnDisconnect = false,
                    ChatIconUrl = "https://link-to-icon.com/icon.png", MaxMembers = 10
                },
                new()
                {
                    Type = EJobType.PRIVATE, JobName = "Special Operations", PermissionGroup = "Spec Ops",
                    LeaderPermissionGroup = "Spec Ops Leader", Permission = "job.specops",
                    Salary = 10, SalaryIntervalInMinutes = 10, RemoveFromJobOnDisconnect = false,
                    ChatIconUrl = "https://link-to-icon.com/icon.png", MaxMembers = 10
                },
                new()
                {
                    Type = EJobType.PRIVATE, JobName = "Firearms Dealer", PermissionGroup = "Firearms",
                    LeaderPermissionGroup = "Firearms Leader", Permission = "job.firearm",
                    Salary = 10, SalaryIntervalInMinutes = 10, RemoveFromJobOnDisconnect = false,
                    ChatIconUrl = "https://link-to-icon.com/icon.png", MaxMembers = 10
                }
            };
        }
    }
}