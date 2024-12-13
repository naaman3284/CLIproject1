// See https://aka.ms/new-console-template for more information
//fib bundel --output D:\folder\bundelFile.txt
using System.CommandLine;

var bundleOption = new Option<FileInfo>("--output", "File path and name") {IsRequired = true,};
bundleOption.AddAlias("-o");
bundleOption.AddValidator(result =>
{
    if (result.GetValueOrDefault<FileInfo>() == null || string.IsNullOrEmpty(result.Tokens[0].Value))
    {
        result.ErrorMessage = "Output file path cannot be empty.";
    }
});
var languageOption = new Option<string[]>("--language", "List of programming languages (use 'all' for all files)")
{
    IsRequired = true,
    ArgumentHelpName = "language"
};
languageOption.AddAlias("-l");
languageOption.AddValidator(result =>
{
    var languages = result.Tokens.Select(t => t.Value).ToArray();
    if (!languages.Contains("all") && languages.Any(string.IsNullOrWhiteSpace))
    {
        result.ErrorMessage = "Please specify valid programming languages or 'all'.";
    }
});
// אפשרות לרשום מקור קובץ הקוד כהערה
var noteOption = new Option<bool>("--note", "Include original code source as a comment in the bundle file");
noteOption.AddAlias("-n");
// בץ או סוג הקובץ**
var sortOption = new Option<string>("--sort", () => "name", "Order of files: by file name (alphabetical) or by code type");
sortOption.AddAlias("-s");
sortOption.AddValidator(result =>
{
    var value = result.Tokens.FirstOrDefault()?.Value;
    if (value != "name" && value != "type")
    {
        result.ErrorMessage = "Sort option must be either 'name' or 'type'.";
    }
});

// אפשרות למחיקת שורות ריקות מהקוד**
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "Remove empty lines from the source code");
removeEmptyLinesOption.AddAlias("-r");
// אפשרות לרשום את שם יוצר הקובץ**
var authorOption = new Option<string>("--author", "Specify the author name to include in the bundle file");
authorOption.AddAlias("-a");

var bundleCommand = new Command("bundle", "Bundele code files to a single file");
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);

bundleCommand.SetHandler((FileInfo output, string[] languages, bool note, string sort, bool removeEmptyLines, string author) =>
{
    try
    {
        var validFiles = Directory.EnumerateFiles(output.DirectoryName ?? "", "*.*", SearchOption.AllDirectories)
            .Where(file =>
            {
                var excludedFolders = new[] { "bin", "debug", "obj" };
                var fileDirectory = Path.GetDirectoryName(file);
                return !excludedFolders.Any(folder => fileDirectory?.Contains(folder, StringComparison.OrdinalIgnoreCase) == true);
            })
            .ToList();

        using var fileStream = File.Create(output.FullName);
        using var writer = new StreamWriter(fileStream);
        ////if (languages.Contains("all"))
        ////{
        ////    Console.WriteLine("Including all code files from the directory.");
        ////}
        ////else
        ////{
        ////    foreach (var language in languages)
        ////    {
        ////        Console.WriteLine($"Including files of language: {language}");
        ////    }
        ////}

        ////// יצירת הקובץ
        ////using (var fileStream = File.Create(output.FullName))
        ////{
        ////    using var writer = new StreamWriter(fileStream);
        ////    // **כתיבת שם היוצר אם סופק**
        if (!string.IsNullOrEmpty(author))
            {
                writer.WriteLine($"// Author: {author}");
            }

            // **אפשרות להוספת הערה על מקור הקוד**
            if (note)
            {
                writer.WriteLine("// Source: Original file path and name");
            }

            // **הוספת קבצי הקוד על פי המיון הנבחר**
            Console.WriteLine($"Sorting files by: {sort}");
            // כאן יש להוסיף את המיון וההוספה של קבצי הקוד

            writer.WriteLine("File was created with selected options.");
            Console.WriteLine("File was created");
        //}    
        // File.Create(output.FullName);
        // Console.WriteLine("File was created");
    }
    catch (Exception ex)
    {
        Console.WriteLine(" Error:File path is invalid");
        Console.WriteLine($"Exception message: {ex.Message}");

    }
 },bundleOption,languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

var createRspCommand = new Command("create-rsp", "Create a response file with preset command options");

createRspCommand.SetHandler(() =>
{
    Console.Write("Enter the output file path and name: ");
    string output = Console.ReadLine();

    Console.Write("Enter languages (comma-separated, or 'all'): ");
    string languages = Console.ReadLine();

    Console.Write("Include code source note? (true/false): ");
    string note = Console.ReadLine();

    Console.Write("Sort files by ('name' or 'type'): ");
    string sort = Console.ReadLine();

    Console.Write("Remove empty lines? (true/false): ");
    string removeEmptyLines = Console.ReadLine();

    Console.Write("Enter author name (optional): ");
    string author = Console.ReadLine();

    var rspContent = $"bundle --output \"{output}\" --language {languages} " +
                     $"{(note.ToLower() == "true" ? "--note " : "")}" +
                     $"--sort {sort} " +
                     $"{(removeEmptyLines.ToLower() == "true" ? "--remove-empty-lines " : "")}" +
                     $"{(!string.IsNullOrEmpty(author) ? $"--author \"{author}\"" : "")}";

    string rspFilePath = "command.rsp";
    File.WriteAllText(rspFilePath, rspContent);

    Console.WriteLine($"Response file created at {rspFilePath}. Run it with: dotnet @command.rsp");
});
var rootCommand = new RootCommand("Root command for File Bundle CLI");
rootCommand.AddCommand(bundleCommand);
await rootCommand.InvokeAsync(args);
