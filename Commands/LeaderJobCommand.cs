using System;
using System.Collections.Generic;
using System.Linq;
using RFJob.Enums;
using RFJob.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace RFJob.Commands
{
    public class LeaderJobCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "leaderjob";
        public string Help => "Leader job commands";
        public string Syntax => "/leaderjob <accept|kick> <playerName|playerId>";
        public List<string> Aliases => new();
        public List<string> Permissions => new() {"ljob"};

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

            var job = JobUtil.GetJob(caller);
            if (job == null)
            {
                Plugin.Say(caller, Plugin.TranslateRich(EResponse.NO_JOB.ToString()));
                return;
            }

            if (!JobUtil.IsLeader(caller, job))
            {
                Plugin.Say(caller, Plugin.TranslateRich(EResponse.NOT_LEADER.ToString()));
                return;
            }

            IRocketPlayer uPlayer = null;
            if (ulong.TryParse(command.ElementAtOrDefault(1), out var playerId))
            {
                var player = PlayerTool.getPlayer(new CSteamID(playerId));
                if (player != null)
                    uPlayer = UnturnedPlayer.FromPlayer(player);
            }
            else
            {
                var player = PlayerTool.getPlayer(string.Join(" ", command.Skip(1)));
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
                case EJobCommandOption.Accept:
                    if (!Plugin.Inst.PrivateJobRequest[job].Contains(ulong.Parse(uPlayer.Id)))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.PLAYER_NOT_APPLY.ToString()));
                        return;
                    }

                    PermissionUtil.AddPlayerToGroup(uPlayer, job.PermissionGroup);
                    JobUtil.SalaryStart(uPlayer as UnturnedPlayer, job);
                    Plugin.Say(caller,
                        Plugin.TranslateRich(EResponse.JOB_ACCEPT_SUCCESS.ToString(),
                            string.IsNullOrWhiteSpace(uPlayer.DisplayName) ? uPlayer.Id : uPlayer.DisplayName));
                    break;
                case EJobCommandOption.Kick:
                    if (!PermissionUtil.HasGroup(uPlayer, job.PermissionGroup))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.PLAYER_NOT_MEMBER.ToString(), job.JobName));
                        return;
                    }

                    JobUtil.KickFromJob(uPlayer);
                    JobUtil.SalaryStop(caller as UnturnedPlayer);
                    Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_KICK_SUCCESS.ToString(), job.JobName));
                    break;
            }

            return;

            Invalid_Parameter:
            Plugin.Say(caller, Plugin.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax));
        }
    }
}