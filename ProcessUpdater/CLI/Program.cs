using CLI.Services;
using CLI.Utility;
using CommandLine;
using System.Diagnostics;


Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(async o =>
    {
        Process target = Process.GetProcessesByName(o.Parent).FirstOrDefault();
        target.Kill();


        // fetch new files 
        GithubReleaseService grs = new GithubReleaseService(new HttpClient());
        var release = await grs.GetRelease("rickifunk/versioning-test");
        if (release is null) throw new Exception();

        var assets = await grs.GetAssets(release.AssetsUrl);
        var asset = await grs.DownloadAsset(assets[0]);

        var tempDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "tmp"));
        if (tempDir.Exists) tempDir.Delete(true);
        tempDir.Create();
        Zipper.Unzip(asset, tempDir.FullName);

        bool updated = false;
        while (!updated)
        {
            bool retry = false;

            DirectoryInfo tempPublishDir = new DirectoryInfo(Path.Combine(tempDir.FullName, "publish"));

            foreach (var fileInfo in tempPublishDir.GetFiles())
            {
                try
                {
                    File.Copy(fileInfo.FullName, Path.Combine(Directory.GetCurrentDirectory(), fileInfo.Name), true);
                }
                catch (global::System.Exception)
                {
                    global::System.Console.WriteLine("Failed to copy, retrying");
                    retry = true;
                    Thread.Sleep(2000);
                }
            }

            if (retry) continue;

            updated = true;
        }

        if (tempDir.Exists) tempDir.Delete(true);

        ProcessStartInfo psi = new()
        {
            FileName = $"WPF.exe",
        };
        Process.Start(psi);

        Environment.Exit(0);
    }
);

public class Options
{
    [Option('p', "parent", Required = true, HelpText = "Name of the parent process that started this process")]
    public string Parent { get; set; }
}