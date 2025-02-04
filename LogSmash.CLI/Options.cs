using CommandLine;

public class Options
{
    [Option('s', "search", Required = true, HelpText = "Search text file")]
    public string SearchText { get; set; }
    
    [Option('p', "pattern", Required = false, HelpText = "Search pattern", Default = "*.log")]
    public string SearchPattern { get; set; }
    
    [Option('d', "directory", Required = true, HelpText = "Search directory")]
    public string Directory { get; set; }
}