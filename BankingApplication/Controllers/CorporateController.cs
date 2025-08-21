using BankingApplication.Models;

using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace BankingApplication.Controllers
{
    public class CorporateController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public CorporateController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // GET: Corporate List
        public IActionResult Index()
        {
            List<CorporateModel> corporates = new List<CorporateModel>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetAllCorporates", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    corporates.Add(new CorporateModel
                    {
                        CorporateID = Convert.ToInt32(dr["CorporateID"]),
                        CorporateName = dr["CorporateName"].ToString(),
                        Location = dr["Location"].ToString()
                    });
                }
            }

            return View(corporates);
        }

        // GET: Create Corporate
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Corporate
        [HttpPost]
        public IActionResult Create(CorporateModel corporate)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_InsertCorporate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CorporateName", corporate.CorporateName);
                    cmd.Parameters.AddWithValue("@Location", corporate.Location ?? (object)DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            return View(corporate);
        }

        // GET: Edit Corporate
        public IActionResult Edit(int id)
        {
            CorporateModel corporate = new CorporateModel();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetCorporateById", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CorporateID", id);
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    corporate.CorporateID = Convert.ToInt32(dr["CorporateID"]);
                    corporate.CorporateName = dr["CorporateName"].ToString();
                    corporate.Location = dr["Location"].ToString();
                }
            }
            return View(corporate);
        }

        // POST: Edit Corporate
        [HttpPost]
        public IActionResult Edit(CorporateModel corporate)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_UpdateCorporate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CorporateID", corporate.CorporateID);
                    cmd.Parameters.AddWithValue("@CorporateName", corporate.CorporateName);
                    cmd.Parameters.AddWithValue("@Location", corporate.Location ?? (object)DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            return View(corporate);
        }

        // GET: Delete Corporate
        public IActionResult Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteCorporate", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CorporateID", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}
