using System;
using UnityEngine;

namespace RFJob.Models
{
    public class WorkshiftData
    {
        public Coroutine Cor { get; set; }
        public Coroutine AFKCor { get; set; }
        public uint AFKTime { get; set; }
        public DateTime? LastWorkshift { get; set; }
        public Vector3 LastPosition { get; set; }
        public WorkshiftData()
        {
            
        }
    }
}