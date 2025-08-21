//using BankingApplication.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using System.Data;
//using System.Data.SqlClient;

//namespace BankingApplication.Controllers
//{
//    public class CustomerController : Controller
//    {
//        private readonly string _connectionString;

//        public CustomerController(IConfiguration configuration)
//        {
//            _connectionString = configuration.GetConnectionString("DefaultConnection");
//        }

//        // Helpers
//        private List<SelectListItem> GetCorporates()
//        {
//            var corporates = new List<SelectListItem>();

//            using var con = new SqlConnection(_connectionString);
//            using var cmd = new SqlCommand("dbo.sp_GetAllCorporates", con)
//            {
//                CommandType = CommandType.StoredProcedure
//            };

//            con.Open();
//            using var rdr = cmd.ExecuteReader();
//            while (rdr.Read())
//            {
//                corporates.Add(new SelectListItem
//                {
//                    Value = rdr["CorporateID"].ToString(),
//                    Text = rdr["CorporateName"].ToString()
//                });
//            }

//            return corporates;
//        }

//        private List<SelectListItem> GetBranches(int? corporateId = null)
//        {
//            var branches = new List<SelectListItem>();

//            using var con = new SqlConnection(_connectionString);
//            using var cmd = new SqlCommand("dbo.sp_GetBranchesByCorporateID", con)
//            {
//                CommandType = CommandType.StoredProcedure
//            };

//            cmd.Parameters.Add("@CorporateID", SqlDbType.Int).Value =
//                corporateId.HasValue ? corporateId.Value : (object)DBNull.Value;

//            con.Open();
//            using var rdr = cmd.ExecuteReader();
//            while (rdr.Read())
//            {
//                branches.Add(new SelectListItem
//                {
//                    Value = rdr["BranchID"].ToString(),
//                    Text = rdr["BranchName"].ToString()
//                });
//            }

//            return branches;
//        }

//        // List view
//        [HttpGet]
//        public IActionResult Index()
//        {
//            var customers = new List<CustomerModel>();

//            using var con = new SqlConnection(_connectionString);
//            using var cmd = new SqlCommand("dbo.sp_GetAllCustomers", con)
//            {
//                CommandType = CommandType.StoredProcedure
//            };

//            con.Open();
//            using var rdr = cmd.ExecuteReader();
//            while (rdr.Read())
//            {
//                customers.Add(new CustomerModel
//                {
//                    CustomerID = Convert.ToInt32(rdr["CustomerID"]),
//                    CorporateID = Convert.ToInt32(rdr["CorporateID"]),
//                    CorporateName = rdr["CorporateName"].ToString(),
//                    BranchID = Convert.ToInt32(rdr["BranchID"]),
//                    BranchName = rdr["BranchName"].ToString(),
//                    CustomerName = rdr["CustomerName"]?.ToString() ?? "",
//                    Gender = rdr["Gender"]?.ToString() ?? "",
//                    PhoneNumber = rdr["PhoneNumber"]?.ToString() ?? "",
//                    PANNumber = rdr["PANNumber"]?.ToString() ?? "",
//                    AadhaarNumber = rdr["AadhaarNumber"]?.ToString() ?? ""
//                });
//            }

//            return View(customers);
//        }

//        // Create form
//        [HttpGet]
//        public IActionResult Create()
//        {
//            ViewBag.Corporates = GetCorporates();
//            ViewBag.Branches = null; // only show fields after Load Branches
//            return View(new CustomerModel());
//        }

//        // Create submit (Load Branches + Save)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Create(CustomerModel model, string? loadBranches)
//        {
//            ViewBag.Corporates = GetCorporates();

//            // Load Branches button clicked
//            if (!string.IsNullOrEmpty(loadBranches))
//            {
//                ViewBag.Branches = GetBranches(model.CorporateID);
//                return View(model);
//            }

//            // Save button clicked — ensure branches are repopulated for redisplay
//            ViewBag.Branches = GetBranches(model.CorporateID);

//            if (!ModelState.IsValid)
//                return View(model);

//            try
//            {
//                using var con = new SqlConnection(_connectionString);
//                using var cmd = new SqlCommand("dbo.sp_UpsertCustomer", con)
//                {
//                    CommandType = CommandType.StoredProcedure
//                };

//                var customerIdParam = cmd.Parameters.Add("@CustomerID", SqlDbType.Int);
//                customerIdParam.Direction = ParameterDirection.InputOutput;
//                customerIdParam.Value = DBNull.Value;

//                cmd.Parameters.AddWithValue("@CorporateID", model.CorporateID!.Value);
//                cmd.Parameters.AddWithValue("@BranchID", model.BranchID!.Value);
//                cmd.Parameters.AddWithValue("@CustomerName", model.CustomerName?.Trim());
//                cmd.Parameters.AddWithValue("@Gender", model.Gender?.Trim());
//                cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber?.Trim());
//                cmd.Parameters.AddWithValue("@PANNumber", model.PANNumber?.Trim());
//                cmd.Parameters.AddWithValue("@AadhaarNumber", model.AadhaarNumber?.Trim());

