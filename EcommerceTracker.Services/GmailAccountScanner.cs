namespace EcommerceTracker.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using EcommerceTracker.Domain.Models;
    using EcommerceTracker.DataAccess.Contexts;

    public class GmailAccountScanner : IEmailAccountScanner
    {
        private readonly GmailApiService _gmailService = new GmailApiService();
        private readonly EcommerceTrackerContext _context = new EcommerceTrackerContext();

        public List<EmailMessage> GetNewEmailMessages(int emailAccountId)
        {
            var existingMessages = _context.EmailMessages.Where(m => m.TrackedEmailAccountId == emailAccountId).ToList();

            var numberOfMessages = _gmailService.GetTotalMessages(emailAccountId);
            // If there are no new email messages, return empty list
            if (numberOfMessages == existingMessages.Count()) return new List<EmailMessage>();

            var allMessageIds = GetAllMessageIdentifiers(emailAccountId);

            return allMessageIds.Except(existingMessages.Select(e => e.Identifier)).Select(
                m => new EmailMessage
                {
                    Identifier = m,
                    TrackedEmailAccountId = emailAccountId,
                    Scanned = null
                }).ToList();
        }

        private IEnumerable<string> GetAllMessageIdentifiers(int emailAccountId)
        {
            var messages = _gmailService.GetAllMessages(emailAccountId).Select(m => m.Id.ToString()).ToList();

            return messages;
        }
    }
}
