using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;
using System.Drawing;
using System.Net;
using System.Security.Cryptography.X509Certificates;
namespace SeamlessGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class SequenceController : ControllerBase
    {
        private readonly ISequenceRepository _sequenceRepository;
        private readonly IMapper _mapper;

        public SequenceController(ISequenceRepository sequenceRepository, IMapper mapper)
        {
            _sequenceRepository = sequenceRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SequenceDto>>> GetSequencesAsync([FromQuery] int UserID)
        {

            var sequences = await _sequenceRepository.GetAllAsync(UserID);
            if (sequences == null || !sequences.Any())
                return NotFound();


            return Ok(_mapper.Map<IEnumerable<SequenceDto>>(sequences));

        }
    }
}