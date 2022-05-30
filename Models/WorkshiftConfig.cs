namespace RFJob.Models
{
    public class WorkshiftConfig
    {
        public decimal BaseWorkshiftEarningPerMinute;
        public uint MaxWorkshiftInMinutes;
        public float CancelWorkshiftAfterAFKInSeconds;
        public uint WorkshiftCooldownInSeconds;

        public WorkshiftConfig()
        {
            
        }
    }
}