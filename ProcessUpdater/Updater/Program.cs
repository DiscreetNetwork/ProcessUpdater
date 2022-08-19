using Updater.Flows;
using CommandLine;
using System.Diagnostics;

bool CopyDirectory(string sourceDirectoryPath, string outputDirectoryPath)
{
    DirectoryInfo di = new DirectoryInfo(sourceDirectoryPath);
    foreach (var file in di.GetFiles())
    {
        try
        {
            File.Copy(file.FullName, Path.Combine(outputDirectoryPath, file.Name), true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    foreach (var dir in di.GetDirectories())
    {
        DirectoryInfo subDirInfo = new DirectoryInfo(Path.Combine(outputDirectoryPath, dir.Name));
        if (!subDirInfo.Exists) subDirInfo.Create();

        return CopyDirectory(dir.FullName, subDirInfo.FullName);
    }

    return true;
}

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync<Options>(async o =>
    {
        if(o.Kill && !string.IsNullOrWhiteSpace(o.Parent))
        {
            Console.WriteLine("Killing parent process");
            Process target = Process.GetProcessesByName(o.Parent).FirstOrDefault();
            if (target is null)
            {
                throw new Exception($"Could not find parent process - {o.Parent}");
            }

            target.Kill();
        }

        DirectoryInfo tempFileDirectoryInfo = null;

        if(o.GithubRepositories != null && o.GithubRepositories.Any())
        {
            Console.WriteLine("Running GithubFlow");
            foreach (var repo in o.GithubRepositories)
            {
                global::System.Console.WriteLine(repo);
            }
            tempFileDirectoryInfo = new DirectoryInfo(await new GithubFlow().Run(o.GithubRepositories));
        }

        while (true)
        {
            //TODO: Delete all files in the output directory
            // If a file from a previous version is gone due to the new update, we should make sure its deleted while we add the new files
            
            if(!CopyDirectory(tempFileDirectoryInfo.FullName, string.IsNullOrWhiteSpace(o.OutputDirectoryPath) ? Directory.GetCurrentDirectory() : o.OutputDirectoryPath))
            {
                continue;
            }

            break;
        }

        if (tempFileDirectoryInfo.Exists) tempFileDirectoryInfo.Delete(true);

        if(!string.IsNullOrWhiteSpace(o.Parent))
        {
            ProcessStartInfo psi = new()
            {
                FileName = o.Parent,
            };
            Process.Start(psi);
        }

        Environment.Exit(0);
    }
);

public class Options
{
    [Option('p', "parent", Required = false, HelpText = "Name of the parent process that started this process")]
    public string Parent { get; set; }

    [Option('k', "kill", Required = false, HelpText = "Boolean that indiciates whether or not to kill the calling process before updating")]
    public bool Kill { get; set; }

    [Option('g', "grepository", Required = true, HelpText = "Space seperated name(s) of the github repository (+ asset) name used to fetch new binaries from. Format ''{organization}/{project}+{releaseAssetName}''")]
    public IEnumerable<string> GithubRepositories { get; set; }

    [Option('o', "output", Required = false, HelpText = "Full path to the directory in which to output the new files. Defaults to current directory")]
    public string OutputDirectoryPath { get; set; }
}