//                var statusParam = cmd.Parameters.Add("@StatusMessage", SqlDbType.NVarChar, 200);
//                statusParam.Direction = ParameterDirection.Output;

//                con.Open();
//                cmd.ExecuteNonQuery();

//                string? status = statusParam.Value?.ToString();

//                if (!string.IsNullOrWhiteSpace(status) &&
//                    (status.Contains("successfully", StringComparison.OrdinalIgnoreCase) ||
//                     status.Contains("updated", StringComparison.OrdinalIgnoreCase)))
//                {
//                    TempData["Message"] = status;
//                    return RedirectToAction(nameof(Index));
//                }

//                ModelState.AddModelError("", status ?? "Something went wrong while saving.");
//            }
//            catch (SqlException ex)
//            {
//                ModelState.AddModelError("", $"SQL Error ({ex.Number}): {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
//            }

//            return View(model);
//        }
//    }
//}

using BankingApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Data.SqlClient;

namespace BankingApplication.Controllers
{
    public class CustomerController : Controller
    {
        private readonly string _connectionString;

        public CustomerController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Helpers
        private List<SelectListItem> GetCorporates()
        {
            var corporates = new List<SelectListItem>();

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_GetAllCorporates", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            con.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                corporates.Add(new SelectListItem
                {
                    Value = rdr["CorporateID"].ToString(),
                    Text = rdr["CorporateName"].ToString()
                });
            }

            return corporates;
        }

        private List<SelectListItem> GetBranches(int? corporateId = null)
        {
            var branches = new List<SelectListItem>();

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_GetBranchesByCorporateID", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@CorporateID", SqlDbType.Int).Value =
                corporateId.HasValue ? corporateId.Value : (object)DBNull.Value;

            con.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                branches.Add(new SelectListItem
                {
                    Value = rdr["BranchID"].ToString(),
                    Text = rdr["BranchName"].ToString()
                });
            }

            return branches;
        }

        // List view
        [HttpGet]
        public IActionResult Index()
        {
            var customers = new List<CustomerModel>();

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_GetAllCustomers", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            con.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                customers.Add(new CustomerModel
                {
                    CustomerID = Convert.ToInt32(rdr["CustomerID"]),
                    CorporateID = Convert.ToInt32(rdr["CorporateID"]),
                    CorporateName = rdr["CorporateName"].ToString(),
                    BranchID = Convert.ToInt32(rdr["BranchID"]),
                    BranchName = rdr["BranchName"].ToString(),
                    CustomerName = rdr["CustomerName"]?.ToString() ?? "",
                    Gender = rdr["Gender"]?.ToString() ?? "",
                    PhoneNumber = rdr["PhoneNumber"]?.ToString() ?? "",
                    PANNumber = rdr["PANNumber"]?.ToString() ?? "",
                    AadhaarNumber = rdr["AadhaarNumber"]?.ToString() ?? ""
                });
            }

            return View(customers);
        }

        // Create form
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Corporates = GetCorporates();
            ViewBag.Branches = null;
            return View(new CustomerModel());
        }

        // Create submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CustomerModel model, string? loadBranches)
        {
            ViewBag.Corporates = GetCorporates();

            if (!string.IsNullOrEmpty(loadBranches))
            {
                ViewBag.Branches = GetBranches(model.CorporateID);
                return View(model);
            }

            ViewBag.Branches = GetBranches(model.CorporateID);

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("dbo.sp_UpsertCustomer", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var customerIdParam = cmd.Parameters.Add("@CustomerID", SqlDbType.Int);
                customerIdParam.Direction = ParameterDirection.InputOutput;
                customerIdParam.Value = DBNull.Value;

                cmd.Parameters.AddWithValue("@CorporateID", model.CorporateID!.Value);
                cmd.Parameters.AddWithValue("@BranchID", model.BranchID!.Value);
                cmd.Parameters.AddWithValue("@CustomerName", model.CustomerName?.Trim());
                cmd.Parameters.AddWithValue("@Gender", model.Gender?.Trim());
                cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber?.Trim());
                cmd.Parameters.AddWithValue("@PANNumber", model.PANNumber?.Trim());
                cmd.Parameters.AddWithValue("@AadhaarNumber", model.AadhaarNumber?.Trim());

                var statusParam = cmd.Parameters.Add("@StatusMessage", SqlDbType.NVarChar, 200);
                statusParam.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();

                string? status = statusParam.Value?.ToString();

                if (!string.IsNullOrWhiteSpace(status) &&
                    (status.Contains("successfully", StringComparison.OrdinalIgnoreCase) ||
                     status.Contains("updated", StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = status;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", status ?? "Something went wrong while saving.");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", $"SQL Error ({ex.Number}): {ex.Message}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
            }

            return View(model);
        }

        // ❌ Delete customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int customerId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("dbo.sp_DeleteCustomer", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CustomerID", customerId);

                con.Open();
                cmd.ExecuteNonQuery();

                TempData["Message"] = $"Customer #{customerId} deleted successfully.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = $"SQL Error ({ex.Number}): {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
