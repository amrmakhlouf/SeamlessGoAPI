using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;

namespace SeamlessGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
 
       
    
        public class OrdersController : ControllerBase
        {
            private readonly IOrderRepository _orderRepository;

            public OrdersController(IOrderRepository orderRepository)
            {
                _orderRepository = orderRepository;
            }

            // GET: api/orders
            [HttpGet]
            public async Task<ActionResult<IEnumerable<OrderDtocs>>> GetOrders([FromQuery] DateTime? LastModifiedUtc)
            {
                try
                {
                    var orders = await _orderRepository.GetAllAsync(LastModifiedUtc);
                    var orderDtos = orders.Select(MapOrderToDto);
                    return Ok(orderDtos);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error retrieving orders.", error = ex.Message });
                }
            }

            // GET: api/orders/5
            [HttpGet("{id}")]
            public async Task<ActionResult<OrderDtocs>> GetOrder(string id)
            {
                try
                {
                    var order = await _orderRepository.GetByIdWithLinesAsync(id);

                    if (order == null)
                    {
                        return NotFound(new { message = $"Order with ID {id} not found." });
                    }

                    return Ok(MapOrderToDto(order));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error retrieving order.", error = ex.Message });
                }
            }

            // POST: api/orders
            [HttpPost]
            public async Task<ActionResult<OrderDtocs>> CreateOrder(CreateOrderDto createOrderDto)
            {
                try
                {
                    var order = new Order
                    {
                        OrderID = createOrderDto.OrderID,
                        CustomerID = createOrderDto.CustomerID,
                        OrderDate = createOrderDto.OrderDate ?? DateTime.Now,
                        OrderTypeID = createOrderDto.OrderTypeID,
                        SubTotal = createOrderDto.SubTotal,
                        TotalAmount = createOrderDto.TotalAmount,
                        GrossAmount = createOrderDto.GrossAmount,
                        TotalRemainingAmount = createOrderDto.TotalRemainingAmount,
                        DiscountAmount = createOrderDto.DiscountAmount,
                        DiscountPerc = createOrderDto.DiscountPerc,
                        NetAmount = createOrderDto.NetAmount,
                        Tax = createOrderDto.Tax,
                        TaxPerc = createOrderDto.TaxPerc,
                        Status = createOrderDto.Status,
                        CreatedByUserID = createOrderDto.CreatedByUserID,
                        RouteID = createOrderDto.RouteID,
                        Note = createOrderDto.Note,
                        IsVoided = createOrderDto.IsVoided,
                        InvoicedID = createOrderDto.SourceOrderID,
                        LastModifiedUtc = createOrderDto.LastModifiedUtc,
                        SyncStatus =createOrderDto.SyncStatus
                    };

                    // Convert CreateOrderLineDto to OrderLine
                    List<OrderLine>? orderLines = null;
                    if (createOrderDto.OrderLines != null && createOrderDto.OrderLines.Any())
                    {
                        orderLines = createOrderDto.OrderLines.Select(dto => new OrderLine
                        {
                            OrderLineID = dto.OrderLineID,
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

                    var createdOrder = await _orderRepository.CreateAsync(order, orderLines);

                    var orderWithLines = await _orderRepository.GetByIdWithLinesAsync(createdOrder.OrderID);

                    if (orderWithLines == null)
                    {
                        return StatusCode(500, new { message = "Order created but could not retrieve details." });
                    }

                    var orderDto = MapOrderToDto(orderWithLines);
                    return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.OrderID }, orderDto);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while creating the order.", error = ex.Message });
                }
            }

            // PUT: api/orders/5
            //[HttpPut("{id}")]
            //public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDto updateOrderDto)
            //{
            //    try
            //    {
            //        var order = new Orders
            //        {
            //            CustomerID = updateOrderDto.CustomerID,
            //            OrderDate = updateOrderDto.OrderDate,
            //            OrderTypeID = updateOrderDto.OrderTypeID,
            //            DiscountAmount = updateOrderDto.DiscountAmount,
            //            DiscountPerc = updateOrderDto.DiscountPerc,
            //            Tax = updateOrderDto.Tax,
            //            TaxPerc = updateOrderDto.TaxPerc,
            //            Status = updateOrderDto.Status,
            //            RouteID = updateOrderDto.RouteID,
            //            IsVoided = updateOrderDto.IsVoided,
            //            Note = updateOrderDto.Note
            //        };

            //        var success = await _orderRepository.UpdateAsync(id, order);

            //        if (!success)
            //        {
            //            return NotFound(new { message = $"Order with ID {id} not found." });
            //        }

            //        return NoContent();
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, new { message = "An error occurred while updating the order.", error = ex.Message });
            //    }
            //}

            // DELETE: api/orders/5
            //[HttpDelete("{id}")]
            //public async Task<IActionResult> DeleteOrder(int id)
            //{
            //    try
            //    {
            //        var success = await _orderRepository.DeleteAsync(id);

            //        if (!success)
            //        {
            //            return NotFound(new { message = $"Order with ID {id} not found." });
            //        }

            //        return NoContent();
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, new { message = "An error occurred while deleting the order.", error = ex.Message });
            //    }
            //}

            // GET: api/orders/search?customerId=CUST001
            //[HttpGet("search")]
            //public async Task<ActionResult<IEnumerable<OrderDtocs>>> SearchOrders([FromQuery] string? customerId)
            //{
            //    try
            //    {
            //        if (string.IsNullOrEmpty(customerId))
            //        {
            //            return BadRequest(new { message = "Customer ID is required for search." });
            //        }

            //        var orders = await _orderRepository.GetOrdersByCustomerAsync(customerId);
            //        var orderDtos = orders.Select(MapOrderToDto);
            //        return Ok(orderDtos);
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, new { message = "An error occurred while searching orders.", error = ex.Message });
            //    }
        //    }

            private static OrderDtocs MapOrderToDto(Order order)
            {
                return new OrderDtocs
                {
                    OrderID = order.OrderID,
                    CustomerID = order.CustomerID,
                    OrderDate = order.OrderDate,
                    UpdateDate = order.UpdateDate,
                    OrderTypeID = order.OrderTypeID,
                    SubTotal = order.SubTotal,
                    TotalAmount = order.TotalAmount,
                    GrossAmount = order.GrossAmount,
                    TotalRemainingAmount = order.TotalRemainingAmount,
                    DiscountAmount = order.DiscountAmount,
                    DiscountPerc = order.DiscountPerc,
                    NetAmount = order.NetAmount,
                    Tax = order.Tax,
                    TaxPerc = order.TaxPerc,
                    Status = order.Status,
                    CreatedByUserID = order.CreatedByUserID,
                    RouteID = order.RouteID,
                    IsVoided = order.IsVoided,
                    Note = order.Note,
                    InvoicedID = order.InvoicedID,
                    LastModifiedUtc=order.LastModifiedUtc,
                    OrderLines = order.OrderLines?.Select(line => new OrderLineDto
                    {
                        OrderLineID = line.OrderLineID,
                        OrderID = line.OrderID,
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