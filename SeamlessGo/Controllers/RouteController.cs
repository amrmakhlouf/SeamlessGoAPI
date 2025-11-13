

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;
namespace SeamlessGo.Controllers


{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController : ControllerBase
    {
        public readonly IRouteRepository _routeRepository;
        private readonly IMapper _mapper;

        public RouteController (IRouteRepository routeRepository,IMapper mapper)
        {
            _routeRepository = routeRepository;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<RouteDto>> GetRoutes([FromQuery] int UserID)
        {
            try
            {
                var routes = await _routeRepository.GetAllAsync(UserID);

                if (routes == null )
                    return NotFound();
                var routedto = _mapper.Map<RouteDto>(routes);
                return Ok(routedto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving orders.", error = ex.Message });
            }
        }
    }
}
