namespace BankingApplication.Models
{
    public class KycReviewViewModel
    {
        public int KycID { get; set; }
        public string CustomerName { get; set; }
        public string PANCardPath { get; set; }
        public string AadhaarCardPath { get; set; }
        public string KycStatus { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string Remarks { get; set; }
    }
}
