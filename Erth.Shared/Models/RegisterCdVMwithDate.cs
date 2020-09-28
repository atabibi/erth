namespace Erth.Shared.Models
{
    public class RegisterCdVMwithDate 
    {
        public string FullName { get; set; }
        public string SoftwareType { get; set; }
        public string United { get; set; }
        public string City { get; set; }
        public string Shobeh { get; set; }
        public string CdLabel { get; set; }
        public string Sn { get; set; }
        public string DateRegistred { get; set; }

        public string ToCsv => $"{CdLabel},{FullName},{United},{City},{Shobeh},{(Sn =="unknown" ? "-" : Sn)},{DateRegistred}";
       
    }
}