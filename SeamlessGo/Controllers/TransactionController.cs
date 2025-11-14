using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;

namespace SeamlessGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
 
       
    
        public class TransactionController : ControllerBase
        {
            private readonly ITransactionRepository _TransactionRepository;

            public TransactionController(ITransactionRepository TransactionRepository)
            {
                _TransactionRepository = TransactionRepository;
            }

            // GET: api/Transactions
            [HttpGet]
            public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetTransactions([FromQuery] DateTime? LastModifiedUtc)

            {
                try
                {
                    var Transactions = await _TransactionRepository.GetAllAsync(LastModifiedUtc);
                    var TransactionDtos = Transactions.Select(MapTransactionToDto);
                    return Ok(TransactionDtos);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error retrieving Transactions.", error = ex.Message });
                }
            }

            // GET: api/Transactions/5
            [HttpGet("{id}")]
            public async Task<ActionResult<TransactionDTO>> GetTransaction(string id)
            {
                try
                {
                    var Transaction = await _TransactionRepository.GetByIdWithLinesAsync(id);

                    if (Transaction == null)
                    {
                        return NotFound(new { message = $"Transaction with ID {id} not found." });
                    }

                    return Ok(MapTransactionToDto(Transaction));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error retrieving Transaction.", error = ex.Message });
                }
            }

            // POST: api/Transactions
            [HttpPost]
            public async Task<ActionResult<TransactionDTO>> CreateTransaction(CreateTransactionDTO createTransactionDto)
            {
                try
                {
                    var Transaction = new Transaction
                    {
                        TransactionID = createTransactionDto.TransactionID,
                        CustomerID = createTransactionDto.CustomerID,
                        TransactionDate = createTransactionDto.TransactionDate ?? DateTime.Now,
                        TransactionTypeID = createTransactionDto.TransactionTypeID,
                        SubTotal = createTransactionDto.SubTotal,
                        TotalAmount = createTransactionDto.TotalAmount,
                        GrossAmount = createTransactionDto.GrossAmount,
                        TotalRemainingAmount = createTransactionDto.TotalRemainingAmount,
                        DiscountAmount = createTransactionDto.DiscountAmount,
                        DiscountPerc = createTransactionDto.DiscountPerc,
                        NetAmount = createTransactionDto.NetAmount,
                        Tax = createTransactionDto.Tax,
                        TaxPerc = createTransactionDto.TaxPerc,
                        Status = createTransactionDto.Status,
                        CreatedByUserID = createTransactionDto.CreatedByUserID,
                        RouteID = createTransactionDto.RouteID,
                        Note = createTransactionDto.Note,
                        IsVoided = createTransactionDto.IsVoided,
                        SourceTransactionID = createTransactionDto.SourceTransactionID,
                        LastModifiedUtc=createTransactionDto.LastModifiedUtc
                    };

                    // Convert CreateTransactionLineDto to TransactionLine
                    List<TransactionLine>? TransactionLines = null;
                    if (createTransactionDto.TransactionLine != null && createTransactionDto.TransactionLine.Any())
                    {
                        TransactionLines = createTransactionDto.TransactionLine.Select(dto => new TransactionLine
                        {
                            TransactionLineID = dto.TransactionLineID,
                            ItemPackID = dto.ItemPackID,
                            DiscountAmount = dto.DiscountAmount,
                            DiscountPerc = dto.DiscountPerc,
                            Bonus = dto.Bonus,
                            Quantity = dto.Quantity,
                            ItemPrice = dto.ItemPrice,
                            FullPrice = dto.ItemPrice, // Or calculate based on your logic
                            TotalPrice = (dto.ItemPrice ?? 0) * (dto.Quantity ?? 0), // Calculate total
                            Note = dto.Note
                        }).ToList();
                    }

                    var createdTransaction = await _TransactionRepository.CreateAsync(Transaction, TransactionLines);

                    var TransactionWithLines = await _TransactionRepository.GetByIdWithLinesAsync(createdTransaction.TransactionID);

                    if (TransactionWithLines == null)
                    {
                        return StatusCode(500, new { message = "Transaction created but could not retrieve details." });
                    }

                    var TransactionDto = MapTransactionToDto(TransactionWithLines);
                    return CreatedAtAction(nameof(GetTransaction), new { id = createdTransaction.TransactionID }, TransactionDto);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while creating the Transaction.", error = ex.Message });
                }
            }

            // PUT: api/Transactions/5
            //[HttpPut("{id}")]
            //public async Task<IActionResult> UpdateTransaction(int id, UpdateTransactionDto updateTransactionDto)
            //{
            //    try
            //    {
            //        var Transaction = new Transactions
            //        {
            //            CustomerID = updateTransactionDto.CustomerID,
            //            TransactionDate = updateTransactionDto.TransactionDate,
            //            TransactionTypeID = updateTransactionDto.TransactionTypeID,
            //            DiscountAmount = updateTransactionDto.DiscountAmount,
            //            DiscountPerc = updateTransactionDto.DiscountPerc,
            //            Tax = updateTransactionDto.Tax,
            //            TaxPerc = updateTransactionDto.TaxPerc,
            //            Status = updateTransactionDto.Status,
            //            RouteID = updateTransactionDto.RouteID,
            //            IsVoided = updateTransactionDto.IsVoided,
            //            Note = updateTransactionDto.Note
            //        };

            //        var success = await _TransactionRepository.UpdateAsync(id, Transaction);

            //        if (!success)
            //        {
            //            return NotFound(new { message = $"Transaction with ID {id} not found." });
            //        }

            //        return NoContent();
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, new { message = "An error occurred while updating the Transaction.", error = ex.Message });
            //    }
            //}

            // DELETE: api/Transactions/5
            //[HttpDelete("{id}")]
            //public async Task<IActionResult> DeleteTransaction(int id)
            //{
            //    try
            //    {
            //        var success = await _TransactionRepository.DeleteAsync(id);

            //        if (!success)
            //        {
            //            return NotFound(new { message = $"Transaction with ID {id} not found." });
            //        }

            //        return NoContent();
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, new { message = "An error occurred while deleting the Transaction.", error = ex.Message });
            //    }
            //}

            // GET: api/Transactions/search?customerId=CUST001
            //[HttpGet("search")]
            //public async Task<ActionResult<IEnumerable<TransactionDtocs>>> SearchTransactions([FromQuery] string? customerId)
            //{
            //    try
            //    {
            //        if (string.IsNullOrEmpty(customerId))
            //        {
            //            return BadRequest(new { message = "Customer ID is required for search." });
            //        }

            //        var Transactions = await _TransactionRepository.GetTransactionsByCustomerAsync(customerId);
            //        var TransactionDtos = Transactions.Select(MapTransactionToDto);
            //        return Ok(TransactionDtos);
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, new { message = "An error occurred while searching Transactions.", error = ex.Message });
            //    }
        //    }

            private static TransactionDTO MapTransactionToDto(Transaction Transaction)
            {
                return new TransactionDTO
                {
                    TransactionID = Transaction.TransactionID,
                    CustomerID = Transaction.CustomerID,
                    TransactionDate = Transaction.TransactionDate,
                    UpdateDate = Transaction.UpdateDate,
                    TransactionTypeID = Transaction.TransactionTypeID,
                    SubTotal = Transaction.SubTotal,
                    TotalAmount = Transaction.TotalAmount,
                    GrossAmount = Transaction.GrossAmount,
                    TotalRemainingAmount = Transaction.TotalRemainingAmount,
                    DiscountAmount = Transaction.DiscountAmount,
                    DiscountPerc = Transaction.DiscountPerc,
                    NetAmount = Transaction.NetAmount,
                    Tax = Transaction.Tax,
                    TaxPerc = Transaction.TaxPerc,
                    Status = Transaction.Status,
                    CreatedByUserID = Transaction.CreatedByUserID,
                    RouteID = Transaction.RouteID,
                    IsVoided = Transaction.IsVoided,
                    Note = Transaction.Note,
                    SourceTransactionID = Transaction.SourceTransactionID,
                    SourceOrderID = Transaction.SourceOrderID,
                    LastModifiedUtc=Transaction.LastModifiedUtc,
                    TransactionLine = Transaction.TransactionLine?.Select(line => new TransactionLineDTO
                    {
                        TransactionLineID = line.TransactionLineID,
                        TransactionID = line.TransactionID,
                        ItemPackID = line.ItemPackID,
                        DiscountAmount = line.DiscountAmount,
                        DiscountPerc = line.DiscountPerc,
                        Bonus = line.Bonus,
                        Quantity = line.Quantity,
                        ItemPrice = line.ItemPrice,
                        FullPrice = line.FullPrice,
                        TotalPrice = line.TotalPrice,
                        Note = line.Note
                    }).ToList()
                };
            }
        }
    
}