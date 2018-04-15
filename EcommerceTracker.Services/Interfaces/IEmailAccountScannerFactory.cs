using EcommerceTracker.Domain.Models;

namespace EcommerceTracker.Services.Interfaces
{
    internal interface IEmailAccountScannerFactory
    {
        IEmailAccountScanner GetScanner(EmailAccountType emailAccountType);
    }
}
