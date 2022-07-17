using AutoMapper;
using FME.Domain.Genre;
using FME.Repository.Dao.Genre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FME.Repository.AutoMapper
{
    public class GenreProfile : Profile
    {
        public GenreProfile()
        {
            _ = CreateMap<Genre, GenreDao>().ReverseMap();
        }
    }
}
