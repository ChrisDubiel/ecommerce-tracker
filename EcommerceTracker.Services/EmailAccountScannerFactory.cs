namespace EcommerceTracker.Services
{
    using EcommerceTracker.Domain.Models;
    using Interfaces;

    public class EmailAccountScannerFactory : IEmailAccountScannerFactory
    {
        public IEmailAccountScanner GetScanner(EmailAccountType emailAccountType)
        {
            switch (emailAccountType)
            {
                case EmailAccountType.Gmail:
                    return new GmailAccountScanner();
                default:
                    return null;
            }
        }
    }
}
