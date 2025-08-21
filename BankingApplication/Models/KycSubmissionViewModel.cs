using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BankingApplication.Models
{
    public class KycSubmissionViewModel
    {
        [Required(ErrorMessage = "Please select a customer.")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Please upload PAN card.")]
        public IFormFile PANCardFile { get; set; }

        [Required(ErrorMessage = "Please upload Aadhaar card.")]
        public IFormFile AadhaarCardFile { get; set; }
    }
}
