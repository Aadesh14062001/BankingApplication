using BankingApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace YourNamespace.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;
        private object _configuration;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
        {
            var accounts = new List<AccountModel>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetAllAccounts", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        accounts.Add(new AccountModel
                        {
                            AccountID = Convert.ToInt32(rdr["AccountID"]),
                            AccountNumber = rdr["AccountNumber"].ToString(),
                            AccountType = rdr["AccountType"].ToString(),
                            Currency = rdr["Currency"].ToString(),
                            OpenedOn = Convert.ToDateTime(rdr["OpenedOn"]),
                            CustomerID = Convert.ToInt32(rdr["CustomerID"]),
                            BranchID = Convert.ToInt32(rdr["BranchID"]),
                            CorporateID = Convert.ToInt32(rdr["CorporateID"]),
                            CustomerName = rdr["CustomerName"].ToString()
                        });
                    }
                }
            }

            return View(accounts);
        }

        // GET: Create Account
        public IActionResult Create()
        {
            var model = new AccountViewModel
            {
                Corporates = GetCorporates(),
                Branches = new List<SelectListItem>(),
                Customers = new List<SelectListItem>(),
                AccountTypes = GetAccountTypes(),
                Currencies = GetCurrencies(),
                Account = new AccountModel()
            };

            return View(model);
        }

        // POST: Create Account
        [HttpPost]
        public IActionResult Create(AccountViewModel model)
        {
            //if (ModelState.IsValid)
            {
                // Generate account number if customer is selected
                if (model.Account.BranchID > 0 && model.Account.CustomerID > 0)
                {
                    model.Account.AccountNumber = GenerateAccountNumber();
                }
                else
                {
                    ModelState.AddModelError("", "Please select Branch and Customer before creating account.");
                    RepopulateDropdowns(model);
                    return View(model);
                }

                // Set current date automatically
                model.Account.OpenedOn = DateTime.Now;

                using (SqlConnection con = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_InsertAccount", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CorporateID", model.Account.CorporateID);
                    cmd.Parameters.AddWithValue("@BranchID", model.Account.BranchID);
                    cmd.Parameters.AddWithValue("@CustomerID", model.Account.CustomerID);
                    cmd.Parameters.AddWithValue("@AccountType", model.Account.AccountType ?? "");
                    cmd.Parameters.AddWithValue("@Currency", model.Account.Currency ?? "");
                    cmd.Parameters.AddWithValue("@AccountNumber", model.Account.AccountNumber);
                    cmd.Parameters.AddWithValue("@OpenedOn", model.Account.OpenedOn);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }

            RepopulateDropdowns(model);
            return View(model);
        }

        private void RepopulateDropdowns(AccountViewModel model)
        {
            model.Corporates = GetCorporates();
            model.Branches = model.Account.CorporateID > 0 ? GetBranches(model.Account.CorporateID) : new List<SelectListItem>();
            model.Customers = model.Account.BranchID > 0 ? GetCustomers(model.Account.BranchID) : new List<SelectListItem>();
            model.AccountTypes = GetAccountTypes();
            model.Currencies = GetCurrencies();
        }

        // Get Branches by Corporate
        public List<SelectListItem> GetBranches(int corporateId)
        {
            var branches = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetBranchesByCorporate", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CorporateID", corporateId);
                con.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        branches.Add(new SelectListItem
                        {
                            Value = rdr["BranchID"].ToString(),
                            Text = rdr["BranchName"].ToString()
                        });
                    }
                }
            }
            return branches;
        }

        // Get Customers by Branch
        public List<SelectListItem> GetCustomers(int branchId)
        {
            var customers = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetCustomersByBranch", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BranchID", branchId);
                con.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        customers.Add(new SelectListItem
                        {
                            Value = rdr["CustomerID"].ToString(),
                            Text = rdr["CustomerName"].ToString()
                        });
                    }
                }
            }
            return customers;
        }

        // Get all Corporates
        public List<SelectListItem> GetCorporates()
        {
            var corporates = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetAllCorporates", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        corporates.Add(new SelectListItem
                        {
                            Value = rdr["CorporateID"].ToString(),
                            Text = rdr["CorporateName"].ToString()
                        });
                    }
                }
            }
            return corporates;
        }

        // Get Account Types dropdown (static list, can be replaced with DB SP)
        public List<SelectListItem> GetAccountTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Savings", Text = "Savings" },
                new SelectListItem { Value = "Current", Text = "Current" },
                new SelectListItem { Value = "Fixed Deposit", Text = "Fixed Deposit" }
            };
        }

        // Get Currencies dropdown (static list, can be replaced with DB SP)
        public List<SelectListItem> GetCurrencies()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "USD", Text = "USD" },
                new SelectListItem { Value = "EUR", Text = "EUR" },
                new SelectListItem { Value = "INR", Text = "INR" }
            };
        }

        // Generate a 12-digit Account Number
        private string GenerateAccountNumber()
        {
            Random random = new Random();
            return random.Next(100000000, 999999999).ToString() + random.Next(10, 99).ToString(); // ensures 12 digits
        }

        [Obsolete]
        public IActionResult Delete(int Accountid)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
         

            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteAccount", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AccountID", Accountid);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return RedirectToAction("Index");
        }

    }
}

