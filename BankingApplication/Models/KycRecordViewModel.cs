namespace BankingApplication.Models
{
    public class KycRecordViewModel
    {
        public int KycID { get; set; }
        public string CustomerName { get; set; }
        public string PANCardPath { get; set; }
        public string AadhaarCardPath { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string Status { get; set; }
    }
}
