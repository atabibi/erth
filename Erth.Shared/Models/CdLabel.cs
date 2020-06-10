using System;
using System.Collections.Generic;

namespace Erth.Shared.Models
{
    public class CdLabel
    {
        public int Id { get; set; }
        public string Label { get; set; }

        // 0: pro; 1: student
        public int TypeErth { get; set; }

        public ICollection<RegistredLabel> RegisteredLabels { get; set; }
    }
}