using System;
using System.Collections.Generic;
using System.Linq;
using RFJob.Enums;
using RFJob.Models;
using RFJob.Utils;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFJob.Commands
{
    public class JobCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "job";
        public string Help => "Public job commands";
        public string Syntax => "/job <join|apply|list|leave>";
        public List<string> Aliases => new();
        public List<string> Permissions => new() {"job"};

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                goto Invalid_Parameter;
            }

            var flag = Enum.TryParse<EJobCommandOption>(command[0], true, out var opt);
            if (!flag)
            {
                goto Invalid_Parameter;
            }

            switch (opt)
            {
                case EJobCommandOption.Join:
                    if (caller is ConsolePlayer)
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.CONSOLE_NOT_ALLOWED.ToString()));
                        return;
                    }

                    if (command.Length == 1)
                    {
                        Plugin.Say(caller,
                            Plugin.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), "/job join <jobName>"));
                        return;
                    }

                    if (JobUtil.HasJob(caller))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.HAS_JOB.ToString()));
                        return;
                    }

                    var jobName = string.Join(" ", command.Skip(1));
                    var flag2 = Plugin.Inst.PublicJobs.TryGetValue(new Job {JobName = jobName},
                        out var requestedJob);
                    if (!flag2)
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_NOT_FOUND.ToString()));
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(requestedJob.Permission) &&
                        !caller.HasPermission(requestedJob.Permission))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_NO_PERMISSION.ToString()));
                        return;
                    }

                    if (JobUtil.IsJobFull(requestedJob))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_FULL.ToString()));
                        return;
                    }

                    var playerId = ulong.Parse(caller.Id);
                    if (Plugin.Inst.JoinJobCooldown.TryGetValue(playerId, out var lastLeave) &&
                        (DateTime.Now - lastLeave).TotalSeconds < Plugin.Conf.CooldownAfterLeaveJob)
                    {
                        Plugin.Say(caller,
                            Plugin.TranslateRich(EResponse.JOB_JOIN_COOLDOWN.ToString(),
                                Plugin.Conf.CooldownAfterLeaveJob -
                                (DateTime.Now - lastLeave).TotalSeconds));
                        return;
                    }

                    if (Plugin.Inst.JoinJobCooldown.ContainsKey(playerId))
                        Plugin.Inst.JoinJobCooldown.Remove(playerId);

                    PermissionUtil.AddPlayerToGroup(caller, requestedJob.PermissionGroup);
                    if (requestedJob.Salary != 0)
                    {
                        JobUtil.SalaryStart(caller as UnturnedPlayer, requestedJob);
                    }

                    Plugin.Say(caller,
                        Plugin.TranslateRich(EResponse.JOB_JOIN_SUCCESS.ToString(), requestedJob.JobName));
                    if (Plugin.Conf.AnnounceOnJoinJob)
                    {
                        Plugin.Say(Plugin.TranslateRich(EResponse.ANNOUNCE_JOB_JOIN.ToString(), caller.DisplayName,
                            requestedJob.JobName));
                    }

                    break;
                case EJobCommandOption.Apply:
                    if (caller is ConsolePlayer)
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.CONSOLE_NOT_ALLOWED.ToString()));
                        return;
                    }

                    if (command.Length == 1)
                    {
                        Plugin.Say(caller,
                            Plugin.TranslateRich(EResponse.INVALID_PARAMETER.ToString(),
                                "/job apply <jobName>"));
                        return;
                    }

                    if (JobUtil.HasJob(caller))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.HAS_JOB.ToString()));
                        return;
                    }

                    jobName = string.Join(" ", command.Skip(1));
                    flag2 = Plugin.Inst.PrivateJobs.TryGetValue(new Job {JobName = jobName},
                        out requestedJob);
                    if (!flag2)
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_NOT_FOUND.ToString()));
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(requestedJob.Permission) &&
                        !caller.HasPermission(requestedJob.Permission))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_NO_PERMISSION.ToString()));
                        return;
                    }

                    if (JobUtil.IsJobFull(requestedJob))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_FULL.ToString()));
                        return;
                    }

                    Plugin.Inst.PrivateJobRequest[requestedJob].Add(ulong.Parse(caller.Id));
                    Plugin.Say(caller,
                        Plugin.TranslateRich(EResponse.JOB_APPLY_SUCCESS.ToString(), requestedJob.JobName));
                    foreach (var steamPlayer in Provider.clients)
                    {
                        var uPlayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                        if (PermissionUtil.HasGroup(uPlayer, requestedJob.LeaderPermissionGroup))
                        {
                            Plugin.Say(uPlayer,
                                Plugin.TranslateRich(EResponse.JOB_APPLY_LEADER.ToString(), caller.DisplayName, requestedJob.JobName));
                        }
                    }

                    break;
                case EJobCommandOption.List:
                    if (caller is ConsolePlayer)
                    {
                        Logger.LogWarning($"[{Plugin.Inst.Name}] Public Jobs:");
                        foreach (var job in Plugin.Inst.PublicJobs)
                        {
                            Logger.LogWarning($"[{Plugin.Inst.Name}] {job}");
                        }

                        Logger.LogWarning($"[{Plugin.Inst.Name}] Private Jobs:");
                        foreach (var job in Plugin.Inst.PrivateJobs)
                        {
                            Logger.LogWarning($"[{Plugin.Inst.Name}] {job}");
                        }

                        return;
                    }

                    if (command.Length == 1)
                    {
                        Plugin.Say(caller,
                            Plugin.TranslateRich(EResponse.INVALID_PARAMETER.ToString(),
                                "/job list <public|private> [page]"));
                        return;
                    }

                    Enum.TryParse(command.ElementAtOrDefault(1), true, out opt);
                    byte.TryParse(command.ElementAtOrDefault(2), out var page);
                    if (page == 0)
                        page = 1;
                    
                    switch (opt)
                    {
                        case EJobCommandOption.Public:
                            if (Plugin.Inst.PublicJobs.Count is > 0 and <= 4)
                            {
                                foreach (var job in Plugin.Inst.PublicJobs)
                                {
                                    Plugin.Say(caller,
                                        Plugin.TranslateRich(EResponse.JOB_LIST.ToString(), job.JobName));
                                }

                                Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_LIST_END_PUBLIC.ToString()));
                                return;
                            }

                            if (Plugin.Inst.PublicJobs.Count > 4)
                            {
                                var count = 0;
                                var jobs = Plugin.Inst.PublicJobs.Skip((page - 1) * 4);
                                foreach (var job in jobs)
                                {
                                    if (count == 4)
                                        break;

                                    Plugin.Say(caller,
                                        Plugin.TranslateRich(EResponse.JOB_LIST.ToString(), job.JobName));
                                    count++;
                                }

                                Plugin.Say(caller,
                                    Plugin.Inst.PublicJobs.Count > (page * 4)
                                        ? Plugin.TranslateRich(EResponse.JOB_LIST_NEXT.ToString(), opt, page + 1)
                                        : Plugin.TranslateRich(EResponse.JOB_LIST_END_PUBLIC.ToString()));
                            }

                            break;
                        case EJobCommandOption.Private:
                            if (Plugin.Inst.PrivateJobs.Count is > 0 and <= 4)
                            {
                                foreach (var job in Plugin.Inst.PrivateJobs)
                                {
                                    Plugin.Say(caller,
                                        Plugin.TranslateRich(EResponse.JOB_LIST.ToString(), job.JobName));
                                }

                                Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_LIST_END_PRIVATE.ToString()));
                                return;
                            }

                            if (Plugin.Inst.PrivateJobs.Count > 4)
                            {
                                var count = 0;
                                var jobs = Plugin.Inst.PrivateJobs.Skip((page - 1) * 4);
                                foreach (var job in jobs)
                                {
                                    if (count == 4)
                                        break;

                                    Plugin.Say(caller,
                                        Plugin.TranslateRich(EResponse.JOB_LIST.ToString(), job.JobName));
                                    count++;
                                }

                                Plugin.Say(caller,
                                    Plugin.Inst.PublicJobs.Count > (page * 4)
                                        ? Plugin.TranslateRich(EResponse.JOB_LIST_NEXT.ToString(), opt, page + 1)
                                        : Plugin.TranslateRich(EResponse.JOB_LIST_END_PRIVATE.ToString()));
                            }

                            break;
                    }

                    break;
                case EJobCommandOption.Leave:
                    if (caller is ConsolePlayer)
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.CONSOLE_NOT_ALLOWED.ToString()));
                        return;
                    }

                    if (!JobUtil.HasJob(caller))
                    {
                        Plugin.Say(caller, Plugin.TranslateRich(EResponse.NO_JOB.ToString()));
                        return;
                    }

                    JobUtil.KickFromJob(caller);
                    JobUtil.SalaryStop(caller as UnturnedPlayer);

                    Plugin.Inst.JoinJobCooldown.Add(ulong.Parse(caller.Id), DateTime.Now);
                    Plugin.Say(caller, Plugin.TranslateRich(EResponse.JOB_LEAVE_SUCCESS.ToString()));
                    break;
            }

            return;

            Invalid_Parameter:
            Plugin.Say(caller, Plugin.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax));
        }
    }
}