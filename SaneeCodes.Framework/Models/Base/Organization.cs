namespace SaneeCodes.Framework.Models;

public class Organization
{
    public int? OrgId  { get; set; }
    public string? OrgName  { get; set; }
    public string? OrgDescription  { get; set; }
    public bool IsActive  { get; set; }

    public List<Person>? Employees {get; set;}
}