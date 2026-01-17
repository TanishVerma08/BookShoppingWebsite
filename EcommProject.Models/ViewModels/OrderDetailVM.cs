using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommProject.Models.ViewModels
{
    public class OrderDetailVM
    {
        public Product Products { get; set; }
        public int TotalSold { get; set; }
    }
}
