

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;
using System.Numerics;
namespace SeamlessGo.Controllers


{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController : ControllerBase
    {
        public readonly IRouteRepository _routeRepository;
        private readonly IMapper _mapper;

        public RouteController(IRouteRepository routeRepository, IMapper mapper)
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

                if (routes == null)
                    return NotFound();
                var routedto = _mapper.Map<RouteDto>(routes);

                return Ok(new
                {
                    Routeinfo = new
                    {
                        RouteID = routedto.RouteID,
                        PlanID = routedto.PlanID,
                        UserID = routedto.UserID,
                        StartDate = routedto.StartDate,
                        EndDate = routedto.EndDate,
                        Status = routedto.Status,

                    },
                    PlanInfo = new
                    {
                        PlanID = routedto.Plan.PlanID,
                        PlanName = routedto.Plan.PlanName,
                        PlanDesc = routedto.Plan.PlanDesc,
                        CreatedDate = routedto.Plan.CreatedDate,
                        UpdatedDate = routedto.Plan.UpdatedDate,
                        CreatedByUserID = routedto.Plan.CreatedByUserID,
                        PlanUserID = routedto.Plan.PlanUserID

                    }
                }); 


            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving orders.", error = ex.Message });
            }
        }
        [HttpPatch]
        public async Task<ActionResult> UpdateRouteStatus([FromQuery] int RouteID, [FromQuery] byte Status, [FromQuery] DateTime EndDate)
        {
            try
            {
                var result = await _routeRepository.UpdateStatusAsync(RouteID, Status, EndDate);

                if (!result)
                {
                    return NotFound(new { message = "Route not found." });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating route status.", error = ex.Message });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> CreateNewVisit([FromQuery] int RouteID)
        //{
        //    var result = _routeRepository.CreateNewVisit(RouteID);
        //}
    }
}
