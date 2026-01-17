using EcommProject.Data;
using EcommProject.DataAccess.Repositories.IRepository;
using EcommProject.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommProject.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(context);
            CoverType = new CoverTypeRepository(context);
            SP_CALL = new SP_CALL(context);
            Product = new ProductRepository(context);
            Company = new CompanyRepository(context);
            ApplicationUser = new ApplicationUserRepository(context);
            ShoppingCart = new ShoppingCartRepository(context);
            OrderHeader = new OrderHeaderRepository(context);
            OrderDetails = new OrderDetailsRepository(context);
        }
        public ICategoryRepository Category { private set; get; }

        public ICoverTypeRepository CoverType { private set; get; }
        public ISP_CALL SP_CALL { private set; get; }

        public IProductRepository Product { private set; get; }
        public ICompanyRepository Company { private set; get; }

        public IApplicationUserRepository ApplicationUser { private set; get; }
        public IShoppingCartRepository ShoppingCart { private set; get; }
        public IOrderDetailsRepsitory OrderDetails { private set; get; }
        public IOrderHeaderRepository OrderHeader { private set; get; }

      

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
