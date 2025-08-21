using BankingApplication.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace BankingApplication.Models
{
    public class AccountViewModel
    {
        public AccountModel Account { get; set; }
        public List<SelectListItem> Corporates { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public List<SelectListItem> Customers { get; set; }
        public List<SelectListItem> AccountTypes { get; set; }
        public List<SelectListItem> Currencies { get; set; }
    }
}
