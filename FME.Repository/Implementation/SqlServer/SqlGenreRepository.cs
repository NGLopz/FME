using AutoMapper;
using FME.Domain.Genre;
using FME.Repository.Dao.Genre;
using FME.Repository.Interfaces;
using FME.Repository.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FME.Repository.Implementation.SqlServer
{
    public class SqlGenreRepository : IGenreRepository
    {
        private readonly ILogger<SqlGenreRepository> _logger;
        private readonly IConnector _connection;
        private readonly IMapper _mapper;


        public SqlGenreRepository(ILogger<SqlGenreRepository> logger, IConnector connection, IMapper mapper)
        {
            _logger = logger;
            _connection = connection;
            _mapper = mapper;
        }

        public List<Genre> GetGenreList(GenreFilter filter)
        {
            _logger.LogInformation("GetGenreList repository was called", null);
            var daoFilter = _mapper.Map<GenreFilterDao>(filter);
            var json = _connection.GetJson("[SRM].[SpRecepcionesComprasConsultarVisorMovil]", JObject.FromObject(daoFilter));
            var dao = JsonUtils.DeserializeObjectOrDefault(json, new List<GenreDao>());
            var domain = _mapper.Map<List<Genre>>(dao).ToList();
            return domain;
        }
    }
}
