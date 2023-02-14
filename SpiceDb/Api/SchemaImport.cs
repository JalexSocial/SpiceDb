namespace SpiceDb.Api;

internal class SchemaImport123
{
    public string schema { get; set; } = string.Empty;
    public string relationships { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Assertions { get; set; } = new();
    public Dictionary<string, List<string>> Validation { get; set; } = new();
}
