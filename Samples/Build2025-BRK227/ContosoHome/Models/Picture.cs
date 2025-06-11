namespace ContosoHome.Models;

public class Scene
{
    public string? Path { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Id { get; set; }

    public double AspectRatio { get; set; } = 1;

    public Location Location { get; set; }
}

public enum Location
{
    Seattle,
    Miami,
    LA
}
