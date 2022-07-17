using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FME.Repository.Dao.Genre
{
    public class GenreDao
    {
        [JsonProperty("IdGenero")]
        public int IdGenre { get; set; }
        
        [JsonProperty("Nombre")]
        public string? Name { get; set; }
    }
}
