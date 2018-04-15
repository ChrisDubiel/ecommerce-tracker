namespace EcommerceTracker.Controllers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using AutoMapper;

    using DataAccess.Contexts;

    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Auth.OAuth2.Mvc;
    using Google.Apis.Auth.OAuth2.Responses;
    using Google.Apis.Gmail.v1;
    using Google.Apis.Services;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Domain.Models;
    using EcommerceTracker.Services;

    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private readonly EcommerceTrackerContext _context = new EcommerceTrackerContext();

        public AuthCallbackController()
        {
        }

        public AuthCallbackController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

            private set => _signInManager = value;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            private set => _userManager = value;
        }

        public override async Task<ActionResult> IndexAsync(
            AuthorizationCodeResponseUrl authorizationCode,
            CancellationToken taskCancellationToken)
        {
            if (string.IsNullOrEmpty(authorizationCode.Code))
            {
                var errorResponse = new TokenErrorResponse(authorizationCode);
                Logger.Info("Received an error. The response is: {0}", errorResponse);
                return OnTokenError(errorResponse);
            }

            var savedState = _context.GoogleOauthStates.Find(User.Identity.GetUserId());

            TokenResponse token = null;

            // TODO: Inverse ifs
            if (savedState != null)
            {
                if (authorizationCode.State.Equals(savedState.Value))
                {
                    var requestUrl = Request.Url;
                    if (requestUrl != null)
                    {
                        var url = requestUrl.ToString();
                        var redirectUrl = url.Substring(0, url.IndexOf("?", StringComparison.Ordinal));
                        token =
                            await Flow.ExchangeCodeForTokenAsync(
                                UserId,
                                authorizationCode.Code,
                                redirectUrl,
                                taskCancellationToken).ConfigureAwait(false);
                    }
                }

                _context.GoogleOauthStates.Remove(savedState);
                _context.SaveChanges();
            }

            if (token != null)
            {
                // TODO: Move all of this into a wrapper service
                var credential = new UserCredential(Flow, UserId, token);

                var gmailService =
                    new GmailService(
                        new BaseClientService.Initializer
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = "Ecommerce Tracker"
                            });
                var emailAddress = gmailService.Users.GetProfile("me").Execute();

                // Check if email address is already being tracked by this or another user
                if (_context.TrackedEmailAccounts.FirstOrDefault(t => t.EmailAddress == emailAddress.EmailAddress) == null)
                {
                    var emailAccountEntity = _context.TrackedEmailAccounts.Add(
                        new TrackedEmailAccount
                            {   
                                UserId = UserId,
                                EmailAddress = emailAddress.EmailAddress,
                                EmailAccountType = EmailAccountType.Gmail
                            });
                    _context.SaveChanges();

                    var tokenResponseEntity = Mapper.Map<GoogleOauthTokenResponse>(token);
                    tokenResponseEntity.TrackedEmailAccountId = emailAccountEntity.Id;
                    _context.GoogleOauthTokenResponses.Add(tokenResponseEntity);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Success! Your Gmail account is now being tracked.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Sorry, that Gmail account is already being tracked by you or someone else. Please track a different Gmail account.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Sorry, there was a problem authenticating that Gmail account. Please try again.";
            }

            return RedirectToAction("Index", "EmailAccounts");
        }

        protected override FlowMetadata FlowData => new AppFlowMetadata();
    }
}