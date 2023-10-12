using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;

namespace MyRazorPage.Pages.Admin.Product
{
    public class TestModel : PageModel
    {
        private readonly MyRazorPage.Models.PRN221_DBContext _context;

        private readonly IHubContext<HubServer> hubContext;
        public TestModel(MyRazorPage.Models.PRN221_DBContext _context, IHubContext<HubServer> hubContext)
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

        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(int id)
        {
          if (!ModelState.IsValid)
            {
                return Page();
            }
            if (ProductsExists(id))
            {

                if (Product.ProductId != id)
                {
                    return NotFound();
                }
                else
                {
                    _context.Products.Update(Product);

                }
            }
            else
            {
                _context.Products.Add(Product);

            }


            await _context.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("ReloadProduct", await _context.Products.Include(x => x.Category).Select(x => new
            {
                productId = x.ProductId,
                productName = x.ProductName,
                unitPrice = x.UnitPrice,
                categoryName = x.Category.CategoryName,
            }).ToListAsync());
            return RedirectToPage("./Index");
        }

        public bool ProductsExists(int id)
        {
            return _context.Products.Any(x => x.ProductId == id);
        }
    }
}
