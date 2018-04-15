namespace EcommerceTracker.Services
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Google.Apis.Auth.OAuth2.Flows;
    using Google.Apis.Auth.OAuth2.Mvc;
    using EcommerceTracker.DataAccess.Contexts;
    using System.Data.Entity.Migrations;
    using EcommerceTracker.Domain.Models;

    public class GoogleAuthorizationCodeService
    {
        private readonly EcommerceTrackerContext _context = new EcommerceTrackerContext();

        private IAuthorizationCodeFlow Flow { get; set; }

        private string RedirectUri { get; set; }

        private string State { get; set; }

        private string UserId { get; set; }

        public GoogleAuthorizationCodeService(Controller controller, FlowMetadata flowData){
            
            Flow = flowData.Flow;
            RedirectUri = new Uri($"{controller.Request.Url?.GetLeftPart(UriPartial.Authority)}{flowData.AuthCallback}").ToString();
            State = controller.Request.Url?.ToString();
            UserId = flowData.GetUserId(controller);
        }

        public string RedirectToGoogleAuthorizationPage()
        {
            var codeRequest = Flow.CreateAuthorizationCodeRequest(RedirectUri);

            // TODO: Use a more cryptographically safe nonce generator
            var oauthState = $"{State}{new Random().Next(int.Parse(new string('9', 8))).ToString("D" + 8)}";

            // TODO: What should happen if an OAuth state is already in the database? 
            // TODO: For now, upsert
            _context.GoogleOauthStates.AddOrUpdate(
                new GoogleOauthState { UserId = UserId, Value = oauthState });

            // TODO: Make sure the code request URL can be built before saving the state in the database
            _context.SaveChanges();
            codeRequest.State = oauthState;

            // HACK: Needed to add "&prompt=select_account" to force Google account selection screen
            return $"{codeRequest.Build()}&prompt=select_account+consent";
        }
    }
}