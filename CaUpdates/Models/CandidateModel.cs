using System;

namespace CaUpdates.Models
{
    class CandidateModel
    {
        public bool Incumbent { get; set; }
        public string Name { get; set; }
        public string Party { get; set; }
        public int Votes { get; set; }
        public Decimal Percentage { get; set; }
    }
}