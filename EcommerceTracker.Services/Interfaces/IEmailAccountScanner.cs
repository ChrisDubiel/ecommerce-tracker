namespace EcommerceTracker.Services.Interfaces
{
    using EcommerceTracker.Domain.Models;
    using System.Collections.Generic;

    public interface IEmailAccountScanner
    {
        List<EmailMessage> GetNewEmailMessages(int emailAccountId);
    }
}
