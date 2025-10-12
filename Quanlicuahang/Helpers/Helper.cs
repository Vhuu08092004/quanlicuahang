using Quanlicuahang.Enum;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Quanlicuahang.Helpers
{
    public static class EnumHelper
    {
        public static string GetDisplayName(System.Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? value.ToString();
        }

        public static List<object> GetAllPermissions()
        {
            return System.Enum.GetValues(typeof(Permission))
                .Cast<Permission>()
                .Select(p => new
                {
                    Code = p.ToString(),
                    Name = GetDisplayName(p)
                })
                .ToList<object>();
        }

        public static void PrintAllPermissions()
        {
            var list = GetAllPermissions();
            Console.WriteLine("Danh sách quyền:");
            foreach (dynamic item in list)
            {
                Console.WriteLine($"- {item.Code}: {item.Name}");
            }
        }

    }
}
