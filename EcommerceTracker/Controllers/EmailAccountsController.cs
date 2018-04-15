namespace EcommerceTracker.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Web.Mvc;

    using DataAccess.Contexts;
    using Domain.Models;
    using Extensions;
    using Hangfire;
    using Microsoft.AspNet.Identity;
    using Newtonsoft.Json;
    using Services;

    [Authorize]
    public class EmailAccountsController : Controller
    {
        private readonly EcommerceTrackerContext _context = new EcommerceTrackerContext();
        private readonly EmailAccountScannerService _emailAccountScannerService = new EmailAccountScannerService();

        // GET: EmailAddresses
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            TempData.TryGetValue("ErrorMessage", out object errorMessage);
            if (errorMessage != null)
            {
                ViewBag.ErrorMessage = errorMessage.ToString();
            }

            TempData.TryGetValue("SuccessMessage", out object successMessage);
            if (successMessage != null)
            {
                ViewBag.StatusMessage = successMessage.ToString();
            }

            return View(_context.TrackedEmailAccounts.Where(e => e.UserId == userId).ToList());
        }

        [HttpPost]
        public JsonResult Scan(string emailAddress)
        {
            var userId = User.Identity.GetUserId();
            var emailAccount =
                _context.TrackedEmailAccounts.SingleOrDefault(
                    e => e.EmailAddress == emailAddress && e.UserId == userId);

            string message;
            if (emailAccount != null)
            {
                BackgroundJob.Enqueue(() => _emailAccountScannerService.Scan(emailAccount.Id));
                emailAccount.LastScanned = DateTime.Now;
                _context.SaveChanges();
                message = JsonConvert.SerializeObject(emailAccount);
            }
            else
            {
                message = "Sorry, an error occured.";
            }

            return Json(message);
        }

        // GET: EmailAddresses/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmailAddresses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,EmailAddress,EmailAccountType")] TrackedEmailAccount trackedEmailAddress)
        {
            if (!ModelState.IsValid) return View(trackedEmailAddress);
            _context.TrackedEmailAccounts.Add(trackedEmailAddress);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult AuthorizeGoogle(CancellationToken cancellationToken)
        {
            var authorizationUrl = new GoogleAuthorizationCodeService(this, new AppFlowMetadata())
                .RedirectToGoogleAuthorizationPage();

            return Redirect(authorizationUrl);
        }

        // GET: EmailAddresses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var trackedEmailAddress = _context.TrackedEmailAccounts.Find(id);
            if (trackedEmailAddress == null)
            {
                return HttpNotFound();
            }

            return View(trackedEmailAddress);
        }

        // POST: EmailAddresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var trackedEmailAddress = _context.TrackedEmailAccounts.Find(id);
            if (trackedEmailAddress == null)
            {
                return HttpNotFound();
            }

            trackedEmailAddress.RemoveAuthorization(_context);
            _context.TrackedEmailAccounts.Remove(trackedEmailAddress);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
