namespace EcommerceTracker.Extensions
{
    using System.Linq;
    using DataAccess.Contexts;
    using Domain.Models;

    public static class TrackedEmailAddressExtensions
    {
        public static void RemoveAuthorization(
            this TrackedEmailAccount trackedEmailAccount,
            EcommerceTrackerContext context)
        {
            if (trackedEmailAccount.EmailAccountType != EmailAccountType.Gmail)
            {
                return;
            }

            var googleOauthTokenResponse =
                context.GoogleOauthTokenResponses.Single(t => t.TrackedEmailAccountId == trackedEmailAccount.Id);
            context.GoogleOauthTokenResponses.Remove(googleOauthTokenResponse);
        }
    }
}