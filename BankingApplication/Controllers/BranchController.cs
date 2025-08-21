

using BankingApplication.Models;  
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BankingApplication.Controllers
{
    public class BranchController : Controller
    {
        private readonly IConfiguration _configuration;

        public BranchController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Load Corporate dropdown items for Create/Edit views
        private void LoadCorporateDropdown()
        {
            List<SelectListItem> corporateList = new List<SelectListItem>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllCorporates", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        corporateList.Add(new SelectListItem
                        {
                            Value = rdr["CorporateID"].ToString(),
                            Text = rdr["CorporateName"].ToString()
                        });
                    }
                    con.Close();
                }
            }

            ViewBag.CorporateList = corporateList;
        }

        // =================== INDEX ===================
        public IActionResult Index()
        {
            List<BranchModel> branches = new List<BranchModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllBranches", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        branches.Add(new BranchModel
                        {
                            BranchID = Convert.ToInt32(rdr["BranchID"]),
                            BranchName = rdr["BranchName"].ToString(),
                            BranchCode = rdr["BranchCode"].ToString(),
                            City = rdr["City"].ToString(),
                            CorporateID = Convert.ToInt32(rdr["CorporateID"]),
                            CorporateName = rdr["CorporateName"].ToString()
                        });
                    }
                    con.Close();
                }
            }

            return View(branches);
        }

        // =================== CREATE - GET ===================
        [HttpGet]
        public IActionResult Create()
        {
            LoadCorporateDropdown();
            return View();
        }

        // =================== CREATE - POST ===================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BranchModel model)
        {
            //if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_InsertBranch", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BranchName", model.BranchName);
                        cmd.Parameters.AddWithValue("@BranchCode", model.BranchCode);
                        cmd.Parameters.AddWithValue("@City", model.City);
                        cmd.Parameters.AddWithValue("@CorporateID", model.CorporateID);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
                return RedirectToAction("Index");
            }

            LoadCorporateDropdown();
            return View(model);
        }

        // =================== EDIT - GET ===================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            BranchModel model = new BranchModel();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetBranchById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchID", id);

                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        model.BranchID = Convert.ToInt32(rdr["BranchID"]);
                        model.BranchName = rdr["BranchName"].ToString();
                        model.BranchCode = rdr["BranchCode"].ToString();
                        model.City = rdr["City"].ToString();
                        model.CorporateID = Convert.ToInt32(rdr["CorporateID"]);
                    }
                    con.Close();
                }
            }

            LoadCorporateDropdown();
            return View(model);
        }

        // =================== EDIT - POST ===================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BranchModel model)
        {
            //if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_UpdateBranch", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BranchID", model.BranchID);
                        cmd.Parameters.AddWithValue("@BranchName", model.BranchName);
                        cmd.Parameters.AddWithValue("@BranchCode", model.BranchCode);
                        cmd.Parameters.AddWithValue("@City", model.City);
                        cmd.Parameters.AddWithValue("@CorporateID", model.CorporateID);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
                return RedirectToAction("Index");
            }

            LoadCorporateDropdown();
            return View(model);
        }

        // =================== DELETE - POST ===================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteBranch", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchID", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return RedirectToAction("Index");
        }
    }
}
