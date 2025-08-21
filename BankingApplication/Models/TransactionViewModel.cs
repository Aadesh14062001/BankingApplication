using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class TransactionViewModel
    {
        public DepositWithdrawViewModel DepositWithdraw { get; set; } = new();
        public TransactionHistoryViewModel History { get; set; } = new();
    }

    public class DepositWithdrawViewModel
    {
    //    [Required(ErrorMessage = "Account No is required.")]
        public string AccountNumber { get; set; }

        //[Required(ErrorMessage = "Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        // ✅ Add this for live balance preview
        public decimal CurrentBalance { get; set; }
    }




    public class TransactionHistoryViewModel
    {
        public string AccountNumber { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<TransactionHistoryItem> Items { get; set; } = new();
       

    }

    public class TransactionHistoryItem
    {
        public int TransactionId { get; set; }
        public int CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public int BranchId { get; set; }
        public string TransactionType { get; set; }
        public string Mode { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedOn { get; set; } // ✅ Already present
        public string CreatedBy { get; set; }
        public Guid RefId { get; set; }
        public string Status { get; set; }
        public decimal CurrentBalance { get; set; }


        // ✅ Add these:
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedDate { get; set; } 
    }
}