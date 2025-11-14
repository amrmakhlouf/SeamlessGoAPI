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

    public class AuthController : ControllerBase
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IMapper _mapper;



        public AuthController(IUsersRepository usersRepository, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _mapper = mapper;


        }

        [HttpGet("login")]
        public async Task<ActionResult<IEnumerable<UsersDTOcs>>> GetUsers([FromQuery] string username, [FromQuery] string password)
        {
            try
            {
                var users = await _usersRepository.GetAllAsync(username, password);
                if (users == null || !users.Any())
                    return NotFound();

                // 👇 Map the collection, not single object
                var userDtos = _mapper.Map<IEnumerable<UsersDTOcs>>(users);
                var user = userDtos.FirstOrDefault();
                return Ok(new
                {
                    UserInfo = new
                    {
                        userID = user.UserID,
                        userName = user.UserName,
                        phoneNumber = user.PhoneNumber,
                        password = user.Password,
                        passwordSalt = user.PasswordSalt,
                        email = user.Email,
                        isActive = user.IsActive,
                        displayName = user.DisplayName,
                        userRoleID = user.UserRoleID,
                    },
                    ClientInfo = new
                    {
                        clientID = user.client.ClientID,
                        clientName = user.client.ClientName,
                        organizationName = user.client.OrganizationName,
                        contactPhone = user.client.ContactPhone,
                        taxNumber = user.client.TaxNumber,
                        address = user.client.Address,
                        isActive = user.client.IsActive
                    }
                    , LocationInfo = new
                    {
                              LocationID             =  user.stockLocation.LocationID,
                              LocationName           =  user.stockLocation.LocationName,
                              LocationCode           =  user.stockLocation.LocationCode,
                              StockLocationTypeID    =  user.stockLocation.StockLocationTypeID,
                              VehiclePlate           =  user.stockLocation.VehiclePlate,
                              DriverUserID           =  user.stockLocation.DriverUserID,
                              Capacity               =  user.stockLocation.Capacity,
                              IsActive               =  user.stockLocation.IsActive,
                              Address                =  user.stockLocation.Address,
                              Region                 =  user.stockLocation.Region,
                              CityID                 =  user.stockLocation.CityID,
                              CountryID              =  user.stockLocation.CountryID,
                              CreatedDate            =  user.stockLocation.CreatedDate,
                              CreatedByUserID        =  user.stockLocation.CreatedByUserID,
                    }
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving users.", error = ex.Message });
            }
        }
        private static UsersDTOcs MapUserToDto(User user)
        {
            return new UsersDTOcs
            {
                UserID = user.UserID,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                IsActive = user.IsActive,
                DisplayName = user.DisplayName,
                UserRoleID = user.UserRoleID
            };
        }

    }
}