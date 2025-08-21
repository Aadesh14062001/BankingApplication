
using BankingApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BankingApplication.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public TransactionController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        //  Dashboard
        [HttpGet]
        public IActionResult Index()
        {
            var model = new TransactionViewModel();
            ViewBag.Success = TempData["Success"];
            ViewBag.NewBalance = TempData["NewBalance"];
            return View(model);
        }

        //  Deposit with Live Balance Preview
        [HttpGet]
        public IActionResult Deposit(string accountNumber)
        {
            var model = new DepositWithdrawViewModel { AccountNumber = accountNumber };
            ViewBag.Success = TempData["Success"];
            ViewBag.NewBalance = TempData["NewBalance"];

            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("SELECT Balance FROM Account WHERE AccountNumber = @AccountNumber", con);
                cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);

                con.Open();
                var balance = cmd.ExecuteScalar();
                if (balance != null && balance != DBNull.Value)
                    model.CurrentBalance = (decimal)balance;
            }

            return View("Deposit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deposit(DepositWithdrawViewModel model)
        {
            if (!ModelState.IsValid) return View("Deposit", model);

            try
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("dbo.sp_CreditAmount", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@AccountNumber", model.AccountNumber);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);

                var pNewBal = new SqlParameter("@NewBalance", SqlDbType.Decimal) { Precision = 18, Scale = 2, Direction = ParameterDirection.Output };
                var pResult = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pMsg = new SqlParameter("@Message", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pNewBal);
                cmd.Parameters.Add(pResult);
                cmd.Parameters.Add(pMsg);

                con.Open();
                cmd.ExecuteNonQuery();

                int result = (int)pResult.Value;
                string msg = (string)pMsg.Value;
                decimal newBal = pNewBal.Value != DBNull.Value ? (decimal)pNewBal.Value : 0;

                if (result == 0)
                {
                    TempData["Success"] = $"Deposit successful. New Balance: ₹{newBal:0.00}";
                    TempData["NewBalance"] = newBal.ToString("0.00");
                    return RedirectToAction("Deposit", new { accountNumber = model.AccountNumber });
                }

                ModelState.AddModelError(string.Empty, msg ?? "Deposit failed.");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, $"Database error: {ex.Message}");
            }

            return View("Deposit", model);
        }

        // 💸 Withdraw with Live Balance Preview
        [HttpGet]
        public IActionResult Withdraw(string accountNumber)
        {
            var model = new DepositWithdrawViewModel { AccountNumber = accountNumber };
            ViewBag.Success = TempData["Success"];
            ViewBag.NewBalance = TempData["NewBalance"];

            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("SELECT Balance FROM Account WHERE AccountNumber = @AccountNumber", con);
                cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);

                con.Open();
                var balance = cmd.ExecuteScalar();
                if (balance != null && balance != DBNull.Value)
                    model.CurrentBalance = (decimal)balance;
            }

            return View("Withdraw", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Withdraw(DepositWithdrawViewModel model)
        {
            if (!ModelState.IsValid) return View("Withdraw", model);

            try
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("dbo.sp_DebitAmount", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@AccountNumber", model.AccountNumber);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);

                var pNewBal = new SqlParameter("@NewBalance", SqlDbType.Decimal) { Precision = 18, Scale = 2, Direction = ParameterDirection.Output };
                var pResult = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pMsg = new SqlParameter("@Message", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pNewBal);
                cmd.Parameters.Add(pResult);
                cmd.Parameters.Add(pMsg);

                con.Open();
                cmd.ExecuteNonQuery();

                int result = (int)pResult.Value;
                string msg = (string)pMsg.Value;
                decimal newBal = pNewBal.Value != DBNull.Value ? (decimal)pNewBal.Value : 0;

                if (result == 0)
                {
                    TempData["Success"] = $"Withdraw successful. New Balance: ₹{newBal:0.00}";
                    TempData["NewBalance"] = newBal.ToString("0.00");
                    return RedirectToAction("Withdraw", new { accountNumber = model.AccountNumber });
                }

                ModelState.AddModelError(string.Empty, msg ?? "Withdraw failed.");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, $"Database error: {ex.Message}");
            }

            return View("Withdraw", model);
        }

        //  Transaction History (GET)
        [HttpGet("Transaction/History")]
        public IActionResult TransactionHistory(string accountNumber)
        {
            var model = new TransactionHistoryViewModel
            {
                AccountNumber = accountNumber,
                Items = new List<TransactionHistoryItem>()
            };

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                //ModelState.AddModelError(nameof(model.AccountNumber), "Account Number is required.");
                return View("TransactionHistory", model);
            }

            try
            {
                using var con = new SqlConnection(_connectionString);
                con.Open();

                //  Get current balance
                using (var balCmd = new SqlCommand("SELECT Balance FROM Account WHERE AccountNumber = @acc", con))
                {
                    balCmd.Parameters.AddWithValue("@acc", accountNumber);
                    var balanceObj = balCmd.ExecuteScalar();
                    if (balanceObj != null && balanceObj != DBNull.Value)
                        model.ClosingBalance = (decimal)balanceObj;
                }

                //  Get transaction history
                using var cmd = new SqlCommand("dbo.sp_GetTransactionHistory", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    model.Items.Add(new TransactionHistoryItem
                    {
                        TransactionId = rdr.GetInt32(rdr.GetOrdinal("TransactionId")),
                        AccountNumber = rdr.GetString(rdr.GetOrdinal("AccountNumber")),
                        TransactionType = rdr.GetString(rdr.GetOrdinal("TransactionType")),
                        Amount = rdr.GetDecimal(rdr.GetOrdinal("Amount")),
                        CreatedOn = rdr.GetDateTime(rdr.GetOrdinal("CreatedOn")),
                        CreatedDate = rdr.GetDateTime(rdr.GetOrdinal("CreatedOn"))
                    });
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, $"Database error: {ex.Message}");
            }

            return View("TransactionHistory", model);
        }

        // 📜 Transaction History (POST)
        [HttpPost("Transaction/History")]
        [ValidateAntiForgeryToken]
        public IActionResult TransactionHistory(TransactionHistoryViewModel model)
        {
            model.Items = new List<TransactionHistoryItem>();

            if (string.IsNullOrWhiteSpace(model.AccountNumber))
            {
                //ModelState.AddModelError(nameof(model.AccountNumber), "Account Number is required.");
                return View("TransactionHistory", model);
            }

            try
            {
                using var con = new SqlConnection(_connectionString);
                con.Open();

                //  Get current balance
                using (var balCmd = new SqlCommand("SELECT Balance FROM Account WHERE AccountNumber = @acc", con))
                {
                    balCmd.Parameters.AddWithValue("@acc", model.AccountNumber);
                    var balanceObj = balCmd.ExecuteScalar();
                    if (balanceObj != null && balanceObj != DBNull.Value)
                        model.ClosingBalance = (decimal)balanceObj;
                }

                // Get transaction history
                using var cmd = new SqlCommand("dbo.sp_GetTransactionHistory", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@AccountNumber", model.AccountNumber);

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    model.Items.Add(new TransactionHistoryItem
                    {
                        TransactionId = rdr.GetInt32(rdr.GetOrdinal("TransactionId")),
                        AccountNumber = rdr.GetString(rdr.GetOrdinal("AccountNumber")),
                        TransactionType = rdr.GetString(rdr.GetOrdinal("TransactionType")),
                        Amount = rdr.GetDecimal(rdr.GetOrdinal("Amount")),
                        CreatedDate = rdr.GetDateTime(rdr.GetOrdinal("CreatedDate"))
                    });
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, $"Database error: {ex.Message}");
            }

            return View("TransactionHistory", model);
        }
    }
}




