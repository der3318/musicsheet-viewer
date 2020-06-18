### Development Notes
```csharp
https://nugetmusthaves.com/Package/BitMiracle.Docotic.Pdf
https://github.com/dotnet/standard/issues/489

download and install .net SDK 2.0 from https://dotnet.microsoft.com/download/dotnet-core/2.0
C:\Program Files\dotnet\sdk\2.0.0\Microsoft\Microsoft.NET.Build.Extensions\net461\lib\netstandard.dll
C:\Program Files (x86)\dotnet\shared\Microsoft.NETCore.App\3.1.5\System.Drawing.dll

RequestedTheme="Dark"

var folderPicker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
folderPicker.FileTypeFilter.Add("*");
picturesFolder = await folderPicker.PickSingleFolderAsync();

Dictionary<string, List<StorageFile>> folderDictionary = new Dictionary<string, List<StorageFile>>();

string parentFolderPath = file.Path.Replace(file.Name, "");
if (folderDictionary.ContainsKey(parentFolderPath))
{
    folderDictionary[parentFolderPath].Add(file);
}
else
{
    folderDictionary.Add(parentFolderPath, new List<StorageFile> { file });
}

foreach (KeyValuePair<string, List<StorageFile>> kvp in folderDictionary)
{
    kvp.Value.Sort((file1, file2) => file1.Name.CompareTo(file2.Name));
    Images.Add(await LoadImageInfo(kvp.Value.First()));
}
```
