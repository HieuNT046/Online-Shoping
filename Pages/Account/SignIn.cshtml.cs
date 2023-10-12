using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.common;
using MyRazorPage.Models;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Configuration;

namespace MyRazorPage.Pages.Account
{
  
    public class SignInModel : PageModel
    {

        private readonly PRN221_DBContext prn221DBContext;

        public SignInModel(PRN221_DBContext prn221DBContext) => this.prn221DBContext = prn221DBContext;

        [BindProperty]
        public Models.Account? account { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                account = await findByEmailAndPassword(account.Email, account.Password);

                if (account is not null)
                {
                    var claims = new List<Claim>
                    {
                       new Claim(ClaimTypes.Email, account.Email),
                       new Claim(ClaimTypes.Role, account.Role.ToString()),
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme
                        , new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme))
                        , new AuthenticationProperties
                        {
                            IsPersistent = true,
                            AllowRefresh = true,
                        });
                    HttpContext.Session.SetString("account", JsonSerializer.Serialize(account));
                    if (account.Role == 2 && account.Status == true) return RedirectToPage("/index");
                    else if (account.Role == 1) return RedirectToPage("/admin/product/index");

                    else if (account.Role == 2 && account.Status == false)
                    {
                        ViewData["message"] = "This account is inactive";
                         return Page();
                    }
                    else return Page();
                }
                else
                {
                    ViewData["message"] = "This account is not valid";
                    return Page();
                }
            }
            return Page();
        }
        public string Depass(string a)
        {
            var text = System.Convert.FromBase64String(a);
            return System.Text.Encoding.UTF8.GetString(text);
        }
        public async Task<Models.Account?> findByEmailAndPassword(String? email, String? password)
        {
            var accountInDB = await prn221DBContext.Accounts
                .FirstOrDefaultAsync(x => x.Email == email );
            if (accountInDB is not null && accountInDB.Status == true)
            {
                if (Depass(accountInDB.Password).Equals(password))
                {
                    return accountInDB;
                }
            }
            return null;
        }
    
    }
}
