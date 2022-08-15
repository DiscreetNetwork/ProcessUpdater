# ProcessUpdater
Simple crossplatform CLI that is capable of updating other processes


### Usage
```
Updater [ARGS]
```

### Arguements
[-p, --parent] 
```
Name of the parent process calling the CLI. Used by the CLI to kill the parent before updating the binaries and starting the parent after the updating is done
```

[-k, --kill]
```
Boolean that indiciates whether or not to kill the calling process before updating
```

[-g, --grepository, Required] 
```
Space seperated name(s) of the github repository (+ asset) name used to fetch new binaries from. Format "{organization}/{project}+{releaseAssetName}"
```

[-o, --output, Optional] 
```
Full path to the directory in which to output the new files. Defaults to current directory
```


### Sample usage
For windows with a single github repository
```
Updater.exe --parent "DiscreetWallet" --grepository "DiscreetNetwork/discreet-gui+win-x64.zip"
```

For windows with multiple github repository
```
Updater.exe --parent "DiscreetWallet" --grepository "DiscreetNetwork/discreet-gui+win-x64.zip" "DiscreetNetwork/DiscreetCore+DiscreetCore.dll"
```
