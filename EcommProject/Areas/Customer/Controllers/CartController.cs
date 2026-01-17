using EcommProject.DataAccess.Repository.IRepository;
using EcommProject.Models;
using EcommProject.Models.ViewModels;
using EcommProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Stripe.Climate;
using System.Collections.Generic;

namespace EcommProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private static bool isEmailConfirm = false;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ITwilioSender _twilioSender;
        public CartController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager,IEmailSender emailSender,ITwilioSender twilioSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
            _twilioSender = twilioSender;

        }
        [BindProperty] //ShoppingCartVM Model ko bind kra hai is property ke sath.
        public ShoppingCartVM ShoppingCartVM { get; set; }  
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);  //User ka data fetch krenge
            if (claims == null)  //agr login nhi hai ... Jo ki hoga nhi kunkii authorize kra hua hai..
            {
                ShoppingCartVM = new ShoppingCartVM()  
                {
                    ListCart = new List<ShoppingCart>() //To list cart khali rhega 
                };
                return View(ShoppingCartVM);
            }
            ShoppingCartVM = new ShoppingCartVM()  //User ko compare krenge ki jisne login kra hai or
                                                   //jo shopping cart access krra hai vo dono same hai
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc=>sc.ApplicationUserId==claims.Value,includeProperties:"Product"),
                                                                //List cart mai saare products aa jayenge
                                                                   
                OrderHeader = new OrderHeader()  //OrderHeader bhi access ho jayega 
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0; //OrderTotal zero initialize kra hai
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au=>au.Id == claims.Value);
                                                       //Application user mai vhi user store hoga jo login hai

            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50,list.Product.Price100);
                                    //Price accordingly set ho jayega jis hisab se order hogaa

                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price*list.Count);

                if(list.Product.Description.Length >= 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99)+"...";
                }
            }
            if (!isEmailConfirm)
            {
                ViewBag.EmailMessage = "Email must be confirmed for Authorize customer !!!";
                ViewBag.EmailCSS = "text-danger";
                isEmailConfirm = false;
            }
            else
            {
                ViewBag.EmailMessage = "Email has been sent kindly verify your Email";
                ViewBag.EmailCSS = "text-danger";
            }

                return View(ShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email empty!!!");
            }
            else
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code },
                    protocol: Request.Scheme);

                 await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            }
            return RedirectToAction(nameof(Index));

        }

        public IActionResult plus(int id)
        {
            var cart=_unitOfWork.ShoppingCart.Get(id);
            cart.Count += 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int id) 
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            if (cart.Count == 1)
                cart.Count = 1;
            else
                cart.Count -= 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult delete(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult summary(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc=>sc.ApplicationUserId== claims.Value,includeProperties:"Product"),
                OrderHeader = new OrderHeader()

            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au=>au.Id==claims.Value);
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count,list.Product.Price,list.Product.Price50,list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0,99)+"...";
                }
            }
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("summary")]
        public async Task<IActionResult> summaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            ShoppingCartVM.ListCart =_unitOfWork.ShoppingCart.GetAll(sc=>sc.ApplicationUser.Id == claims.Value,
                includeProperties:"Product");
            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId =list.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = list.Price,
                    Count = list.Count
                };
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price*list.Count);
                _unitOfWork.OrderDetails.Add(orderDetail);
                _unitOfWork.Save();
            }
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();
            //Session Count
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);

            //Stripe Payment
            if (stripeToken == null)  //For Company User Agr payment late krni hai to
            {
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal),
                    Currency = "usd",
                    Description = "Order Id : " + ShoppingCartVM.OrderHeader.Id.ToString(),
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)  //Payment nhi aai to
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

                    // Order Confirmation Email Code

                    var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
                    var userId = await _userManager.GetUserIdAsync(user);
                    string body = $"Order Confirmation: Order #{ShoppingCartVM.OrderHeader.Id}";
                    string message = "<p>Thank you for your order !!!</p>";
                    message += "<h3>Order Details:</h3>";
                    message += "<ul>";
                    foreach (var item in ShoppingCartVM.ListCart) 
                    {
                         message += $"<li>{item.Product.Title} -- Quantity: {item.Count}</li>";
                    }
                    message += "</ul>";
                    message += $"<p><strong>Total: </strong> ${ShoppingCartVM.OrderHeader.OrderTotal}</p>";
                    message += $"<p>Your order will be shipped till {ShoppingCartVM.OrderHeader.OrderDate.AddDays(7).Date}</p>";

                    await _emailSender.SendEmailAsync(user.Email, body, message);
                    var phone = user.PhoneNumber;
                    if (!phone.StartsWith("+"))
                    {
                        phone = "+91" + phone;
                    }

                    //await _twilioSender.SendOrderPlacedAsync(phone);
                    //await _twilioSender.SendOrderPlacedCallAsync(phone);

                }
                _unitOfWork.Save();
            }
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });

        }
        public IActionResult OrderConfirmation(int id)
        {
            ViewBag.Id = id;
            var orderDetails = _unitOfWork.OrderDetails.GetAll(od => od.OrderHeaderId == id, includeProperties: "Product");
            foreach (var detail in orderDetails)
            {
                detail.Price = SD.GetPriceBasedOnQuantity(detail.Count, detail.Product.Price, detail.Product.Price50, detail.Product.Price100);
            }
            return View(orderDetails);
        }
        public IActionResult Reorder(int orderId)
        {
            var orderDetails = _unitOfWork.OrderDetails
                .GetAll(od => od.OrderHeaderId == orderId, includeProperties: "Product")
                .ToList();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (userId == null || orderDetails == null || !orderDetails.Any())
            {
                TempData["Error"] = "Unable to reorder: No items found.";
                return RedirectToAction("Index", "Recent");
            }

            foreach (var item in orderDetails)
            {
                var existingCart = _unitOfWork.ShoppingCart
                    .FirstOrDefault(sc => sc.ApplicationUserId == userId && sc.ProductId == item.ProductId);

                if (existingCart != null)
                {
                    existingCart.Count += item.Count;
                }
                else
                {
                    ShoppingCart cartItem = new ShoppingCart()
                    {
                        ApplicationUserId = userId,
                        ProductId = item.ProductId,
                        Count = item.Count
                    };
                    _unitOfWork.ShoppingCart.Add(cartItem);
                }
            }

            _unitOfWork.Save();

            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == userId).Count();
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);

            return RedirectToAction(nameof(Index));
        }
    }
}
