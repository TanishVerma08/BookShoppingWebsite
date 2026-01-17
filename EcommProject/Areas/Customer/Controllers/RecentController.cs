using EcommProject.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class RecentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public RecentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();

            string userId = claims.Value;
            var recentOrders = _unitOfWork.OrderHeader.GetAll(o => o.ApplicationUserId == userId, includeProperties: "ApplicationUser")
                .OrderByDescending(o => o.OrderDate).ToList();
            return View(recentOrders);
        }
    }
}
