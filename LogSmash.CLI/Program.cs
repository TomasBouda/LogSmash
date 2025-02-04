using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using CommandLine;
using ConsoleTables;

var errors = new ConcurrentDictionary<string, FileResults>();
var foundTotal = 0;
	
var sw = System.Diagnostics.Stopwatch.StartNew();

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync<Options>(async o =>
    {
        var searchRegex = new Regex(o.SearchText, RegexOptions.Compiled);
        var logFiles = Directory.GetFiles(o.Directory, o.SearchPattern, SearchOption.AllDirectories);
        
        await Parallel.ForEachAsync(logFiles, async (logFile, token) =>
        {
            int textFound = 0;

            await using (FileStream fs = File.Open(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            await using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                while (await sr.ReadLineAsync() is { } line)
                {
                    var span = line.AsSpan();

                    if (searchRegex.IsMatch(span))
                    {
                        textFound++;
                    }
                }
            }

            errors[logFile] = new FileResults
            {
                FileName = logFile,
                SearchTextOccurrences = textFound
            };

            Interlocked.Add(ref foundTotal, textFound);
        });
    });

sw.Stop();

var table = new ConsoleTable("File", "Results");

foreach (var error in errors.Values.OrderByDescending(e => e.SearchTextOccurrences))
{
    table.AddRow(error.FileName, error.SearchTextOccurrences);
}
table.Write();
Console.WriteLine($"Elapsed time: {sw.Elapsed}");
Console.Out.Flush();