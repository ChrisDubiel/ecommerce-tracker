namespace EcommerceTracker.Services
{
    using EcommerceTracker.DataAccess.Contexts;
    using EcommerceTracker.Domain.Models;
    using System.Linq;

    public class EmailAccountScannerService
    {
        private readonly EcommerceTrackerContext _context = new EcommerceTrackerContext();

        private TrackedEmailAccount _emailAccount;

        public void Scan(int accountId)
        {
            _emailAccount = _context.TrackedEmailAccounts.SingleOrDefault(e => e.Id == accountId);
            if (_emailAccount == null) return;

            ScanAccountForNewMessages();
            CreateNewBatches();
        }

        private void ScanAccountForNewMessages()
        {
            if (_emailAccount == null) return;

            var scannerFactory = new EmailAccountScannerFactory();
            var emailAccountScanner = scannerFactory.GetScanner(_emailAccount.EmailAccountType);
            if (emailAccountScanner == null) return;

            var newMessages = emailAccountScanner.GetNewEmailMessages(_emailAccount.Id);
            if (newMessages.Count == 0) return;
            _context.EmailMessages.AddRange(newMessages);
            _context.SaveChanges();
        }

        private void CreateNewBatches()
        {
            if (_emailAccount == null) return;
            const int batchSize = 100;

            var nextBatch =
                _context.EmailMessages.Where(m => m.TrackedEmailAccountId == _emailAccount.Id && m.Scanned == false).Take(batchSize).ToList();




        }
    }
}
