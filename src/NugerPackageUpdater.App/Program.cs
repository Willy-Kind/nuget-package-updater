using System.Diagnostics;

var outdatedPackages = GetOutdatedPackages(args[0]);
UpdatePackages(args[0], outdatedPackages);

static Dictionary<string, string> GetOutdatedPackages(string pathToProject)
{
    var psi = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = $"list {Directory.GetCurrentDirectory()}/{pathToProject} package --outdated",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    var outdatedPackages = new Dictionary<string, string>();
    using (var process = new Process { StartInfo = psi })
    {
        process.Start();

        while (!process.StandardOutput.EndOfStream)
        {
            var line = process.StandardOutput.ReadLine()!;

            if (!string.IsNullOrWhiteSpace(line) && line.Contains(' ') && line.Trim()[0] == '>')
            {
                var parts = line
                    .Trim()
                    .Substring(1)
                    .Trim()
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                outdatedPackages.Add(parts.First(), parts.Last());
            }
        }

        process.WaitForExit();
    }

    return outdatedPackages;
}

static void UpdatePackages(string pathToProject, Dictionary<string, string> packages)
{
    foreach (var package in packages)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"add {Directory.GetCurrentDirectory()}/{pathToProject} package {package.Key} -v {package.Value}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = psi })
        {
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Console.WriteLine("Output: " + output);
            Console.WriteLine("Error: " + error);
        }
    }
}