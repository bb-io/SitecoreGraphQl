namespace Apps.SitecoreGraphQl.Models.Dtos;

public class NodesDto<T>
{
    public List<T> Nodes { get; set; } = new();
}