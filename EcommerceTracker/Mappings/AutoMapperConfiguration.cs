namespace EcommerceTracker.Mappings
{
    using AutoMapper;
    using Domain.Models;
    using Extensions;
    using Google.Apis.Auth.OAuth2.Responses;
    using ViewModels;

    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(
                cfg =>
                {
                    cfg.CreateMap<TokenResponse, GoogleOauthTokenResponse>();
                    cfg.CreateMap<GoogleOauthTokenResponse, TokenResponse>();
                    cfg.CreateMap<Purchase, PurchaseViewModel>()
                        .ForMember(vm => vm.NecessityDescription,
                            x => x.MapFrom(m => m.GetNecessityValue().ToDescription()))
                        .ForMember(vm => vm.ParentCategoryName,
                            x => x.MapFrom(m => m.Category.ParentCategory.Name));
                    cfg.CreateMap<PurchaseViewModel, Purchase>();
                    cfg.CreateMap<Purchase, EditPurchaseViewModel>();
                    cfg.CreateMap<Category, CategoryViewModel>()
                        .ForMember(vm => vm.NecessityDescription,
                            x => x.MapFrom(m => m.GetNecessityValue().ToDescription()))
                        .ForMember(vm => vm.ParentCategoryName,
                            x => x.MapFrom(m => m.ParentCategory.Name));
                    cfg.CreateMap<Category, EditCategoryViewModel>();
                    cfg.CreateMap<EditCategoryViewModel, Category>();
                });
        }
    }
}