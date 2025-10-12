namespace Quanlicuahang.DTOs.ActionLog
{
    public class ActionLogFilterDto
    {
        public string? FunctionType { get; set; } 
        public string? FunctionId { get; set; }  
        public string? Type { get; set; }      
        public string? CreatedBy { get; set; }   
    }

    public class ActionLogSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public ActionLogFilterDto? Where { get; set; }
    }
}