namespace Quanlicuahang.DTOs.ActionLog
{
    public class ActionLogFilterDto
    {
        public string? FunctionType { get; set; }  // EntityType
        public string? FunctionId { get; set; }    // EntityId
        public string? Type { get; set; }          // Action
        public string? CreatedBy { get; set; }     // UserId
    }

    public class ActionLogSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public ActionLogFilterDto? Where { get; set; }
    }
}
