using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;

namespace MyRazorPage.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;

        [BindProperty]
        public List<Models.Category> categories { get; set; }
        [BindProperty]
        public List<Models.Product> bestSaleProducts { get; set; }

        public IndexModel(PRN221_DBContext prn221DBContext)
            => this.prn221DBContext = prn221DBContext;

        public void OnGet()
        {
            loadCategories();
            loadBestSaleProducts();
        }

        private void loadCategories()
        {
            categories = prn221DBContext.Categories.ToList();
        }

        private void loadBestSaleProducts()
        {
            var listOrderDetails = prn221DBContext.OrderDetails
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();
            var listMostOrderProducts = listOrderDetails
                .Select(id =>
                {
                    int count = prn221DBContext.OrderDetails
                                .Where(x => x.ProductId == id)
                                 .Count();
                    return new
                    {
                        ProductId = id,
                        Count = count
                    };
                })
                .OrderByDescending(x => x.Count)
                .ToList();
            var listBestSaleProdcutsId = listMostOrderProducts
                                       .Take(4)
                                       .Select(x => x.ProductId)
                                       .ToHashSet();
            bestSaleProducts = prn221DBContext.Products
                                .Where(x => listBestSaleProdcutsId
                                             .Contains(x.ProductId))
                                .ToList();
        }
    }
}
