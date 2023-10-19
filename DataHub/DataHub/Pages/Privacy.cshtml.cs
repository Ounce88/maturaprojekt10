using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DataHub.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }
        
        [BindProperty]
        public string Name { get; set; }

        public void OnGet()
        {
            

        }
        public IActionResult OnPost()
        {
            
            return Page();

        }
    }
}