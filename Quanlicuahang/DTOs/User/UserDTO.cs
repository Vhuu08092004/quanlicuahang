namespace Quanlicuahang.DTOs
{
    public class UserCreateUpdateDto
    {
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? EmployeeId { get; set; }
        public List<string>? RoleIds { get; set; } = new();
    }


    public class UserDto : BaseDto
    {
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public List<string> Roles { get; set; } = new();
    }


    public class UserSearchDto : BaseSearchDto
    {
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
