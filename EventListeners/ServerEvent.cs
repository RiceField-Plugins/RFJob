using System;
using System.Collections.Generic;
using System.Linq;
using RFJob.Enums;
using RFJob.Models;
using RFJob.Utils;
using RFRocketLibrary.Hooks;

namespace RFJob.EventListeners
{
    internal static class ServerEvent
    {
        internal static void OnPostLevelLoaded(int level)
        {
            Plugin.Inst.PublicJobs = new HashSet<Job>(new JobComparer());
            Plugin.Inst.PrivateJobs = new HashSet<Job>(new JobComparer());
            Plugin.Inst.PrivateJobRequest = new Dictionary<Job, HashSet<ulong>>();
            Plugin.Inst.JoinJobCooldown = new Dictionary<ulong, DateTime>();
            foreach (var job in Plugin.Conf.Jobs.Where(x => x.Type == EJobType.PUBLIC))
            {
                Plugin.Inst.PublicJobs.Add(job);
            }
            foreach (var job in Plugin.Conf.Jobs.Where(x => x.Type == EJobType.PRIVATE))
            {
                Plugin.Inst.PrivateJobs.Add(job);
                Plugin.Inst.PrivateJobRequest.Add(job, new HashSet<ulong>());
            }

            switch (Plugin.Conf.Balance)
            {
                case EBalance.UCONOMY:
                    if (UconomyHook.CanBeLoaded())
                        UconomyHook.Load();
                    break;
                case EBalance.AVIECONOMY:
                    if (AviEconomyHook.CanBeLoaded())
                        AviEconomyHook.Load();
                    break;
            }

            if (Plugin.Conf.KickInactivePlayer)
            {
                Plugin.Inst.StartCoroutine(JobUtil.InactivePlayerEnumerator());
            }
        }
    }
}