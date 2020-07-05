using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace PianoSheetViewer
{
    public class PianoSheetInfo
    {
        public PianoSheetInfo(StorageFile coverFile, List<StorageFile> pageFiles, string name, string fileType, ImageProperties properties)
        {
            CoverFile = coverFile;
            PageFiles = pageFiles;
            Name = name;
            FileType = fileType;
            Width = properties.Width;
            Height = properties.Height;
        }

        public StorageFile CoverFile { get; }

        public List<StorageFile> PageFiles { get; }

        public string Name { get; }

        public string FileType { get; }

        public uint Width { get; }

        public uint Height { get; }

        public int NumberOfPages
        {
            get
            {
                if (PageFiles is null)
                {
                    return 0;
                }
                return PageFiles.Count;
            }
        }

        public async Task<BitmapImage> GetImageThumbnailAsync()
        {
            var thumbnail = await CoverFile.GetThumbnailAsync(ThumbnailMode.PicturesView);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(thumbnail);
            thumbnail.Dispose();
            return bitmapImage;
        }
    }
}
