# ProcessUpdater
Simple crossplatform CLI that is capable of updating other processes. 

Bundled with Discreet Daemon & Wallet.

The build process for the updater can be seen inside the `.github/workflows/master-push.yml` file, when building either the daemon or the wallet manually from source.

### Usage
```
Updater [ARGS]
```

### Arguements
[-p, --parent] 
```
Name of the process calling the CLI. Used by the CLI to shut down the calling process before updating the binaries, and also starting the calling process after the updating is done
```

[-k, --kill]
```
Boolean that indiciates whether or not to shut down the process specified by the `Parent` arguement
```

[-g, --grepository, Required] 
```
Space seperated name(s) of the github repository (+ asset) name used to fetch new binaries from. Format "{organization}/{project}+{releaseAssetName}". It will use the latest release of the specified repository
```

[-o, --output] 
```
Full path to the directory in which to output the new files. Defaults to current directory
```


### Sample usage
For windows with a single github repository
```
Updater.exe --parent "DiscreetWallet" --grepository "DiscreetNetwork/discreet-gui+win-x64.zip"
```

For windows with multiple github repositories
```
Updater.exe --parent "DiscreetWallet" --grepository "DiscreetNetwork/discreet-gui+win-x64.zip" "DiscreetNetwork/DiscreetCore+DiscreetCore.dll"
```
