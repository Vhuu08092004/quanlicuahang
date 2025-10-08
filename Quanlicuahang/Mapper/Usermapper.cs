using Quanlicuahang.Models;
using AutoMapper;
using Quanlicuahang.DTOs;

namespace Quanlicuahang.Mapper
{
    public class UserMapper :Profile
    {
        public UserMapper()
        {
            CreateMap<User, UserResponse>().ReverseMap();
            CreateMap<UserRequest, User>();
            CreateMap<UserUpdateRequest, User>();
        }
    }
}
