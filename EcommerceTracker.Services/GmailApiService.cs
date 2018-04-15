namespace EcommerceTracker.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AutoMapper;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Auth.OAuth2.Responses;
    using Google.Apis.Gmail.v1;
    using Google.Apis.Gmail.v1.Data;
    using Google.Apis.Services;
    using EcommerceTracker.DataAccess.Contexts;

    public class GmailApiService
    {
        private readonly EcommerceTrackerContext _context = new EcommerceTrackerContext();

        public List<Message> GetAllMessages(int emailAccountId)
        {
            var service = GetGmailService(emailAccountId);
            var result = new List<Message>();
            var request = service.Users.Messages.List("me");
            do
            {
                try
                {
                    var response = request.Execute();
                    result.AddRange(response.Messages);
                    request.PageToken = response.NextPageToken;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("An error occurred: " + e.Message);
                }
            } while (!string.IsNullOrEmpty(request.PageToken));
             return result;
        }

        public int? GetTotalMessages(int emailAccountId)
        {
            var service = GetGmailService(emailAccountId);
            return service.Users.GetProfile("me")
                .Execute()
                .MessagesTotal;
        }

        private UserCredential GetUserCredential(int emailAccountId)
        {
            var token = _context.GoogleOauthTokenResponses.Single(oa => oa.TrackedEmailAccountId == emailAccountId);
            return new UserCredential(new AppFlowMetadata().Flow, "me", Mapper.Map<TokenResponse>(token));
        }

        private GmailService GetGmailService(int emailAccountId)
        {
            var credential = GetUserCredential(emailAccountId);

            // TODO: Make this a singleton?
            return new GmailService(
                    new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Ecommerce Tracker"
                    });

        }
    }
}
