using RFJob.Utils;
using Rocket.Unturned.Player;
using UnityEngine;

namespace RFJob
{
    public class PlayerComponent : UnturnedPlayerComponent
    {
        internal Coroutine SalaryCor { get; set; }
        protected override void Load()
        {
            var job = JobUtil.GetJob(Player);
            if (job != null)
            {
                JobUtil.SalaryStart(Player, job);
            }
        }

        protected override void Unload()
        {
            if (SalaryCor != null)
                Plugin.Inst.StopCoroutine(SalaryCor);
        }
    }
}