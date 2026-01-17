using EcommProject.DataAccess.Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommProject.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        ICoverTypeRepository CoverType { get; }
        ISP_CALL SP_CALL { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IApplicationUserRepository ApplicationUser{ get; }
        IShoppingCartRepository ShoppingCart{ get; }
        IOrderHeaderRepository OrderHeader{ get; }
        IOrderDetailsRepsitory OrderDetails{ get; }
        void Save();
    }
}
