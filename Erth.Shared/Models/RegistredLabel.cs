using System;

namespace Erth.Shared.Models
{
    public class RegistredLabel
    {
        public int Id { get; set; }
        public string UserPcSN { get; set; }
        public string FullName { get; set; }
        public string United { get; set; }
        public string City { get; set; }
        public string Shobeh { get; set; }
        public DateTime DateOf { get; set; }
        
        public virtual CdLabel CdLabel { get; set; }
    }
}