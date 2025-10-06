using Quanlicuahang.Enum;

namespace Quanlicuahang.DTOs.Response
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public List<Role> Role { get; set; }
    }
}
