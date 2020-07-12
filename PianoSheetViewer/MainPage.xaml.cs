using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace PianoSheetViewer
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private PianoSheetInfo persistedItem;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();
            PianoSheets = new ObservableCollection<PianoSheetInfo>();
            SearchFolderPath = "Browse...";
            IsBusy = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.Black;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            base.OnNavigatedTo(e);
        }

        private async void StartConnectedAnimationForBackNavigation()
        {
            if (persistedItem != null)
            {
                SheetGridView.ScrollIntoView(persistedItem);
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("backAnimation");
                if (animation != null)
                {
                    await SheetGridView.TryStartConnectedAnimationAsync(animation, persistedItem, "ItemImage");
                }
            }
        }

        private void OnSheetClick(object sender, ItemClickEventArgs e)
        {
            persistedItem = e.ClickedItem as PianoSheetInfo;
            this.Frame.Navigate(typeof(ViewPage), persistedItem);
        }

        private async void OnSearchFolderClick(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folderPicked = await folderPicker.PickSingleFolderAsync();
            if (folderPicked != null)
            {
                IsBusy = true;
                UpdatePianoSheetsFromFolder(folderPicked);
                await Task.Delay(5000);
                IsBusy = false;
            }
        }

        private async void UpdatePianoSheetsFromFolder(StorageFolder searchFolder)
        {
            PianoSheets.Clear();
            SearchFolderPath = searchFolder.Path.Length > 0 ? searchFolder.Path : searchFolder.Name;
            StorageFolder temporaryFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("PianoSheetViewer", CreationCollisionOption.ReplaceExisting);
            await ConvertPortableDocumentFormatFilesAsync(searchFolder, temporaryFolder);
            await GetPianoSheetsAsync(true, temporaryFolder);
            await GetPianoSheetsAsync(false, searchFolder);
        }

        private async Task ConvertPortableDocumentFormatFilesAsync(StorageFolder searchFolder, StorageFolder temporaryFolder)
        {
            QueryOptions options = new QueryOptions();
            options.FolderDepth = FolderDepth.Shallow;
            options.FileTypeFilter.Add(".pdf");
            StorageFileQueryResult query = searchFolder.CreateFileQueryWithOptions(options);
            IReadOnlyList<StorageFile> pdfFiles = await query.GetFilesAsync();
            foreach (StorageFile pdfFile in pdfFiles)
            {
                PdfDocument pdfDoc = await PdfDocument.LoadFromFileAsync(pdfFile);
                for (uint pageIdx = 0U; pageIdx < pdfDoc.PageCount; pageIdx++)
                {
                    PdfPage pdfPage = pdfDoc.GetPage(pageIdx);
                    SoftwareBitmap buffer;
                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        await pdfPage.RenderToStreamAsync(stream);
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                        SoftwareBitmap softBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        SoftwareBitmapSource source = new SoftwareBitmapSource();
                        await source.SetBitmapAsync(softBitmap);
                        buffer = softBitmap;
                    }
                    try
                    {
                        StorageFolder folderToSave = await temporaryFolder.CreateFolderAsync(pdfFile.Name.Replace(".pdf", ""), CreationCollisionOption.OpenIfExists);
                        StorageFile fileToSave = await folderToSave.CreateFileAsync(pdfFile.Name.Replace(".pdf", "") + "-" + (pageIdx + 1U) + ".jpg");
                        using (IRandomAccessStream stream = await fileToSave.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                            encoder.SetSoftwareBitmap(buffer);
                            await encoder.FlushAsync();
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        private async Task GetPianoSheetsAsync(bool isPortableDocumentFormatFile, StorageFolder storageFolder)
        {
            QueryOptions options = new QueryOptions();
            options.FolderDepth = FolderDepth.Deep;
            options.FileTypeFilter.Add(".jpg");
            options.FileTypeFilter.Add(".png");
            options.FileTypeFilter.Add(".gif");
            StorageFileQueryResult query = storageFolder.CreateFileQueryWithOptions(options);
            IReadOnlyList<StorageFile> imageFiles = await query.GetFilesAsync();
            Dictionary<string, List<StorageFile>> parentFolderPathToImages = new Dictionary<string, List<StorageFile>>();
            foreach (StorageFile file in imageFiles)
            {
                string parentFolderPath = file.Path.Replace(file.Name, "");
                if (parentFolderPathToImages.ContainsKey(parentFolderPath))
                {
                    parentFolderPathToImages[parentFolderPath].Add(file);
                }
                else
                {
                    parentFolderPathToImages.Add(parentFolderPath, new List<StorageFile> { file });
                }
            }
            foreach (KeyValuePair<string, List<StorageFile>> kvp in parentFolderPathToImages)
            {
                kvp.Value.Sort((file1, file2) => file1.Name.CompareTo(file2.Name));
                string name = kvp.Key;
                string[] parentFolderPathSplited = kvp.Key.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                if (parentFolderPathSplited.Length > 1)
                {
                    name = parentFolderPathSplited[parentFolderPathSplited.Length - 2];
                }
                string fileType = String.Format("Images 📄×{0}", kvp.Value.Count); 
                if (isPortableDocumentFormatFile)
                {
                    fileType = String.Format("PDF Document 📄×{0}", kvp.Value.Count);
                }
                ImageProperties properties = await kvp.Value.First().Properties.GetImagePropertiesAsync();
                PianoSheetInfo pianoSheetInfo = new PianoSheetInfo(kvp.Value.First(), kvp.Value, name, fileType, properties);
                PianoSheets.Add(pianoSheetInfo);
            }
        }

        private void DetermineItemSize()
        {
            ItemSize = ZoomSlider.Value;
        }

        private void ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
                var image = (Image)templateRoot.FindName("ItemImage");
                image.Source = null;
            }
            if (args.Phase == 0)
            {
                args.RegisterUpdateCallback(ShowImageThumbnail);
                args.Handled = true;
            }
        }

        private async void ShowImageThumbnail(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase == 1)
            {
                var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
                var image = (Image)templateRoot.FindName("ItemImage");
                image.Opacity = 100;
                var item = args.Item as PianoSheetInfo;
                try
                {
                    image.Source = await item.GetImageThumbnailAsync();
                }
                catch (Exception)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.UriSource = new Uri(image.BaseUri, "Assets/StoreLogo.png");
                    image.Source = bitmapImage;
                }
            }
        }

        public ObservableCollection<PianoSheetInfo> PianoSheets { get; }

        public string SearchFolderPath
        {
            get => _searchFolderPath;
            set
            {
                if (_searchFolderPath != value)
                {
                    _searchFolderPath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchFolderPath)));
                }
            }
        }
        private string _searchFolderPath;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
                }
            }
        }
        private bool _isBusy;

        public double ItemSize
        {
            get => _itemSize;
            set
            {
                if (_itemSize != value)
                {
                    _itemSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemSize)));
                }
            }
        }
        private double _itemSize;
    }
}
