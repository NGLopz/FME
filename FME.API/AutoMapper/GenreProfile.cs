using AutoMapper;
using FME.API.ViewModels.Genre;
using FME.Domain.Genre;

namespace FME.API.AutoMapper
{
    public class GenreProfile : Profile
    {
        public GenreProfile()
        {
            _ = CreateMap<GenreFilter, GenreFilterViewModel>().ReverseMap();
        }
    }
}
