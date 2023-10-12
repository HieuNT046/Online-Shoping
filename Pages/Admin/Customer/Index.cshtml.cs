using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.common;
using MyRazorPage.Models;
using System.Net.Mail;
using System.Net;

namespace MyRazorPage.Pages.Admin.Customer
{
    [Authorize(Roles = "1")]
    public class IndexModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IConfiguration configuration;
        private readonly IHubContext<HubServer> hubContext;

        [BindProperty]
        public Pager<Models.Account>? accounts { get; set; }

        [BindProperty]
        public int currentPage { get; set; }


        public IndexModel(PRN221_DBContext prn221DBContext,
            IConfiguration configuration,
            IHubContext<HubServer> hubContext)
        {
            this.prn221DBContext = prn221DBContext;
            this.configuration = configuration;
            this.hubContext = hubContext;
        }
        public async Task<IActionResult> OnGet(int pg, string? txtSearch)
        {

            if (pg < 1) pg = 1;
            await getAllAccounts(pg, txtSearch);
            return Page();
        }

        public async Task<IActionResult> OnGetActive(int id)
        {
            bool isSend = false;
            var account = await prn221DBContext.Accounts.FirstOrDefaultAsync(x => x.AccountId == id);
            if (account is not null)
            {
                if (account.Status == true)
                {
                    account.Status = false;

                    //MailMessage mail = new MailMessage();
                    //SmtpClient SmtpServer = new SmtpClient();
                    //mail.From = new MailAddress("hoangngoclong0807@gmail.com");
                    //mail.To.Add(account.Email);
                    //mail.Subject = "Notification";
                    //mail.Body = string
                    //    .Format("Hi ,<br /><br />You have banned from website shopping.<br /><br />Thank You.");
                    //mail.IsBodyHtml = true;
                    //SmtpServer.UseDefaultCredentials = false;
                    //NetworkCredential NetworkCred = new NetworkCredential("hoangngoclong0807@gmail.com", "yfdeogihcarsimoc");
                    //SmtpServer.Credentials = NetworkCred;
                    //SmtpServer.EnableSsl = true;
                    //SmtpServer.Port = 587;
                    //SmtpServer.Host = "smtp.gmail.com";
                    //SmtpServer.Send(mail);
                    //ViewData["message"] = "Send mail success";
                    //isSend = true;

                }
                else account.Status = true;
                await prn221DBContext.SaveChangesAsync();
            }
            await hubContext.Clients.All.SendAsync("ReloadCustomer"
        , await prn221DBContext.Accounts.ToListAsync());
            return RedirectToPage("/admin/customer/index");
        }

        public async Task getAllAccounts(int? pageIndex, string? txtSearch)
        {
            IQueryable<Models.Account> accountsIQ = from account in prn221DBContext.Accounts
                                                    select account;
            if (txtSearch is not null)
            {
                accountsIQ = accountsIQ.Where(x => x.Email.Contains(txtSearch.Trim()));
                ViewData["txtSearch"] = txtSearch;
            }
            var pageSize = configuration.GetValue("TableSize", 5);
            accounts = await Pager<Models.Account>.CreateAsync(accountsIQ.Include(x => x.Customer).Where(x => x.Role == 2)
                       .AsNoTracking(), pageIndex ?? 1, pageSize);
            if (accounts is not null)
            {
                currentPage = (int)Math.Ceiling((decimal)accounts.Count / (decimal)pageSize);
            }
        }
    }
}