using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using EcommProject.DataAccess.Repository.IRepository;
using EcommProject.Models;
using EcommProject.Models.ViewModels;
using EcommProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        if (claims != null)
        {
            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
        }
        var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
        var orderDetails = _unitOfWork.OrderDetails.GetAll();
        var finalProduct = productList.Select(product => new OrderDetailVM
        {
            Products = product,
            TotalSold = orderDetails.Where(od=>od.ProductId == product.Id).Sum(od=>od.Count)
        }).OrderByDescending(vm=>vm.TotalSold).ToList();
        return View(finalProduct);

    }

    public IActionResult Details(int id)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        if (claims != null)
        {
            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
        }

        var productInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == id, includeProperties: "Category,CoverType");
        if (productInDb == null) return NotFound();

        var shoppingCart = new ShoppingCart()
        {
            Product = productInDb,
            ProductId = productInDb.Id
        };

        return View(shoppingCart);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        shoppingCart.Id = 0;
        if (ModelState.IsValid)
        {
            //User Ki Id Fetch krenge ki jisne login kraa h uski identity kya hai
            var claimIdentity = (ClaimsIdentity)(User.Identity);
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();
            shoppingCart.ApplicationUserId = claims.Value;  //Fer uske shopping cart ke applicationUSer ID mai store kr denge

            var shoppingCartInDb = _unitOfWork.ShoppingCart.FirstOrDefault(sc => sc.ApplicationUserId == claims.Value &&
            sc.ProductId == shoppingCart.ProductId);  //Is var mai vo variable store hoga jiski identity match ho jayegi
                                                      //means jo add to cart krra ha vo or jisne login kra h vo agr dono same hai
                                                      //tbhi code aage bhdega +++ Product hai ya nhii usme plee se koi

            if (shoppingCartInDb == null)  //Agr koi product nhi aya to add ho jayega means cart khali hai ple se
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            else  //agr ple se vo same product hoga to count plus hoga add nhi hogaa
                shoppingCartInDb.Count += shoppingCart.Count;
            _unitOfWork.Save(); //save hojayega
            return RedirectToAction(nameof(Index));

        }
        else
        {
            var productInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == shoppingCart.Id, includeProperties: "Category,CoverType");
            if (productInDb == null) return NotFound();

            var shoppingCartEdit = new ShoppingCart()
            {
                Product = productInDb,
                ProductId = productInDb.Id
            };
            return View(shoppingCartEdit);
        }
    }
 

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
