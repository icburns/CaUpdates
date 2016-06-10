using System;
using System.Collections.Generic;

namespace CaUpdates.Models
{
    class StateResultsModel
    {
        public string RaceTitle { get; set; }
        public int PrecinctsReporting { get; set; }
        public int PrecinctsTotal { get; set; }
        public string Timestamp { get; set; }
        public string District { get; set; }
        public string County { get; set; }
        public IEnumerable<CandidateModel> Candidates { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}