using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FME.Domain.Genre
{
    public interface IGenreService
    {
        List<Genre> GetGenreList(GenreFilter filter);
    }
}
