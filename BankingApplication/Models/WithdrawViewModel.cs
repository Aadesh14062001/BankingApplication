namespace BankingApplication.Models
{
    public class WithdrawViewModel
    {
        public int AccountId { get; set; }
        public string CustomerName { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AmountToWithdraw { get; set; }
    }
}