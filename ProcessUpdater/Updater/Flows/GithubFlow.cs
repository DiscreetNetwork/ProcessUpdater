using Updater.Services;
using Updater.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Flows
{
    public class GithubFlow
    {
        public async Task<string> Run(IEnumerable<string> githubRepositories)
        {
            GithubReleaseService grs = new GithubReleaseService(new HttpClient());

            var tempDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "tmp"));
            if (tempDir.Exists) tempDir.Delete(true);
            tempDir.Create();

            foreach (var repo in githubRepositories)
            {
                string repositoryName = repo.Split("+")[0];
                string assetName = repo.Split("+")[1];

                var release = await grs.GetRelease(repositoryName);
                if (release is null) throw new Exception();

                var assets = await grs.GetAssets(release.AssetsUrl);

                var assetToDownload = assets.Where(a => a.Name.Equals(assetName)).FirstOrDefault();
                var asset = await grs.DownloadAsset(assetToDownload);

                if(assetToDownload.ContentType.Equals("application/zip"))
                {
                    Zipper.Unzip(asset, tempDir.FullName);
                }
                else
                {
                    using FileStream fs = new FileStream(Path.Combine(tempDir.FullName, assetToDownload.Name), FileMode.Create, FileAccess.Write);
                    fs.Write(asset, 0, asset.Length);
                }
            }

            return tempDir.FullName;
        }
    }
}
