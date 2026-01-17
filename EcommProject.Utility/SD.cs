using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EcommProject.Utility
{
    public class SD
    {
        //Cover Type
        public const string SP_GetCoverTypes = "sp_GetCoverTypes";
        public const string SP_GetCoverType = "sp_GetCoverType";
        public const string SP_CreateCoverType = "sp_CreateCoverType";
        public const string SP_UpdateCoverTypes = "sp_UpdateCoverType";
        public const string SP_DeleteCoverTypes = "sp_DeleteCoverType";

        //Roles
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee User";
        public const string Role_Company = "Company User";
        public const string Role_Individual = "Individual User";

        //Order Status
        public const string OrderStatusPending = "Pending";
        public const string OrderStatusApproved = "Approved";
        public const string OrderStatusInProgress = "Processing";
        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusCancelled = "Cancelled";
        public const string OrderStatusRefunded = "Refunded";

        //Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayPayment = "Payment Status Delay";
        public const string PaymentStatusRejected = "Rejected";

        //Session 
        public const string Ss_CartSessionCount = "Cart Count Session";

        //Method 
        public static double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity < 50)
                return price;
            else if (quantity < 100)
                return price50;return price100;
        }
    }
}
