using AutoMapper;
using BookVerse.Models;
using BookVerse.Models.ViewModels;

namespace BookVerse.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ── Category ─────────────────────────────────────────────────────
            CreateMap<Category, CategoryVM>().ReverseMap();

            // ── Company ──────────────────────────────────────────────────────
            CreateMap<Company, CompanyVM>().ReverseMap();

            // ── Product → ProductVM (flat, for Index / Details / Delete) ─────
            CreateMap<Product, ProductVM>()
                .ForMember(dest => dest.ImageUrl,
                    opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null));

            // ── ProductVM → Product ──────────────────────────────────────────
            CreateMap<ProductVM, Product>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore());

            // NOTE: OrderVM, ShoppingCartVM, ProductUpsertVM all wrap the raw
            // domain models directly — no AutoMapper mappings needed for them.
        }
    }
}