using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;

namespace SeamlessGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomersController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            try
            {
                var customers = await _customerRepository.GetAllAsync();
                var customerDtos = customers.Select(MapCustomerToDto);
                return Ok(customerDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving customers.", error = ex.Message });
            }
        }

        // GET: api/customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(string id)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(id);

                if (customer == null)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }

                var customerDto = MapCustomerToDto(customer);
                return Ok(customerDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the customer.", error = ex.Message });
            }
        }

        // POST: api/customers
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            try
            {
                var customer = new Customer
                {
                    CustomerID = createCustomerDto.CustomerID, 
                    CustomerCode = createCustomerDto.CustomerCode,
                    FullName = createCustomerDto.FullName,
    
                    City = createCustomerDto.City,
                    Address = createCustomerDto.Address,
                    Email = createCustomerDto.Email,
                    PhoneNumber1 = createCustomerDto.PhoneNumber1,
                    PhoneNumber2 = createCustomerDto.PhoneNumber2,
                    Latitude=createCustomerDto.Latitude,
                    Longitude = createCustomerDto.Longitude,
                    CustomerTypeID = createCustomerDto.CustomerTypeID,
                    CustomerBalance = createCustomerDto.CustomerBalance,
                    AccountLimit = createCustomerDto.AccountLimit,
                    CustomerGroupID = createCustomerDto.CustomerGroupID,
                    CreatedByUserID = createCustomerDto.CreatedByUserID,
                    CreatedDate = createCustomerDto.CreatedDate,

                    IsActive = createCustomerDto.IsActive ,
                    CustomerNote = createCustomerDto.CustomerNote,
                };

                var createdCustomer = await _customerRepository.CreateAsync(customer);
                var customerDto = MapCustomerToDto(createdCustomer);

                return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.CustomerID }, customerDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the customerError.", error = ex.Message });
            }
        }

        // PUT: api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(string id, UpdateCustomerDto updateCustomerDto)
        {
            try
            {
                var customer = new Customer
                {
                    CustomerCode = updateCustomerDto.CustomerCode,
                    FullName = updateCustomerDto.FullName,
                    City = updateCustomerDto.City,
                    Address = updateCustomerDto.Address,
                    Email = updateCustomerDto.Email,
                    PhoneNumber1 = updateCustomerDto.PhoneNumber1,
                    PhoneNumber2 = updateCustomerDto.PhoneNumber2,
                    CustomerTypeID = updateCustomerDto.CustomerTypeID,
                    CustomerBalance = updateCustomerDto.CustomerBalance,
                    AccountLimit = updateCustomerDto.AccountLimit,
                    CustomerGroupID = updateCustomerDto.CustomerGroupID,
                    CreatedByUserID = updateCustomerDto.CreatedByUserID,
                    IsActive = updateCustomerDto.IsActive,
                    CustomerNote = updateCustomerDto.CustomerNote
                };

                var success = await _customerRepository.UpdateAsync(id, customer);

                if (!success)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the customer.", error = ex.Message });
            }
        }

        // DELETE: api/customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                var success = await _customerRepository.DeleteAsync(id);

                if (!success)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the customer.", error = ex.Message });
            }
        }

        // GET: api/customers/search?name=john&email=john@example.com&city=newyork
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> SearchCustomers(
            [FromQuery] string? name,
            [FromQuery] string? email,
            [FromQuery] string? city)
        {
            try
            {
                var customers = await _customerRepository.SearchAsync(name, email, city);
                var customerDtos = customers.Select(MapCustomerToDto);
                return Ok(customerDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching customers.", error = ex.Message });
            }
        }

        private static CustomerDto MapCustomerToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerID = customer.CustomerID,
                CustomerCode = customer.CustomerCode,
                FullName = customer.FullName,
                City = customer.City,
                Address = customer.Address,
                Email = customer.Email,
                PhoneNumber1 = customer.PhoneNumber1,
                PhoneNumber2 = customer.PhoneNumber2,
                CustomerTypeID = customer.CustomerTypeID,
                CustomerBalance = customer.CustomerBalance,
                AccountLimit = customer.AccountLimit,
                CustomerGroupID = customer.CustomerGroupID,
                CreatedByUserID = customer.CreatedByUserID,
                CreatedDate = customer.CreatedDate,
                IsActive = customer.IsActive,
                CustomerNote = customer.CustomerNote,

            };
        }
    }
}
