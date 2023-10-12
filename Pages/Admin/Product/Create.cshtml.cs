using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MyRazorPage.Pages.Product
{
    public class CreateModel : PageModel
    {
        private readonly MyRazorPage.Models.PRN221_DBContext _context;

        private readonly IHubContext<HubServer> hubContext;
        public CreateModel(MyRazorPage.Models.PRN221_DBContext _context, IHubContext<HubServer> hubContext)
        {
            this._context = _context;
            this.hubContext = hubContext;
        }

        [BindProperty]
        public Models.Product? Product { get; set; }
        public IActionResult OnGet(int id)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            if (ProductsExists(id))
            {
                Product = _context.Products.FirstOrDefault(x => x.ProductId == id);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
         /*   if (!ModelState.IsValid)
            {
                return Page();
            }*/

                if (Product.ProductId == 0)
            {
             
                    _context.Products.Add(Product);
              
            }
            else
            {
                _context.Products.Update(Product);

            }


            await _context.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("ReloadProduct", await _context.Products.ToListAsync());
            return RedirectToPage("/admin/product/index");
        }

        public bool ProductsExists(int id)
        {
            return _context.Products.Any(x => x.ProductId == id);
        }
    }
}
