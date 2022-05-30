using System;
using System.Collections.Generic;
using System.Linq;
using RFJob.Enums;
using RFJob.Models;
using RFJob.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace RFJob.Commands
{
    public class AdminJobCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "adminjob";
        public string Help => "Admin job commands";
        public string Syntax => "/adminjob <add|remove|clear|fire>";
        public List<string> Aliases => new();
        public List<string> Permissions => new() {"ajob"};

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                goto Invalid_Parameter;
            }

            var flag = Enum.TryParse<EJobCommandOption>(command.ElementAtOrDefault(0), true, out var opt);
            if (!flag)
            {
                goto Invalid_Parameter;
            }

            if (!Plugin.Conf.Jobs.TryGetValue(new Job {JobName = command[1]}, out var job))
            {
                Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_NOT_FOUND.ToString()));
                return;
            }

            if (opt == EJobCommandOption.Clear)
            {
                PermissionUtil.ClearGroup(job.PermissionGroup);
                if (!string.IsNullOrWhiteSpace(job.LeaderPermissionGroup))
                    PermissionUtil.ClearGroup(job.LeaderPermissionGroup);
                Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_CLEAR_SUCCESS.ToString(), job.JobName));
                return;
            }

            var flag2 = opt == EJobCommandOption.Fire;
            IRocketPlayer uPlayer = null;
            if (ulong.TryParse(command.ElementAtOrDefault(flag2 ? 1 : 2), out var playerId))
            {
                var player = PlayerTool.getPlayer(new CSteamID(playerId));
                if (player != null)
                    uPlayer = UnturnedPlayer.FromPlayer(player);
            }
            else
            {
                var player = PlayerTool.getPlayer(string.Join(" ", command.Skip(flag2 ? 1 : 2)));
                if (player != null)
                    uPlayer = UnturnedPlayer.FromPlayer(player);
            }

            if (uPlayer == null && playerId == 0)
            {
                Plugin.Say(caller, Plugin.TranslateRich(EResponse.PLAYER_NOT_FOUND.ToString()));
                return;
            }

            if (uPlayer == null && playerId != 0)
            {
                uPlayer = new RocketPlayer(playerId.ToString());
            }

            switch (opt)
            {
                case EJobCommandOption.Add:
                    if (command.Length == 1)
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), "/adminjob add <jobName> <playerName|playerId>"));
                        return;
                    }
                
                    if (JobUtil.HasJob(uPlayer))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.PLAYER_HAS_JOB.ToString()));
                        return;
                    }

                    if (PermissionUtil.HasGroup(uPlayer, job.PermissionGroup))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.PLAYER_IS_MEMBER.ToString(), job.JobName));
                        return;
                    }

                    PermissionUtil.AddPlayerToGroup(uPlayer, job.PermissionGroup);
                    JobUtil.SalaryStart(uPlayer as UnturnedPlayer, job);
                    Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_ADD_SUCCESS.ToString(),
                        string.IsNullOrWhiteSpace(uPlayer.DisplayName) ? uPlayer.Id : uPlayer.DisplayName,
                        job.JobName));
                    break;
                case EJobCommandOption.Remove:
                    if (command.Length == 1)
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), "/adminjob remove <jobName> <playerName|playerId>"));
                        return;
                    }
                    
                    if (!PermissionUtil.HasGroup(uPlayer, job.PermissionGroup))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.PLAYER_NOT_MEMBER.ToString(), job.JobName));
                        return;
                    }

                    PermissionUtil.RemovePlayerFromGroup(uPlayer, job.PermissionGroup);
                    JobUtil.SalaryStop(uPlayer as UnturnedPlayer);
                    Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_REMOVE_SUCCESS.ToString(),
                        string.IsNullOrWhiteSpace(uPlayer.DisplayName) ? uPlayer.Id : uPlayer.DisplayName,
                        job.JobName));
                    break;
                case EJobCommandOption.Fire:
                    if (command.Length == 1)
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), "/adminjob fire <playerName|playerId>"));
                        return;
                    }
                    
                    if (!JobUtil.HasJob(uPlayer))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.PLAYER_NO_JOB.ToString()));
                        return;
                    }

                    JobUtil.KickFromJob(uPlayer);
                    JobUtil.SalaryStop(uPlayer as UnturnedPlayer);
                    Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_FIRE_SUCCESS.ToString(),
                        string.IsNullOrWhiteSpace(uPlayer.DisplayName) ? uPlayer.Id : uPlayer.DisplayName));
                    break;
            }

            return;

            Invalid_Parameter:
            Plugin.Say(caller, Plugin.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax));
        }
    }
}