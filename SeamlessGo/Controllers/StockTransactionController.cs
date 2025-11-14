using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;
namespace SeamlessGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class StockTransactionController : ControllerBase
    {
        private readonly IStockTransactionRepository _StockTransactionRepository;

        public StockTransactionController(IStockTransactionRepository StockTransactionRepository)
        {
            _StockTransactionRepository = StockTransactionRepository;
        }

        // GET: api/StockTransactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockTransactionDTO>>> GetStockTransactions([FromQuery] DateTime? LastModifiedUtc)
        {
            try
            {
                var StockTransactions = await _StockTransactionRepository.GetAllAsync(LastModifiedUtc);
                var StockTransactionDtos = StockTransactions.Select(MapStockTransactionToDto);
                return Ok(StockTransactionDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving StockTransactions.", error = ex.Message });
            }
        }

        // GET: api/StockTransactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StockTransactionDTO>> GetStockTransaction(string id)
        {
            try
            {
                var StockTransaction = await _StockTransactionRepository.GetByIdWithLinesAsync(id);

                if (StockTransaction == null)
                {
                    return NotFound(new { message = $"StockTransaction with ID {id} not found." });
                }

                return Ok(MapStockTransactionToDto(StockTransaction));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving StockTransaction.", error = ex.Message });
            }
        }

        // POST: api/StockTransactions
        [HttpPost]
        public async Task<ActionResult<StockTransactionDTO>> CreateStockTransaction(CreateStockTransactionDTO createStockTransactionDto)
        {
            try
            {
                var StockTransaction = new StockTransaction
                {
                    StockTransactionID = createStockTransactionDto.StockTransactionID,
                    SupplierID = createStockTransactionDto.SupplierID,
                    TransactionDate = createStockTransactionDto.TransactionDate ?? DateTime.Now,
                    StockTransactionTypeID = createStockTransactionDto.StockTransactionTypeID,
                    SubTotal = createStockTransactionDto.SubTotal,
                    TotalAmount = createStockTransactionDto.TotalAmount,
                    GrossAmount = createStockTransactionDto.GrossAmount,
                    TotalRemainingAmount = createStockTransactionDto.TotalRemainingAmount,
                    DiscountAmount = createStockTransactionDto.DiscountAmount,
                    DiscountPerc = createStockTransactionDto.DiscountPerc,
                    TotalQuantity = createStockTransactionDto.TotalQuantity,
                    UpdateDate = createStockTransactionDto.UpdateDate,
                    SupplierInvoiceNumber = createStockTransactionDto.SupplierInvoiceNumber,
                    DeliveryDate = createStockTransactionDto.DeliveryDate,
                    DeliveryLocationID = createStockTransactionDto.DeliveryLocationID,

                    ShippingCost = createStockTransactionDto.ShippingCost,

                    ImportDuty = createStockTransactionDto.ImportDuty,
                    Status = createStockTransactionDto.Status,
                    Note = createStockTransactionDto.Note,
                    RouteID = createStockTransactionDto.RouteID,

                    CreatedByUserID = createStockTransactionDto.CreatedByUserID,


                };

                // Convert CreateStockTransactionLineDto to StockTransactionLine
                List<StockTransactionLine>? StockTransactionLines = null;
                if (createStockTransactionDto.StockTransactionLine != null && createStockTransactionDto.StockTransactionLine.Any())
                {
                    StockTransactionLines = createStockTransactionDto.StockTransactionLine.Select(dto => new StockTransactionLine
                    {
                         
        StockTransactionLineID = dto.StockTransactionLineID,
                        ItemPackID = dto.ItemPackID,
                        Quantity = dto.Quantity,
                        Coast = dto.Coast,
                        TotalCost = dto.TotalCost,
                        ExpirationDate = dto.ExpirationDate

                       
                    }).ToList();
                }

                var createdStockTransaction = await _StockTransactionRepository.CreateAsync(StockTransaction, StockTransactionLines);

                var StockTransactionWithLines = await _StockTransactionRepository.GetByIdWithLinesAsync(createdStockTransaction.StockTransactionID);

                if (StockTransactionWithLines == null)
                {
                    return StatusCode(500, new { message = "StockTransaction created but could not retrieve details." });
                }

                var StockTransactionDto = MapStockTransactionToDto(StockTransactionWithLines);
                return CreatedAtAction(nameof(GetStockTransaction), new { id = createdStockTransaction.StockTransactionID }, StockTransactionDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the StockTransaction.", error = ex.Message });
            }
        }

        // PUT: api/StockTransactions/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateStockTransaction(int id, UpdateStockTransactionDto updateStockTransactionDto)
        //{
        //    try
        //    {
        //        var StockTransaction = new StockTransactions
        //        {
        //            CustomerID = updateStockTransactionDto.CustomerID,
        //            StockTransactionDate = updateStockTransactionDto.StockTransactionDate,
        //            StockTransactionTypeID = updateStockTransactionDto.StockTransactionTypeID,
        //            DiscountAmount = updateStockTransactionDto.DiscountAmount,
        //            DiscountPerc = updateStockTransactionDto.DiscountPerc,
        //            Tax = updateStockTransactionDto.Tax,
        //            TaxPerc = updateStockTransactionDto.TaxPerc,
        //            Status = updateStockTransactionDto.Status,
        //            RouteID = updateStockTransactionDto.RouteID,
        //            IsVoided = updateStockTransactionDto.IsVoided,
        //            Note = updateStockTransactionDto.Note
        //        };

        //        var success = await _StockTransactionRepository.UpdateAsync(id, StockTransaction);

        //        if (!success)
        //        {
        //            return NotFound(new { message = $"StockTransaction with ID {id} not found." });
        //        }

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while updating the StockTransaction.", error = ex.Message });
        //    }
        //}

        // DELETE: api/StockTransactions/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteStockTransaction(int id)
        //{
        //    try
        //    {
        //        var success = await _StockTransactionRepository.DeleteAsync(id);

        //        if (!success)
        //        {
        //            return NotFound(new { message = $"StockTransaction with ID {id} not found." });
        //        }

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while deleting the StockTransaction.", error = ex.Message });
        //    }
        //}

        // GET: api/StockTransactions/search?customerId=CUST001
        //[HttpGet("search")]
        //public async Task<ActionResult<IEnumerable<StockTransactionDtocs>>> SearchStockTransactions([FromQuery] string? customerId)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(customerId))
        //        {
        //            return BadRequest(new { message = "Customer ID is required for search." });
        //        }

        //        var StockTransactions = await _StockTransactionRepository.GetStockTransactionsByCustomerAsync(customerId);
        //        var StockTransactionDtos = StockTransactions.Select(MapStockTransactionToDto);
        //        return Ok(StockTransactionDtos);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while searching StockTransactions.", error = ex.Message });
        //    }
        //    }

        private static StockTransactionDTO MapStockTransactionToDto(StockTransaction StockTransaction)
        {
            return new StockTransactionDTO
            {

                StockTransactionID = StockTransaction.StockTransactionID,
                SupplierID = StockTransaction.SupplierID,
                TransactionDate = StockTransaction.TransactionDate ?? DateTime.Now,
                StockTransactionTypeID = StockTransaction.StockTransactionTypeID,
                SubTotal = StockTransaction.SubTotal,
                TotalAmount = StockTransaction.TotalAmount,
                GrossAmount = StockTransaction.GrossAmount,
                TotalRemainingAmount = StockTransaction.TotalRemainingAmount,
                DiscountAmount = StockTransaction.DiscountAmount,
                DiscountPerc = StockTransaction.DiscountPerc,
                TotalQuantity = StockTransaction.TotalQuantity,
                UpdateDate = StockTransaction.UpdateDate,
                SupplierInvoiceNumber = StockTransaction.SupplierInvoiceNumber,
                DeliveryDate = StockTransaction.DeliveryDate,
                DeliveryLocationID = StockTransaction.DeliveryLocationID,

                ShippingCost = StockTransaction.ShippingCost,

                ImportDuty = StockTransaction.ImportDuty,
                Status = StockTransaction.Status,
                Note = StockTransaction.Note,
                RouteID = StockTransaction.RouteID,

                CreatedByUserID = StockTransaction.CreatedByUserID,


                StockTransactionLine = StockTransaction.StockTransactionLine?.Select(line => new StockTransactionLinesDTO
                {


                    StockTransactionLineID = line.StockTransactionLineID,
                    ItemPackID = line.ItemPackID,
                    Quantity = line.Quantity,
                    Coast = line.Coast,
                    TotalCost = line.TotalCost,
                    ExpirationDate = line.ExpirationDate

                    
                }).ToList()
            };
        }
    }

}
