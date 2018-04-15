namespace EcommerceTracker.Services
{
    using System.Web.Mvc;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Auth.OAuth2.Flows;
    using Google.Apis.Auth.OAuth2.Mvc;
    using Google.Apis.Gmail.v1;
    using Microsoft.AspNet.Identity;
    using DataflowService = Google.Apis.Dataflow.v1b3.DataflowService;

    public class AppFlowMetadata : FlowMetadata
    {
        public override string GetUserId(Controller controller)
        {
            return controller.User.Identity.GetUserId();
        }

        public override IAuthorizationCodeFlow Flow => flow;

        private static readonly IAuthorizationCodeFlow flow =
            new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                    {
                        // TODO: Get client secrets from a file or environment variable
                        ClientSecrets =
                            new ClientSecrets
                                {
                                    ClientId =
                                        "556531640287-chj3iathok06jfv305s9kb4ndj3bnjq1.apps.googleusercontent.com",
                                    ClientSecret =
                                        "iz07YDVbC01t3o4Hak-2mmFZ"
                            },
                        Scopes =
                            new[]
                                {
                                    DataflowService.Scope.UserinfoEmail,
                                    GmailService.Scope.GmailReadonly
                                }
                    });
    }
}