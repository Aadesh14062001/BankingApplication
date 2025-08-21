using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class CustomerModel
    {
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Corporate is required")]
        public int? CorporateID { get; set; }

        [Required(ErrorMessage = "Branch is required")]
        public int? BranchID { get; set; }

        public string? CorporateName { get; set; }
        public string? BranchName { get; set; }

        [Required(ErrorMessage = "Customer Name is required")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty; // "Male", "Female", "Other"

        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(15, ErrorMessage = "Phone Number cannot exceed 15 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "PAN Number is required")]
        [StringLength(10, ErrorMessage = "PAN must be 10 characters")]
        public string PANNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aadhaar Number is required")]
        [StringLength(12, ErrorMessage = "Aadhaar must be 12 digits")]
        public string AadhaarNumber { get; set; } = string.Empty;
    }
}
