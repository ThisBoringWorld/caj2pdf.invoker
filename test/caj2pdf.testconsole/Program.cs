using System.Diagnostics;
using System.IO.Enumeration;

using caj2pdf;

Caj2PdfConverter.Init();

var rootPath = Path.Combine(Environment.CurrentDirectory, "files");

var searchPatterns = new[] { "*.caj", "*.kdh" };
var files = Directory.EnumerateFiles(rootPath, "*.???", SearchOption.AllDirectories)
                     .Where(fileName => searchPatterns.Any(pattern => FileSystemName.MatchesSimpleExpression(pattern, fileName)))
                     .ToArray();

if (files.Length == 0)
{
    Console.WriteLine("No files found");
    return;
}

foreach (var sourceFile in files)
{
    var sw = Stopwatch.StartNew();
    var outputFile = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(sourceFile))!, "out", Path.ChangeExtension(Path.GetFileName(sourceFile), ".pdf"));
    Console.WriteLine("Converting {0} to {1}", sourceFile, outputFile);
    await Caj2PdfConverter.ConvertAsync(sourceFile, outputFile, CancellationToken.None);
    sw.Stop();
    Console.WriteLine("{0} -> {1} in {2}", sourceFile, outputFile, sw.Elapsed);
    Console.WriteLine();
}
