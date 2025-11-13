using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;

namespace SeamlessGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
      
        private readonly IPaymentsRepository _paymnetsRepository;
        public PaymentsController(IPaymentsRepository paymentrepository)
        {
            _paymnetsRepository = paymentrepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetPayments([FromQuery] DateTime? LastModifiedUtc)
        {
            try
            {
                var payments = await _paymnetsRepository.GetAllAsync(LastModifiedUtc);
                var paymentdto = payments.Select(MapPaymentToDto);
                return Ok(paymentdto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving orders.", error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<ActionResult<PaymentDTO>> CreatePayment(PaymentDTO createPaymentDto)
        {
            try
            {
                var payment = new Payments
                {
                    PaymentID = createPaymentDto.PaymentID,
                    Amount = createPaymentDto.Amount,
                    ChequeID = createPaymentDto.ChequeID,
                    CreatedByUserID = createPaymentDto.CreatedByUserID,
                    CreatedDate = createPaymentDto.CreatedDate ?? DateTime.Now,
                    CurrencyID = createPaymentDto.CurrencyID,
                    CustomerID = createPaymentDto.CustomerID,
                    PaymentMethod = createPaymentDto.PaymentMethod,
                    PaymentStatus = createPaymentDto.PaymentStatus,
                    RouteID = createPaymentDto.RouteID,
                    SyncStatus = createPaymentDto.SyncStatus,
                    UpdatedDate = createPaymentDto.UpdatedDate,
                    IsVoided = createPaymentDto.IsVoided,
                    ClientID = createPaymentDto.ClientID,
                    LastModifiedUtc = createPaymentDto.LastModifiedUtc ?? DateTime.UtcNow
                };

                // Convert CreatePaymentAllocationDto to PaymentAllocation
                List<PaymentAllocations>? paymentAllocations = null;
                if (createPaymentDto.Allocations != null && createPaymentDto.Allocations.Any())
                {
                    paymentAllocations = createPaymentDto.Allocations.Select(dto => new PaymentAllocations
                    {
                        TransactionID = dto.TransactionID,
                        AllocatedAmount = dto.AllocatedAmount,
                        SyncStatus = dto.SyncStatus
                    }).ToList();
                }

                var createdpayment = await _paymnetsRepository.CreateAsync(payment, paymentAllocations);

                var fullpayment = await _paymnetsRepository.GetByIdAsync(createPaymentDto.PaymentID);

                if (fullpayment == null)
                {
                    return StatusCode(500, new { message = "Order created but could not retrieve details." });
                }

                var paymentDto = MapPaymentToDto(payment);
                return CreatedAtAction(nameof(CreatePayment), new { id = payment.PaymentID }, paymentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private static PaymentDTO MapPaymentToDto(Payments payment)
        {
            return new PaymentDTO
            {
                PaymentID = payment.PaymentID,
                Amount = payment.Amount,
                ChequeID = payment.ChequeID,
                CreatedByUserID = payment.CreatedByUserID,
                CreatedDate = payment.CreatedDate,
                CurrencyID = payment.CurrencyID,
                CustomerID = payment.CustomerID,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                RouteID = payment.RouteID,
                SyncStatus = payment.SyncStatus,
                UpdatedDate = payment.UpdatedDate,
                IsVoided = payment.IsVoided,
                ClientID = payment.ClientID,
                LastModifiedUtc = payment.LastModifiedUtc,
                Allocations = payment.Allocations?.Select(allocation => new PaymentAllocationsDTO
                {
                    TransactionID = allocation.TransactionID,
                    AllocatedAmount = allocation.AllocatedAmount,
                    SyncStatus = allocation.SyncStatus
                }).ToList()
            };
        }
    }
}
