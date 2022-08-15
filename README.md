# ProcessUpdater
Simple crossplatform CLI that is capable of updating other processes


### Usage
```
Updater [ARGS]
```

### Arguements
[-p, --parent, Required] 
```
Name of the parent process calling the CLI. Used by the CLI to kill the parent before updating the binaries and starting the parent after the updating is done
```

[-g, --grepository, Required] 
```
Name of the github repository to fetch new binaries from. Format is "{organization}/{project}"
```

[-o, --output, Optional] 
```
Path to a directory in which the updated files should be outputted to. If this is specified, this path will be prepended to the parent name
```


### Sample usage
For windows
```
Updater.exe --parent "DiscreetWallet" --grepository "DiscreetNetwork/discreet-gui"
```
