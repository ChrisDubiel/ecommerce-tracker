namespace EcommerceTracker.DataAccess.Contexts
{
    using System.Collections.Generic;
    using Domain.Models;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class EcommerceTrackerContext : IdentityDbContext<ApplicationUser>
    {
        public EcommerceTrackerContext() 
            : base("EcommerceTrackerContext", false)
        {
        }

        public static EcommerceTrackerContext Create()
        {
            return new EcommerceTrackerContext();
        }
        
        public DbSet<GoogleOauthState> GoogleOauthStates { get; set; }
        public DbSet<TrackedEmailAccount> TrackedEmailAccounts { get; set; }
        public DbSet<GoogleOauthTokenResponse> GoogleOauthTokenResponses { get; set; }
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<TrackedPurchaseFile> TrackedPurchaseFiles { get; set; }
        public DbSet<SuggestedCategoryName> SuggestedCategoryNames { get; set; }
        public DbSet<SuggestedParentCategory> SuggestedParentCategories { get; set; }
        public DbSet<SuggestedNecessityValue> SuggestedNecessityValues { get; set; }
        public DbSet<Alert> Alerts { get; set; }
    }

    public class EcommerceTrackerInitializer : DropCreateDatabaseIfModelChanges<EcommerceTrackerContext>
    {
        protected override void Seed(EcommerceTrackerContext context)
        {
            SeedAdminUserAndRoles(context);
            SeedSuggestions(context);

            base.Seed(context);
        }

        private static void SeedAdminUserAndRoles(DbContext context)
        {
            const string adminRoleName = "Admin";
            const string adminEmailAddress = "admin@admin.com";
            const string adminPassword = "Password1!";

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var adminUser = new ApplicationUser
            {
                UserName = adminEmailAddress,
                Email = adminEmailAddress
            };
            if (!roleManager.RoleExists(adminRoleName))
                roleManager.Create(new IdentityRole(adminRoleName));
            var adminUserResult = userManager.Create(adminUser, adminPassword);
            if (adminUserResult.Succeeded)
                userManager.AddToRole(adminUser.Id, adminRoleName);
        }

        private static void SeedSuggestions(EcommerceTrackerContext context)
        {
            // TODO: Check if files exist
            const string folderName = @"C:\EcommerceTracker\SeedFiles\";
            var serializerSettings =
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

            var suggestedCategoryNamesJson = File.ReadAllText($"{folderName}\\SuggestedCategoryNameSeed.json");
            var suggestedCategoryNames = 
                JsonConvert.DeserializeObject<List<SuggestedCategoryName>>(
                    suggestedCategoryNamesJson, serializerSettings).Distinct();
            context.SuggestedCategoryNames.AddRange(suggestedCategoryNames);
            context.SaveChanges();

            var suggestedParentCategoriesJson = File.ReadAllText($"{folderName}\\SuggestedParentCategorySeed.json");
            var suggestedParentCategories =
                JsonConvert.DeserializeObject<List<SuggestedParentCategory>>(suggestedParentCategoriesJson,
                    serializerSettings).Distinct();
            context.SuggestedParentCategories.AddRange(suggestedParentCategories);
            context.SaveChanges();

            var suggestedNecessityValuesJson = File.ReadAllText($"{folderName}\\SuggestedNecessityValueSeed.json");
            var suggestedNecessityValues =
                JsonConvert.DeserializeObject<List<SuggestedNecessityValue>>(suggestedNecessityValuesJson,
                    serializerSettings).Distinct();
            context.SuggestedNecessityValues.AddRange(suggestedNecessityValues);
            context.SaveChanges();
        }
    }
}