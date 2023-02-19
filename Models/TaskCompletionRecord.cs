using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class TaskCompletionRecord
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string TaskType { get; set; }
        public string Description { get; set; }
        public string TaskEvidence { get; set; }
        public string TimeTaken { get; set; }
        public string TimeTakenEvidence { get; set; }
        public string? TaskDate { get; set; }
        public string CurrencyName { get; set; }
        public float CurrencyAwarded { get; set; }
        public string Verifiers { get; set; }
        public string RecordDate { get; set; }
        public string Status { get; set; }
    }
}
