using Quanlicuahang.DTOs.Response;
using Quanlicuahang.Models;
using Quanlicuahang.DTOs.Request;
using AutoMapper;

namespace Quanlicuahang.Mapper
{
    public class UserMapper :Profile
    {
        public UserMapper()
        {
            CreateMap<User, UserResponse>();
            CreateMap<UserRequest, User>();
        }
    }
}
