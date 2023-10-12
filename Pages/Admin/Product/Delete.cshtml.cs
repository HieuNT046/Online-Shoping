using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MyRazorPage.Pages.Product
{
    public class DeleteModel : PageModel
    {
        private readonly MyRazorPage.Models.PRN221_DBContext _context;

        private readonly IHubContext<HubServer> hubContext;
        public DeleteModel(MyRazorPage.Models.PRN221_DBContext _context, IHubContext<HubServer> hubContext)
        {
            this._context = _context;
            this.hubContext = hubContext;
        }
        public Models.Product? Product { get; set; }
        public async Task<IActionResult> OnGet(int id)
        {
            Product = _context.Products.FirstOrDefault(x => x.ProductId == id);
            if (Product.ProductId != id)
            {
                return NotFound();
            }
            else
            {
                Product.Discontinued = true;

            }
            await _context.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("ReloadProduct", await _context.Products.Include(x => x.Category).ToListAsync());
            return RedirectToPage("./Index");
        }
    }
}
