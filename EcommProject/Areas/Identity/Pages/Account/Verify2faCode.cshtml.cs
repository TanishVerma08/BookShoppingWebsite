
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using EcommProject.Utility;

namespace EcommProject.Areas.Identity.Pages.Account
{
    public class Verify2faCode : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITwilioSender _twilioSender;

        public Verify2faCode(
            SignInManager<IdentityUser> signInManager,

           ITwilioSender twilioSender)
        {
            _signInManager = signInManager;
            _twilioSender = twilioSender;
        }

        [BindProperty]
        public string Code { get; set; }

        [BindProperty(SupportsGet = true)]

        public string PhoneNumber { get; set; }
        
        [BindProperty(SupportsGet = true)]

        public string User { get; set; }


        public string ReturnUrl { get; set; }

        public void OnGet(string phoneNumber,string user)
        {
            PhoneNumber= phoneNumber;
            User = user;
        }
        public class InputModel
        {
            public string Email { get; set; }
            public string Code { get; set; }
            [BindProperty]
            public string PhoneNumber { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
       

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var isCodeValid = await _twilioSender.VerifyCodeAsync(Input.PhoneNumber, Input.Code);
            if (isCodeValid)
            {
                var user = await _signInManager.UserManager.FindByEmailAsync(User);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(ReturnUrl ?? "~/");
            }
            else 
            {
                ModelState.AddModelError(string.Empty, "Invalid Verification Code.");
                return Page();
            }

        }

    }
}
