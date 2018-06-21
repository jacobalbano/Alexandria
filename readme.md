
Alexandria is a holistic asset library solution. It abstracts the notion of loading files and iterating files and directories so you can use a consistent interface to pull data from a folder, a .ZIP archive, or anywhere you can imagine -- pack your textures into a file and treat it as a directory, load files over HTTP, or read from a password-protected RAR file. 

# Installation
 1. Build the solution (requires .NET 4.5.2)
 2. Reference the DLL in your project
Alternately, copy all the .cs files in the Alexandria project into your project.

# Usage
All operations begin with the Library object:
```csharp
var assets = new Alexandria.Library();
```

### FileStore
FileStores are the means by which the Library will discover files and directories, as well as return data streams to construct assets with. Alexandria comes with three by default:

```csharp
// the ZipFileStore allows access to files inside a specific zip archive
assets.AddFilestore(new ZipFileStore("path/to/archive.zip"));

// the RootDirectoryFileStore allows access to files in a specific folder
// relative paths are ignored to ensure security
assets.AddFileStore(new RootDirectoryFileStore("assets/"));

// the FilesystemFileStore provides dead-simple passthrough to your working directory
assets.AddFileStore(new FilesystemFileStore());
```
You can create a Loader of your own by implementing `Library.IFileStore` or `Library.IReloadableFileStore`. Just as it sounds, `IReloadableFileStore` will alert the Library if one of its files is modified and offer the chance to reload it.

### FileStoreFactory
FileStoreFactories are the means by which the Library can seek recursively inside other special FileStores. You can use them to iterate through files inside an archive, or even seek into zip files *inside* of zip files. Alexandria comes with one by default:

```csharp
assets.AddFileStoreFactory(new ZipFileStore.Factory());
```

### Loader
Loaders are the means by which the Library will turn a data stream into a concrete asset file. Alexandria comes with one by default:

```csharp
assets.AddLoader(new StringLoader());
```

You can create a Loader of your own by subclassing `Library.Loader<T>` or `Library.ReloadableLoader<T>`. `ReloadableLoader` works in synergy with `IReloadableFileStore` and will allow you to update assets in-place to have them instantly reflect their new state.

### Iterating items
The Library provides methods to enumerate files and directories across all FileStores. For example, let's say your structure looks like this:

```
root1/
	subdir/file1.txt

root2/
	subdir/file2.txt

root3.zip
	subdir/file3.txt
```
Using the following code:
```csharp
assets.Add(new RootDirectoryFileStore("root1"));
assets.Add(new RootDirectoryFileStore("root2"));
assets.Add(new ZipFileStore("root3.zip"));

var allFiles = assets.EnumerateFiles("subdir");
// results: "subdir/file1.txt", "subdir/file2.txt", "subdir/file3.txt"
```

### Loading files
Once you have your FileStores and Loaders set up and registered with the Library, you're good to go!

```csharp
var myStr = assets.Load<string>("file.txt");
```
With the right combination of FileStores and FileStoreFactories, you can even do something crazy like this:

```csharp
var myStr = assets.Load<string>("first.zip/second.zip/file.txt");
```

Loaded objects are cached automatically and will only be created once. If multiple files with the same path exist across multiple FileStores, the last one found will be preferred. This allows for assets to override each other (as in a game with mod support).

If you have any questions, find a bug, or want to request a feature, leave a message here or hit me up on Twitter [@jacobalbano][1]!

[1]: http://www.twitter.com/jacobalbano
