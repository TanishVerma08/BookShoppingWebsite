using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommProject.Utility
{
    public interface ITwilioSender
    {
        Task SendVerificationCodeAsync(string phoneNumber, string channel);
       
        Task<bool> VerifyCodeAsync(string phoneNumber, string code);
        Task SendOrderPlacedAsync(string phoneNumber);
        Task SendOrderPlacedCallAsync(string phoneNumber);
    }
}
