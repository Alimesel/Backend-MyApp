using AutoMapper;
using MyApp.Controllers.Resources;
using MyApp.Core.Models;
using static MyApp.Controllers.Resources.UserResponse;


namespace MyApp.Mapping{
    public class MappingProfile : Profile{
        public MappingProfile()

        {  
         CreateMap<User,UserDTO>();     

        CreateMap<Product,ProductResources>()
        .ForMember(pr => pr.Category, opt=>opt.MapFrom(p => p.Category))
        .ForMember(pr => pr.ID,opt => opt.MapFrom(p => p.ID));

       CreateMap<Category, CategroyResources>()
            .ForMember(cr => cr.ID, opt => opt.MapFrom(c => c.CategoryID))
            .ForMember(cr => cr.name, opt => opt.MapFrom(c => c.CategoryName))
            .ForMember(cr => cr.Image,opt => opt.MapFrom(c => c.CategoryImage));

        }
    }
}