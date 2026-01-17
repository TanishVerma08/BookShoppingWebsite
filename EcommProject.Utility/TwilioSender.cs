using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;
using Twilio.Types;

namespace EcommProject.Utility
{
    public class TwilioSender : ITwilioSender
    {
        private readonly TwilioSettings _twilioSettings;
        public TwilioSender(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
            TwilioClient.Init(_twilioSettings.AccountSID,_twilioSettings.AuthToken);
        }
        public async Task SendVerificationCodeAsync(string phoneNumber, string channel)
        {
            await VerificationResource.CreateAsync(
                to:phoneNumber,
                channel: channel,
                pathServiceSid:_twilioSettings.VerifyServiceSID
                );
        }

        public async Task<bool> VerifyCodeAsync(string phoneNumber, string code)
        {
            var verificationCheck = await VerificationCheckResource.CreateAsync(
                to:phoneNumber,
                code:code,
                pathServiceSid:_twilioSettings.VerifyServiceSID
                );
            return verificationCheck.Status=="approved";
        }
        public async Task SendOrderPlacedAsync(string phoneNumber)
        {
            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(_twilioSettings.From),
                body: "New Order Received!!! For More Details please check your dashboard."
                );
        }

        public async Task SendOrderPlacedCallAsync(string phoneNumber)
        {
            var call = await CallResource.CreateAsync(
                to:new PhoneNumber(phoneNumber),
                 from: new PhoneNumber(_twilioSettings.From),
                twiml:new Twiml("<Response><Say>Your order has been received. Please check your dashboard for more details.</Say></Response>")
                );
        }
    }
}
