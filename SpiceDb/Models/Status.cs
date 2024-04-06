namespace SpiceDb.Models;

public class Status
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<object> Details { get; set; } = new List<object>();
}
