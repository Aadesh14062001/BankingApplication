namespace BankingApplication.Models
{
    public class AccountModel
    {
        public int AccountID { get; set; }
        public int CorporateID { get; set; }
        public int BranchID { get; set; }
        public int CustomerID { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        public string AccountNumber { get; set; }
        public DateTime OpenedOn { get; set; }
        public string CustomerName { get; set; }
    }
}