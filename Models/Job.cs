using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using RFJob.Enums;

namespace RFJob.Models
{
    public class Job
    {
        [XmlAttribute] 
        public EJobType Type { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string PermissionGroup { get; set; } = string.Empty;
        public string LeaderPermissionGroup { get; set; }
        public string Permission { get; set; }
        [DefaultValue(-1)]
        public int MaxMembers { get; set; } = -1;
        [DefaultValue("")]
        public string ChatIconUrl { get; set; }
        [DefaultValue(0)]
        public decimal Salary { get; set; }
        [DefaultValue(0)]
        public uint SalaryIntervalInMinutes { get; set; }
        public bool RemoveFromJobOnDisconnect { get; set; }

        public Job()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is not Job job)
                return false;

            return string.Equals(job.JobName, JobName, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return (JobName != null ? JobName.ToLower().GetHashCode() : 0);
        }

        public override string ToString()
        {
            return
                $"Type: {Type}, JobName: {JobName}, PermissionGroup: {PermissionGroup}{(string.IsNullOrWhiteSpace(LeaderPermissionGroup) ? string.Empty : $", LeaderPermissionGroup: {LeaderPermissionGroup}")}{(string.IsNullOrWhiteSpace(Permission) ? string.Empty : $", Permission: {Permission}")}, MaxMembers: {(MaxMembers == -1 ? "Unlimited" : MaxMembers)}";
        }
    }
    
    public class JobComparer : IEqualityComparer<Job>
    {
        public bool Equals(Job x, Job y)
        {
            return string.Equals(x.JobName, y.JobName, StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode(Job obj)
        {
            return obj.JobName.ToLower().GetHashCode();
        }
    }
}