using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;
using System;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Security.Principal;
using System.Text.Json;
using System.Net.Mime;
using System.IdentityModel.Tokens.Jwt;
using DocumentFormat.OpenXml.Math;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Graphics;
using Aspose.Pdf;
using Syncfusion.Drawing;
using Color = Syncfusion.Drawing.Color;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Diagnostics.Metrics;

namespace MyRazorPage.Pages.Account
{
    public class CartModel : PageModel
    {
  
        private readonly PRN221_DBContext prn221DBContext;
        private readonly int lENGTH_CUSTOMER_ID = 5;
        private readonly Random _random = new();
        public CartModel(PRN221_DBContext prn221DBContext)
            =>  this.prn221DBContext = prn221DBContext;
        [BindProperty]
        public List<Cart>? carts { get; set; }

        [BindProperty]
        public Models.Customer? customer { get; set; }

        public async Task<IActionResult> OnGet()
        {
            string? getCart = HttpContext.Session.GetString("cart");
            string? accountSession = HttpContext.Session.GetString("account");

            if (getCart is not null)
            {
                carts = JsonSerializer.Deserialize<List<Cart>>(getCart);
                if (carts is not null)
                {
                    ViewData["TotalPrice"] = carts.Sum(x => x.Quantity * x.Product.UnitPrice);
                    ViewData["Quantity"] = carts.Sum(x => x.Quantity);
                }
                else
                {
                    return Redirect("/index");
                }
            }

            if (accountSession is not null)
            {
                var account = JsonSerializer.Deserialize<Models.Account>(accountSession);
                if (account is not null)
                {
                    customer = await findByCustomerId(account.CustomerId);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnGetBuyNow(int id)
        {

            bool isAddToCart = await AddToCart(id);
            if(isAddToCart)
            {
                return RedirectToPage("/account/cart");
            }
            return Redirect("/index");
        }

        public async Task<IActionResult> OnGetAddToCart(int id)
        {
            bool isAddToCart = await AddToCart(id);
            return Redirect("/product/detail?id="+id);
        }

        public IActionResult OnGetMinus(int id)
        {
            string? getCart = HttpContext.Session.GetString("cart");
            if (getCart is not null)
            {
                carts = JsonSerializer.Deserialize<List<Cart>>(getCart);
            }
            if (carts is not null)
            {
                int cartIndex = Exists(carts, id);
                if (cartIndex != -1)
                {
                    if (carts[cartIndex].Quantity > 1)
                    {
                        carts[cartIndex].Quantity--;
                    }
                }
                string saveCart = JsonSerializer.Serialize(carts);
                HttpContext.Session.SetString("cart", saveCart);
            }

            return RedirectToPage("/account/cart");
        }


        public IActionResult OnGetDelete(int id)
        {
            string? getCart = HttpContext.Session.GetString("cart");
            List<Cart>? carts = JsonSerializer.Deserialize<List<Cart>>(getCart);
            int index = Exists(carts, id);
            carts.RemoveAt(index);
            string savesjoncart = JsonSerializer.Serialize(carts);
            HttpContext.Session.SetString("cart", savesjoncart);
            if(carts.Count > 0)
            {
                return RedirectToPage("/account/cart");
            }
            return Redirect("/index");
        }

        public async Task<IActionResult> OnPostOrder(DateTime ? txtorderdate)
        {
            var session = HttpContext.Session.GetString("account");
        

            var acc = new Models.Account();
            var cus = new Customer();

            if (session is not null)
            {
                acc = JsonSerializer.Deserialize<MyRazorPage.Models.Account>(session);
                cus = prn221DBContext.Customers.SingleOrDefault(x => x.CustomerId == acc.CustomerId);
            }
            else
            {
                ViewData["txtorderdate"] = String.Format("{0:yyyy-MM-dd}", txtorderdate);
                if (txtorderdate > DateTime.Now.AddDays(5))
                {
                    ViewData["message"] = "Date Must <5";
                    return Page();
                }
                else
                {
                 
                    cus = new Customer
                    {
                        CustomerId = generatedCustomerId(),
                        CompanyName = customer.CompanyName,
                        ContactName = customer.ContactName,
                        ContactTitle = customer.ContactTitle,
                        Address = customer.Address
                    };
                    await prn221DBContext.Customers.AddAsync(cus);
                    await prn221DBContext.SaveChangesAsync();

                    Models.Order order = new Models.Order()
                    {
                        CustomerId = cus.CustomerId,
                        OrderDate = txtorderdate,
                        RequiredDate = txtorderdate,
                    };

                    await prn221DBContext.Orders.AddAsync(order);
                    await prn221DBContext.SaveChangesAsync();

                    string? jsoncart = HttpContext.Session.GetString("cart");
                    List<Cart>? carts = JsonSerializer.Deserialize<List<Cart>>(jsoncart);
                    {
                        foreach (var product in carts)
                        {
                            OrderDetail orderDetail = new();
                            orderDetail.OrderId = order.OrderId;
                            orderDetail.ProductId = product.Product.ProductId;
                            orderDetail.UnitPrice = (decimal)product.Product.UnitPrice;
                            orderDetail.Quantity = product.Quantity;
                            orderDetail.Discount = 0;
                            await prn221DBContext.OrderDetails.AddAsync(orderDetail);
                            await prn221DBContext.SaveChangesAsync();
                        }
                    };
                    HttpContext.Session.Remove("cart");
                    if (session is not null)
                    {
                        await sendEmail(order.OrderId);
                    }
                    return RedirectToPage("/Index");
                }

            }

            return Page();
        }

        private async Task<bool> AddToCart(int id)
        {
            string? getCart = HttpContext.Session.GetString("cart");
            if (getCart is not null)
            {
                carts = JsonSerializer.Deserialize<List<Cart>>(getCart);
            }
            if (carts is null)
            {
                carts = new List<Cart>();
                carts.Add(new Cart
                {
                    Product = await prn221DBContext.Products.SingleOrDefaultAsync(x => x.ProductId == id),
                    Quantity = 1
                });
                string saveCart = JsonSerializer.Serialize(carts);
                HttpContext.Session.SetString("cart", saveCart);
                return true;
            }
            else
            {
                int cartIndex = Exists(carts, id);
                if (cartIndex == -1)
                {
                    carts.Add(new Cart
                    {
                        Product = await prn221DBContext.Products.SingleOrDefaultAsync(x => x.ProductId == id),
                        Quantity = 1
                    });
                }
                else
                {
                    carts[cartIndex].Quantity++;
                }
                string saveCart = JsonSerializer.Serialize(carts);
                HttpContext.Session.SetString("cart", saveCart);
                return true;

            }
            
            return false;
        }

        private int Exists(List<Cart> cart, int id)
        {
            for (var i = 0; i < cart.Count; i++)
            {
                if (cart[i].Product.ProductId == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public async Task<Models.Customer?> findByCustomerId(String? customerId)
        {
            var customer = await prn221DBContext.Customers
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);
            if (customer is not null)
            {
                return customer;
            }
            return null;
        }

        private string generatedCustomerId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, lENGTH_CUSTOMER_ID)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public async Task sendEmail(int orderId)
        {
            var order =
               prn221DBContext.Orders
               .Where(o => o.OrderId == orderId)
              .Select(x => new
               {
                  Number = x.OrderId,
                  Date = x.OrderDate,
                  Customer = x.CustomerId
               }).First();

            var account =
                  prn221DBContext.Accounts
                  .Where(x => x.CustomerId == order.Customer)
                  .Include(x => x.Customer)
                  .Select(x => new
                  {
                      Number = x.CustomerId,
                      Name = x.Customer.ContactName,
                      Address = x.Customer.Address,
                      Email = x.Email
                  }).First();

            var orderDetail =
                prn221DBContext.OrderDetails
                .Where(x => x.OrderId == orderId)
                .Include(x => x.Product)
                .Select(x => new
                {
                    Name = x.Product.ProductName,
                    Price = x.Product.UnitPrice,
                    Quantity = x.Quantity,
                    TotalPrice = x.Product.UnitPrice * x.Quantity
                })
                .ToList();
            var res = string.Join("", orderDetail);
            var sum = orderDetail.Sum(y => y.TotalPrice);
            var items = orderDetail.Count;
      

            var data = new
            {
                Data = new
                {
                   Date = DateTime.Now.ToString("MM/dd/yyyy"),
                    res,
                   SubTotal = sum.ToString(),
                   Total = sum.ToString(),
                   Items = items.ToString()
               }

            };
            // Create a new instance of PdfDocument class.
            PdfDocument document = new PdfDocument();
            // Add a page to the document.
            PdfPage page = document.Pages.Add();
            // Create PDF graphics for the page.
            PdfGraphics g = page.Graphics;
            // Create a solid brush
            PdfBrush brush = new PdfSolidBrush(Color.Black);
            // Set the font.
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);
            string[] splittedStrings = data.ToString().Split(",");
            String d = "";
                foreach (string stringElement in splittedStrings)
            {
                d = d + stringElement + "\n";
            }
            string a = d;
            g.DrawString(a, font, brush, new PointF(10, 10));
            MemoryStream ms = new MemoryStream();
            document.Save(ms);
            document.Close(true);
    
            ms.Position = 0;
            Attachment file = new Attachment(ms, "Bill.pdf", "application/pdf");
            //Sends the email message
            //Update the required e-mail id here
           await SendEMail("hoangngoclong0807@gmail.com", account.Email, "Essential PDF document", "Create PDF MailBody", file);
        }
       

        private async Task SendEMail(string from, string recipients, string subject, string body, Attachment attachment)
        {
            //Creates the email message
            MailMessage emailMessage = new MailMessage(from, recipients);
            //Adds the subject for email
            emailMessage.Subject = subject;
            //Sets the HTML string as email body
            emailMessage.IsBodyHtml = false;
            emailMessage.Body = body;
            //Add the file attachment to this e-mail message.
            emailMessage.Attachments.Add(attachment);
            //Sends the email with prepared message
            using (SmtpClient client = new SmtpClient())
            {
                client.UseDefaultCredentials = false;
                NetworkCredential NetworkCred = new NetworkCredential("hoangngoclong0807@gmail.com", "yfdeogihcarsimoc");
                client.Credentials = NetworkCred;
                client.EnableSsl = true;
                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.Send(emailMessage);
            }
        }



    }

}
