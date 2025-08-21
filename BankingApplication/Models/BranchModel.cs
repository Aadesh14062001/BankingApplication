using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class BranchModel
    {
        // For displaying Corporate name in the list
        public string CorporateName { get; set; }
        public int BranchID { get; set; }

        [Required(ErrorMessage = "Branch name is required")]
        public string BranchName { get; set; }

        [Required(ErrorMessage = "Branch code is required")]
        public string BranchCode { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "Corporate selection is required")]
        public int CorporateID { get; set; }

        
    }
}
