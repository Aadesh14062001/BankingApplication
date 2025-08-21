namespace BankingApplication.Models
{
    public class TransactionLogViewModel
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } // Deposit / Withdraw
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerName { get; set; }
    }
}
