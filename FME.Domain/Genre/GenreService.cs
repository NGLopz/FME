using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FME.Domain.Genre
{
    public class GenreService : IGenreService
    {
        private ILogger<GenreService> _logger;
        private IGenreRepository _repository;

        public GenreService(ILogger<GenreService> logger, IGenreRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public List<Genre> GetGenreList(GenreFilter filter)  =>  _repository.GetGenreList(filter);
        
    }
}
