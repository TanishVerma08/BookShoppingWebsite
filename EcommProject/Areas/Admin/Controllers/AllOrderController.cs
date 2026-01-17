using EcommProject.DataAccess.Repository.IRepository;
using EcommProject.Models;
using EcommProject.Models.ViewModels;
using EcommProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class AllOrderController : Controller
    {
        
        private readonly IUnitOfWork _unitOfWork;
        public AllOrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
  
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();
           
            var orders = _unitOfWork.OrderHeader.GetAll(includeProperties:"ApplicationUser").ToList();
            
            return View(orders);
           
        }
        public IActionResult Details(int id)
        {
            var orderDetails = _unitOfWork.OrderDetails.GetAll(od => od.OrderHeaderId == id, includeProperties: "Product,OrderHeader");
            foreach (var detail in orderDetails)
            {
                detail.Price = SD.GetPriceBasedOnQuantity(detail.Count, detail.Product.Price, detail.Product.Price50, detail.Product.Price100);
            }
            return View(orderDetails);
           
        }
        public IActionResult SearchByDate(DateTime fromDate , DateTime toDate)
        {
            if(toDate < fromDate)
            {
                ModelState.AddModelError(string.Empty, "To-Date must be greater than or Equal To From-Date");
                return RedirectToAction(nameof(SearchByDate));
            }
            var orders = _unitOfWork.OrderHeader.GetAll(filter:o=>o.OrderDate.Date >= fromDate && o.OrderDate.Date <= toDate);
            return View(orders);
        }
        public IActionResult SearchByStatus(DateTime fromDate, DateTime toDate)
        {
            if (toDate < fromDate)
            {
                ModelState.AddModelError(string.Empty, "To-Date must be greater than or Equal To From-Date");
                return RedirectToAction(nameof(SearchByDate));
            }
            var orders = _unitOfWork.OrderHeader.GetAll(filter: o => o.OrderDate.Date >= fromDate && o.OrderDate.Date <= toDate);
            return View(orders);
        }
    }
}
