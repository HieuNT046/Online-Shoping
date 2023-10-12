using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;

namespace MyRazorPage.Pages.Admin
{
    [Authorize(Roles = "1")]
    public class DashBoardModel : PageModel
    {
        private readonly MyRazorPage.Models.PRN221_DBContext dBContext;

        public DashBoardModel(MyRazorPage.Models.PRN221_DBContext _context)
        {
            this.dBContext  = _context;
           
        }
        [BindProperty]
        public HashSet<int> Years { get; set; }
        public long TotalWeeklySales { get; set; }
        public int TotalOrders { get; set; }

        public int TotalCustomers { get; set; }

        public int TotalGuests { get; set; }
        //public List<int> OrderInMonth { get; set; }

        public int Total { get; set; }
        public long jan { get; set; }
        public long feb { get; set; }
        public long mar { get; set; }
        public long apr { get; set; }
        public long may { get; set; }
        public long jun { get; set; }
        public long jul { get; set; }
        public long aug { get; set; }
        public long sep { get; set; }
        public long oct { get; set; }
        public long nov { get; set; }
        public long dec { get; set; }

        public async Task<IActionResult> OnGet(int? Year)
        {
            if (Year is null) Year = DateTime.Now.Year;

            Years = dBContext.Orders.Select(x => x.OrderDate.Value.Year).ToHashSet();

            //TotalWeeklySales
            DateTime FirstDayOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            TotalWeeklySales = (long)await dBContext.Orders
                .Where(order => order.OrderDate > FirstDayOfWeek && order.OrderDate < DateTime.Now)
                .SelectMany(order => order.OrderDetails)
                .SumAsync(orderDetail => orderDetail.Quantity * orderDetail.UnitPrice);

            //TotalOrders
            TotalOrders = dBContext.Orders.Count();

            //TotalCustomers
            TotalCustomers = dBContext.Accounts.Where(x => x.CustomerId != null).Count();

            //TotalGuests
            TotalGuests = dBContext.Customers.Count() - TotalCustomers;

            //OrderInMonth
            GetOrderInMonthToDashboard(Year);

            //Total
            Total = await dBContext.Customers.CountAsync();

            ViewData["Year"] = Year;

            return Page();
        }

        public void GetOrderInMonthToDashboard(int? Year)
        {

            jan = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 1)).Count();
            feb = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 2)).Count();
            mar = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 3)).Count();
            apr = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 4)).Count();
            may = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 5)).Count();
            jun = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 6)).Count();
            jul = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 7)).Count();
            aug = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 8)).Count();
            dec = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 9)).Count();
            oct = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 10)).Count();
            sep = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 11)).Count();
            dec = dBContext.Orders.Where(x => (((DateTime)x.OrderDate).Year == Year) && (((DateTime)x.OrderDate).Month == 12)).Count();

        }
    }
}
