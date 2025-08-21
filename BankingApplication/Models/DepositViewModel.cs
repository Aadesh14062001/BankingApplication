namespace BankingApplication.Models
{
    public class DepositViewModel
    {
        public int AccountId { get; set; }
        public string CustomerName { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AmountToDeposit { get; set; }
    }
}