using AutoMapper;
using FME.API.ViewModels.Genre;
using FME.Domain.Genre;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net;

namespace FME.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<GenreController> _logger;


        public GenreController(IGenreService service, IMapper mapper, ILogger<GenreController> logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }


        [HttpGet("GetGenre")]
        [ProducesResponseType(typeof(GenreViewModel), (int)HttpStatusCode.OK)]
        public ActionResult GetGenre([FromQuery] GenreFilterViewModel filter)
        {
            _logger.LogInformation("Get All Genre", null);
            var result = _service.GetGenreList(_mapper.Map<GenreFilter>(filter));
            return new JsonResult(_mapper.Map<List<GenreViewModel>>(result));
        }
    }
}
