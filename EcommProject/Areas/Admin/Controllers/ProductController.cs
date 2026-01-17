using EcommProject.DataAccess.Repository.IRepository;
using EcommProject.Models;
using EcommProject.Models.ViewModels;
using EcommProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment,IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data=_unitOfWork.Product.GetAll()});
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var productInDb = _unitOfWork.Product.Get(id);
            if (productInDb == null)
                return Json(new { success = false, message = "Something Went Wrong" });
            _unitOfWork.Product.Remove(productInDb);
            _unitOfWork.Save();
            var imagePath = Path.Combine(webRootPath, productInDb.ImageUrl.Trim('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            return Json(new { success = true, message = "Data Deleted Successfully" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            //Product product = new Product();
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(ct=> new SelectListItem()
                {
                    Text = ct.Name,
                    Value = ct.Id.ToString()
                })

            };
            if (id == null) return View(productVM);
            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            if (productVM.Product == null) return NotFound();
            return View(productVM);
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                if(files.Count()>0)
                {
                    var fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(files[0].FileName);

                    var uploads = Path.Combine(webRootPath, @"images\products");

                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;

                        productVM.Product.ImageUrl = imageExists;
                    }
                    if (productVM.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.Trim('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(uploads,fileName + extension),FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                else
                {
                    if(productVM.Product.Id != 0 )
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }
                }
                if (productVM.Product.Id == 0)
                    _unitOfWork.Product.Add(productVM.Product);
                else
                    _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                productVM = new ProductVM()
                {
                    Product = new Product(),
                    CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    }),
                    CoverTypeList = _unitOfWork.CoverType.GetAll().Select(ct => new SelectListItem()
                    {
                        Text = ct.Name,
                        Value = ct.Id.ToString()
                    })
                };
                if (productVM.Product.Id != 0)
                {
                    productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                }
                return View(productVM);
            }
        }
        [HttpPost]
        public async Task<IActionResult> ToggleDiscontinue(int id)
        {
            var product = _unitOfWork.Product.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            product.IsDiscontinued = !product.IsDiscontinued;
            string status = product.IsDiscontinued ? "discontinued" : "available";

            // If being discontinued, cancel unshipped orders
            if (product.IsDiscontinued)
            {
                var orderDetails = _unitOfWork.OrderDetails.GetAll(
                    od => od.ProductId == id && od.OrderHeader.OrderStatus != "Shipped",
                    includeProperties: "OrderHeader"
                ).ToList();

                var pendingOrders = orderDetails
                    .Select(od => od.OrderHeader)
                    .Distinct()
                    .ToList();

                foreach (var order in pendingOrders)
                {
                    order.OrderStatus = "Cancelled";

                    var user = _unitOfWork.ApplicationUser.FirstOrDefault(u => u.Id == order.ApplicationUserId);
                    if (user != null)
                    {
                        //Placeholder for email
                        await _emailSender.SendEmailAsync(user.Email, "order cancelled", $"order #{order.Id} has been cancelled...");
                    }
                }
            }
            _unitOfWork.Save();
            return Json(new { success = true, message = $"Product is now {status}." });
        }



    }
}
