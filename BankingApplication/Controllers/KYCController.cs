


using BankingApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BankingApplication.Controllers
{
    public class KycController : Controller
    {
        private readonly IConfiguration _configuration;

        public KycController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //  . Dashboard: Show all KYC submissions
        public IActionResult Index()
        {
            List<KycRecordViewModel> records = new List<KycRecordViewModel>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllKYCRecords", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        records.Add(new KycRecordViewModel
                        {
                            KycID = Convert.ToInt32(reader["KycID"]),
                            CustomerName = reader["CustomerName"].ToString(),
                            PANCardPath = reader["PANCardPath"].ToString(),
                            AadhaarCardPath = reader["AadhaarCardPath"].ToString(),
                            SubmittedOn = Convert.ToDateTime(reader["SubmittedDate"]),
                            Status = reader["KycStatus"].ToString()
                        });
                    }

                    conn.Close();
                }
            }

            return View(records);
        }

        //  Submit KYC (GET) with optional customerId
        [HttpGet]
        public IActionResult SubmitKYC(int? customerId = null)
        {
            ViewBag.CustomerList = GetCustomerDropdown();

            if (customerId.HasValue && customerId.Value > 0)
            {
                ViewBag.SelectedCustomer = GetCustomerDetails(customerId.Value);
            }

            return View(new KycSubmissionViewModel { CustomerID = customerId ?? 0 });
        }

        //  Submit KYC (POST)
        [HttpPost]
        public IActionResult SubmitKYC(KycSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CustomerList = GetCustomerDropdown();
                ViewBag.SelectedCustomer = GetCustomerDetails(model.CustomerID);
                return View(model);
            }

            string panPath = SaveFile(model.PANCardFile);
            string aadhaarPath = SaveFile(model.AadhaarCardFile);

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_SubmitKYC", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerID", model.CustomerID);
                    cmd.Parameters.AddWithValue("@PANCardPath", panPath);
                    cmd.Parameters.AddWithValue("@AadhaarCardPath", aadhaarPath);
                    cmd.Parameters.AddWithValue("@SubmittedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@KycStatus", "Pending");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }

            TempData["Success"] = "KYC submitted successfully!";
            return RedirectToAction("Index");
        }

        //  Review KYC (GET)
        [HttpGet]
        public IActionResult ReviewKYC(int id)
        {
            KycRecordViewModel record = null;

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetKYCById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@KycID", id);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        record = new KycRecordViewModel
                        {
                            KycID = id,
                            CustomerName = reader["CustomerName"].ToString(),
                            PANCardPath = reader["PANCardPath"].ToString(),
                            AadhaarCardPath = reader["AadhaarCardPath"].ToString(),
                            SubmittedOn = Convert.ToDateTime(reader["SubmittedDate"]),
                            Status = reader["KycStatus"].ToString()
                        };
                    }

                    conn.Close();
                }
            }

            return View(record);
        }

        //  Review KYC (POST)
        [HttpPost]
        public IActionResult ReviewKYC(int id, string status)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateKYCStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@KycID", id);
                    cmd.Parameters.AddWithValue("@KycStatus", status);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }

            TempData["Success"] = $"KYC marked as {status}.";
            return RedirectToAction("Index");
        }

        // Save uploaded file
        private string SaveFile(IFormFile file)
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/uploads/" + fileName;
        }

        //  Load customer dropdown with name and mobile number
        private List<SelectListItem> GetCustomerDropdown()
        {
            var customers = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Customer --" }
            };

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT CustomerID, CustomerName, PhoneNumber FROM Customer";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string id = reader["CustomerID"].ToString();
                    string name = reader["CustomerName"].ToString();
                    string mobile = reader["PhoneNumber"].ToString();

                    customers.Add(new SelectListItem
                    {
                        Value = id,
                        Text = $"{name} | {mobile}"
                    });
                }

                conn.Close();
            }

            return customers;
        }

        //  Get customer details for preview
        private CustomerViewModel GetCustomerDetails(int customerId)
        {
            CustomerViewModel customer = null;

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT CustomerID, CustomerName, PhoneNumber FROM Customer WHERE CustomerID = @CustomerID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    customer = new CustomerViewModel
                    {
                        CustomerID = Convert.ToInt32(reader["CustomerID"]),
                        CustomerName = reader["CustomerName"].ToString(),
                        MobileNumber = reader["PhoneNumber"].ToString(),
                        
                    };
                }

                conn.Close();
            }

            return customer;
        }

        //  Update KYC Status with Remarks
        [HttpPost]
        public IActionResult UpdateKycStatus(int KycID, string NewStatus, string Remarks)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateKYCStatusWithRemarks", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@KycID", KycID);
                    cmd.Parameters.AddWithValue("@KycStatus", NewStatus);
                    cmd.Parameters.AddWithValue("@VerifiedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Remarks", Remarks ?? "");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }

            TempData["Success"] = $"KYC #{KycID} marked as {NewStatus}.";
            return RedirectToAction("Index");
        }
    }
}
