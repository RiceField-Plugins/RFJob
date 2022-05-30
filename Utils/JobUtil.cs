using System;
using System.Collections.Generic;
using RFJob.DatabaseManagers;
using RFJob.Enums;
using RFJob.Managers;
using RFJob.Models;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Player;
using UnityEngine;

namespace RFJob.Utils
{
    public static class JobUtil
    {
        public static void KickFromJob(IRocketPlayer rPlayer)
        {
            var playerJob = GetJob(rPlayer);
            if (playerJob == null)
                return;

            PermissionUtil.RemovePlayerFromGroup(rPlayer, playerJob.PermissionGroup);
        }

        public static Job GetJob(IRocketPlayer rPlayer)
        {
            foreach (var job in Plugin.Conf.Jobs)
            {
                if (PermissionUtil.HasGroup(rPlayer, job.PermissionGroup))
                {
                    return job;
                }
            }

            return null;
        }

        public static bool HasJob(IRocketPlayer rPlayer)
        {
            foreach (var job in Plugin.Conf.Jobs)
            {
                if (PermissionUtil.HasGroup(rPlayer, job.PermissionGroup))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsLeader(IRocketPlayer rPlayer, Job job)
        {
            return !string.IsNullOrWhiteSpace(job.LeaderPermissionGroup) &&
                   PermissionUtil.HasGroup(rPlayer, job.LeaderPermissionGroup);
        }

        public static bool IsJobFull(Job job)
        {
            if (job.MaxMembers == -1)
                return false;

            return MemberCount(job) >= job.MaxMembers;
        }

        public static int MemberCount(Job job)
        {
            var rGroup = R.Permissions.GetGroup(job.PermissionGroup);
            return rGroup?.Members?.Count ?? 0;
        }

        public static IEnumerator<WaitForSeconds> SalaryEnumerator(PlayerComponent cPlayer, Job job)
        {
            if (job == null)
                yield break;

            while (Plugin.Conf.Enabled && job.Salary > 0 &&
                   PermissionUtil.HasGroup(cPlayer.Player, job.PermissionGroup))
            {
                yield return new WaitForSeconds(job.SalaryIntervalInMinutes * 60);
                BalanceManager.AddBalance(cPlayer.Player, job.Salary);
                Plugin.Say(cPlayer.Player, Plugin.TranslateRich(EResponse.SALARY.ToString(), job.Salary));
            }

            cPlayer.SalaryCor = null;
        }

        public static void SalaryStart(UnturnedPlayer player, Job job)
        {
            if (player == null)
                return;

            var cPlayer = player.GetComponent<PlayerComponent>();
            cPlayer.SalaryCor = Plugin.Inst.StartCoroutine(SalaryEnumerator(cPlayer, job));
        }

        public static void SalaryStop(UnturnedPlayer player)
        {
            if (player == null)
                return;

            var cPlayer = player.GetComponent<PlayerComponent>();
            if (cPlayer.SalaryCor != null)
                Plugin.Inst.StopCoroutine(cPlayer.SalaryCor);
            cPlayer.SalaryCor = null;
        }

        public static IEnumerator<WaitForSeconds> InactivePlayerEnumerator()
        {
            while (Plugin.Conf.Enabled)
            {
                yield return new WaitForSeconds(3600);
                foreach (var data in PlayerDataManager.Get())
                {
                    if (!data.LastActive.HasValue)
                        continue;

                    if ((DateTime.Now - data.LastActive.Value).TotalDays >=
                        Plugin.Conf.KickInactivePlayerAfterDays)
                    {
                        KickFromJob(new RocketPlayer(data.SteamId.ToString()));
                    }
                }
            }
        }
    }
}