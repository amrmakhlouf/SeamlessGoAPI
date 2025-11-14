using SeamlessGo.DTOs;
using SeamlessGo.Models;
using AutoMapper;

namespace SeamlessGo.Mappings
{
    public class UserProfile:Profile
    {
        public UserProfile()
        {
            CreateMap<User, UsersDTOcs>();
            CreateMap<StockLocation, StockLocationDto>();
            CreateMap<Plan, PlanDto>();
            CreateMap<Sequence, SequenceDto>();


            CreateMap<SeamlessGo.Models.Route, RouteDto>();
        }
    }
}
