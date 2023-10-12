using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyRazorPage.Models;
using OfficeOpenXml;
using System.Configuration;
using System.Drawing.Printing;

namespace MyRazorPage.Pages.Product
{
    [Authorize(Roles = "1")]
    public class ListModel : PageModel
    {
        private readonly PRN221_DBContext _context;
        private readonly IConfiguration configuration;
        private readonly IHubContext<HubServer> hubContext;
        [BindProperty]
        public Pager<Models.Product> products { get; set; }

        [BindProperty]
        public List<Models.Category> categories { get; set; }

        [BindProperty]
        public int currentPage { get; set; }
        public ListModel(PRN221_DBContext dbContext, IConfiguration configuration, IHubContext<HubServer> hubContext)
        {
            _context = dbContext;
            this.configuration = configuration;
            this.hubContext = hubContext;
        }
        public async Task OnGet(int ddlCategory, string txtSearch, int? pageIndex)
        {
            categories = await _context.Categories.ToListAsync();
            if (pageIndex < 1) pageIndex = 1;
            await loadProducts( ddlCategory,  txtSearch,  pageIndex);
        }

        [Obsolete]
        public async Task<IActionResult> OnPost(IFormFile? file)
        {
            var listProductsFromExcel = new List<Models.Product>();

            if (file is not null)
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        for (int i = worksheet.Dimension.Start.Row + 1;
                            i <= worksheet.Dimension.End.Row; i++)
                        {
                            try
                            {
                                int j = 1;
                                string? productName = worksheet.Cells[i, j++].Value.ToString();
                                string? unitPrice = worksheet.Cells[i, j++].Value.ToString();
                                string? quantityPerUnit = worksheet.Cells[i, j++].Value.ToString();
                                String? unitInStock = worksheet.Cells[i, j++].Value.ToString();
                                String? category = worksheet.Cells[i, j++].Value.ToString();
                                String? discontinued = worksheet.Cells[i, j++].Value.ToString();
                                listProductsFromExcel.Add(new Models.Product
                                {
                                    ProductName = productName ?? "NameIsNull/deleted",
                                    UnitPrice = Decimal.Parse(unitPrice ?? "0"),
                                    QuantityPerUnit = quantityPerUnit ?? "0",
                                    UnitsInStock = short.Parse(unitInStock ?? "0"),
                                    CategoryId = Int32.Parse(category ?? "0"),
                                    Discontinued = bool.Parse(discontinued ?? "false"),
                                });

                            }
                            catch (Exception ex)
                            {
                                return Page();
                            }
                        }
                        await saveData(listProductsFromExcel);
                        await hubContext.Clients.All.SendAsync("ReloadData"
                        , await _context.Products.ToListAsync());
                        return RedirectToPage("/admin/product/index");
                    }
                }
            }
            return Page();
        }

        private async Task saveData(List<Models.Product> listProductsFromExcel)
        {
            await _context.Products.AddRangeAsync(listProductsFromExcel);
            await _context.SaveChangesAsync();
        }
        private async Task loadProducts(int ddlCategory, string txtSearch, int? pageIndex)
        {
            IQueryable<Models.Product> productsIQ = from product in _context.Products
                                                    select product;
            try
            {


                if (ddlCategory != 0)
                {
                    productsIQ = productsIQ.Where(x => x.CategoryId == ddlCategory);
                }
                if (txtSearch is not null)
                {
                    productsIQ = productsIQ.Where(x => x.ProductName.Contains(txtSearch));
                }
                int pageSize = configuration.GetValue("TableSize", 10);
                products = await Pager<Models.Product>.CreateAsync(productsIQ.Include(x => x.Category).
                   OrderByDescending(x => x.ProductId).AsNoTracking(), pageIndex ?? 1, pageSize);
                if (products is not null)
                {
                    currentPage = (int)Math.Ceiling((decimal)products.Count / (decimal)pageSize);
                }
            }
            catch (Exception ex)
            {
                ViewData["Mess"] = "NO data";
                Console.WriteLine(ex.Message);
            }
            ViewData["Category"] = ddlCategory;
            ViewData["Search"] = txtSearch;

            }

        public async Task<IActionResult> OnGetExport(int? ddlCategory, string? txtSearch)
        {
            List<Models.Product> Products = await _context.Products.Include(x => x.Category).ToListAsync();
            if (ddlCategory > 0) Products = Products.Where(x => x.CategoryId == ddlCategory).ToList();
            if (txtSearch != null) Products = Products.Where(x => x.ProductName.Contains(txtSearch)).ToList();
            categories = _context.Categories.ToList();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Product id";
                worksheet.Cell(currentRow, 2).Value = "Product name";
                worksheet.Cell(currentRow, 3).Value = "Category id";
                worksheet.Cell(currentRow, 4).Value = "Category name";
                worksheet.Cell(currentRow, 5).Value = "Quantity per unit";
                worksheet.Cell(currentRow, 6).Value = "Unit price";
                worksheet.Cell(currentRow, 7).Value = "Units in stock";
                worksheet.Cell(currentRow, 8).Value = "Units on order";
                worksheet.Cell(currentRow, 9).Value = "Reorder level";
                worksheet.Cell(currentRow, 10).Value = "Discontinued";
                foreach (var product in Products)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = product.ProductId;
                    worksheet.Cell(currentRow, 2).Value = product.ProductName;
                    worksheet.Cell(currentRow, 3).Value = product.CategoryId;
                    worksheet.Cell(currentRow, 4).Value = product.Category.CategoryName;
                    worksheet.Cell(currentRow, 5).Value = product.QuantityPerUnit;
                    worksheet.Cell(currentRow, 6).Value = product.UnitPrice;
                    worksheet.Cell(currentRow, 7).Value = product.UnitsInStock;
                    worksheet.Cell(currentRow, 8).Value = product.UnitsOnOrder;
                    worksheet.Cell(currentRow, 9).Value = product.ReorderLevel;
                    worksheet.Cell(currentRow, 10).Value = product.Discontinued;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "products.xlsx");
                }
            }
            return RedirectToPage("/admin/product");
        }
    }
}
