### Development Notes
```csharp
RequestedTheme="Dark"

var folderPicker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
folderPicker.FileTypeFilter.Add("*");
searchFolder = await folderPicker.PickSingleFolderAsync();

var result = searchFolder.CreateFileQueryWithOptions(options);
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

QueryOptions options = new QueryOptions();
options.FolderDepth = FolderDepth.Shallow;
options.FileTypeFilter.Add(".pdf");
var result = searchFolder.CreateFileQueryWithOptions(options);
IReadOnlyList<StorageFile> pdfFiles = await result.GetFilesAsync();
foreach (StorageFile file in pdfFiles)
{
    PdfDocument doc = await PdfDocument.LoadFromFileAsync(file);
    for (uint pageIdx = 0U; pageIdx < doc.PageCount; pageIdx++)
    {
        var page = doc.GetPage(pageIdx);
        SoftwareBitmap buffer;
        using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
        {
            await page.RenderToStreamAsync(stream);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            SoftwareBitmapSource source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softBitmap);
            buffer = softBitmap;
        }
        try
        {
            StorageFolder folderToSave = await searchFolder.CreateFolderAsync(file.Name.Replace(".pdf", ""), CreationCollisionOption.OpenIfExists);
            StorageFile fileToSave = await folderToSave.CreateFileAsync(file.Name.Replace(".pdf", "") + "-" + pageIdx + ".jpg");
            using (IRandomAccessStream stream = await fileToSave.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetSoftwareBitmap(buffer);
                await encoder.FlushAsync();
            }
        }
        catch (Exception) {}
    }
}
```
