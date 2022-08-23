using Updater.Flows;
using CommandLine;
using System.Diagnostics;

void CopyDirectory(string sourceDirectoryPath, string outputDirectoryPath)
{
    DirectoryInfo di = new DirectoryInfo(sourceDirectoryPath);
    foreach (var file in di.GetFiles())
    {
        File.Copy(file.FullName, Path.Combine(outputDirectoryPath, file.Name), true);
    }

    foreach (var dir in di.GetDirectories())
    {
        DirectoryInfo subDirInfo = new DirectoryInfo(Path.Combine(outputDirectoryPath, dir.Name));
        if (!subDirInfo.Exists) subDirInfo.Create();

        CopyDirectory(dir.FullName, subDirInfo.FullName);
    }
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
                global::System.Console.WriteLine("Could not find parent process by name. Retrying using path");
            }

            FileInfo fi = new FileInfo(o.Parent);
            if(fi.Exists)
            {
                target = Process.GetProcessesByName(fi.Name).FirstOrDefault();
                if (target is null) Console.WriteLine("Could not find parent process by using a path. Retrying and appending {.exe}");
                else global::System.Console.WriteLine("Found parent process");
            }
            else
            {
                global::System.Console.WriteLine("Could not convert provided parent value to a existing file path. Retrying and appending {.exe}");
            }

            fi = new FileInfo($"{o.Parent}.exe");
            if (fi.Exists)
            {
                target = Process.GetProcessesByName(fi.Name.Replace(".exe", "")).FirstOrDefault();
                if(target is null)
                {
                    throw new Exception($"Found existing file at: {fi.FullName} - but could not find a running parent process with path.Name - {fi.Name}");
                }
                else global::System.Console.WriteLine("Found parent process");
            }
            else
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
                global::System.Console.WriteLine($"- {repo}");
            }
            tempFileDirectoryInfo = new DirectoryInfo(await new GithubFlow().Run(o.GithubRepositories));
            if(tempFileDirectoryInfo.Exists && tempFileDirectoryInfo.GetFiles().Any())
            {
                global::System.Console.WriteLine("Downloaded all files");
            }
            else
            {
                throw new Exception("Aborting update. Failed to download files");
            }
        }

        bool aborted = false;
        int retries = 0;
        while (!aborted)
        {
            // Try to delete previous files
            global::System.Console.WriteLine($"Deleting old files - Attempts[{retries + 1}]");

            try
            {
                var outputDirectory = new DirectoryInfo(string.IsNullOrWhiteSpace(o.OutputDirectoryPath) ? Directory.GetCurrentDirectory() : o.OutputDirectoryPath);
                foreach (var file in outputDirectory.GetFiles())
                {
                    file.Delete();
                }

                foreach (var dir in outputDirectory.GetDirectories())
                {
                    // Dont delete the utility folder
                    if (dir.Name.Equals("tmp") || dir.Name.Equals("utility")) continue;

                    dir.Delete(true);
                }
            }
            catch (global::System.Exception e)
            {
                if (retries == 4)
                {
                    aborted = true;
                    break;
                }

                global::System.Console.WriteLine(e.Message);
                global::System.Console.WriteLine($"Failed to delete old files - Retrying in 2 seconds");
                await Task.Delay(2000);
                retries++;
                continue;
            }

            break;
        }

        if(aborted)
        {
            global::System.Console.WriteLine("Aborting update. Failed to delete old files");
            return;
        }

        retries = 0; // Reset retries for upcomiong copy section

        while(!aborted)
        {
            global::System.Console.WriteLine($"Copying new files to output directory - Attempt[{retries + 1}]");

            try
            {
                CopyDirectory(tempFileDirectoryInfo.FullName, string.IsNullOrWhiteSpace(o.OutputDirectoryPath) ? Directory.GetCurrentDirectory() : o.OutputDirectoryPath);
            }
            catch (Exception e)
            {
                if (retries == 4)
                {
                    aborted = true;
                    break;
                }

                global::System.Console.WriteLine(e.Message);
                global::System.Console.WriteLine($"Failed to copy files - Retrying in 2 seconds");
                await Task.Delay(2000);
                retries++;
                continue;
            }

            break;
        }

        if (aborted)
        {
            global::System.Console.WriteLine("Aborting update. Failed to copy new files to destination");
            return;
        }

        if (tempFileDirectoryInfo.Exists) tempFileDirectoryInfo.Delete(true);

        if(!string.IsNullOrWhiteSpace(o.Parent))
        {
            global::System.Console.WriteLine($"Starting process: {o.Parent}");
            ProcessStartInfo psi = new()
            {
                FileName = o.Parent,
            };
            if(Process.Start(psi) == null)
            {
                global::System.Console.WriteLine($"Failed to start parent process: {o.Parent}");
            }
        }

        global::System.Console.WriteLine("Update completed");
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



