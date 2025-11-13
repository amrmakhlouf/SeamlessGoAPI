using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;

namespace SeamlessGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DownPaymentController : ControllerBase
    {



        private readonly IDownPaymentsRepository _downpaymentRepository;
        public DownPaymentController(IDownPaymentsRepository downpaymentrepository)
        {
            _downpaymentRepository = downpaymentrepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DownPaymentDTO>>> GetDownPayments([FromQuery]DateTime? LastModifiedUtc)
        {
            try
            {
                var Downpayments = await _downpaymentRepository.GetAllAsync(LastModifiedUtc);
                var Downpaymentdto = Downpayments.Select(MapDownPaymentToDto);
                return Ok(Downpaymentdto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving orders.", error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<ActionResult<DownPaymentDTO>> CreateDownPayment(DownPaymentDTO createDownPaymentDto)
        {
            try
            {
                var downPayment = new DownPayment
                {
                    DownPaymentID = createDownPaymentDto.DownPaymentID,
                    Amount = createDownPaymentDto.Amount,
                    ChequeID = createDownPaymentDto.ChequeID,
                    CreatedByUserID = createDownPaymentDto.CreatedByUserID,
                    CreatedDate = createDownPaymentDto.CreatedDate ?? DateTime.Now,
                    CreatedFromPaymentID = createDownPaymentDto.CreatedFromPaymentID,
                    CurrencyID = createDownPaymentDto.CurrencyID,
                    CustomerID = createDownPaymentDto.CustomerID,
                    PaymentMethod = createDownPaymentDto.PaymentMethod,
                    PaymentStatus = createDownPaymentDto.PaymentStatus,
                    RemainingAmount = createDownPaymentDto.RemainingAmount,
                    RouteID = createDownPaymentDto.RouteID,
                    SyncStatus = createDownPaymentDto.SyncStatus,
                    UpdatedDate = createDownPaymentDto.UpdatedDate,
                    IsVoided = createDownPaymentDto.IsVoided,
                    ClientID = createDownPaymentDto.ClientID,
                    LastModifiedUtc = createDownPaymentDto.LastModifiedUtc ?? DateTime.UtcNow
                };

                // Convert CreateDownPaymentAllocationDto to DownPaymentAllocation
                List<DownPaymentAllocations>? downPaymentAllocations = null;
                if (createDownPaymentDto.Allocations != null && createDownPaymentDto.Allocations.Any())
                {
                    downPaymentAllocations = createDownPaymentDto.Allocations.Select(dto => new DownPaymentAllocations
                    {
                        PaymentID = dto.PaymentID,
                        AllocatedAmount = dto.AllocatedAmount,
                        SyncStatus = dto.SyncStatus
                    }).ToList();
                }

                var createdpayment = await _downpaymentRepository.CreateAsync(downPayment, downPaymentAllocations);

                var fullpayment = await _downpaymentRepository.GetByIdAsync(createDownPaymentDto.DownPaymentID);

                if (fullpayment == null)
                {
                    return StatusCode(500, new { message = "Order created but could not retrieve details." });
                }

                var downPaymentDto = MapDownPaymentToDto(downPayment);
                return CreatedAtAction(nameof(CreateDownPayment), new { id = downPayment.DownPaymentID }, downPaymentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private static DownPaymentDTO MapDownPaymentToDto(DownPayment downPayment)
        {
            return new DownPaymentDTO
            {
                DownPaymentID = downPayment.DownPaymentID,
                Amount = downPayment.Amount,
                ChequeID = downPayment.ChequeID,
                CreatedByUserID = downPayment.CreatedByUserID,
                CreatedDate = downPayment.CreatedDate,
                CreatedFromPaymentID = downPayment.CreatedFromPaymentID,
                CurrencyID = downPayment.CurrencyID,
                CustomerID = downPayment.CustomerID,
                PaymentMethod = downPayment.PaymentMethod,
                PaymentStatus = downPayment.PaymentStatus,
                RemainingAmount = downPayment.RemainingAmount,
                RouteID = downPayment.RouteID,
                SyncStatus = downPayment.SyncStatus,
                UpdatedDate = downPayment.UpdatedDate,
                IsVoided = downPayment.IsVoided,
                ClientID = downPayment.ClientID,
                LastModifiedUtc = downPayment.LastModifiedUtc,
                Allocations = downPayment.Allocations?.Select(allocation => new DownPaymentAllocationsDTO
                {
                    PaymentID = allocation.PaymentID,
                    AllocatedAmount = allocation.AllocatedAmount,
                    SyncStatus = allocation.SyncStatus
                }).ToList()
            };
        }
    }
}
