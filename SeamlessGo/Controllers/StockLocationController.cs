using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;

namespace SeamlessGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class StockLocationController: ControllerBase
    {
        private readonly IStockLocationRepository _stockLocationRepository;
        private readonly IMapper _mapper;
        public StockLocationController(IStockLocationRepository stockLocationRepository, IMapper mapper)
        {
            _stockLocationRepository = stockLocationRepository;
            _mapper = mapper;


        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockLocationDto>>> GetAllAsync()
        {
            try
            {
                var stockLocations = await _stockLocationRepository.GetAllAsync();
                var stockLocationDtos = _mapper.Map<IEnumerable<StockLocationDto>>(stockLocations);
                return Ok(stockLocationDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving stock locations.", error = ex.Message });
            }
        }

    }
}
