using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace PianoSheetViewer
{
    public sealed partial class ViewPage : Page
    {
        static readonly DisplayRequest displayRequest = new DisplayRequest();
        PianoSheetInfo pianoSheetInfo;
        int viewPhase;
        
        public ViewPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.Black;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
            if (this.Frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
            displayRequest.RequestActive();
            pianoSheetInfo = e.Parameter as PianoSheetInfo;
            viewPhase = 0;
            await RenderView();
            base.OnNavigatedTo(e);
        }

        private void FitToScreen(ref ScrollViewer scrollViewer)
        {
            float zoomFactor = (float)Math.Min(scrollViewer.ActualWidth / pianoSheetInfo.Width, scrollViewer.ActualHeight / pianoSheetInfo.Height);
            scrollViewer.ChangeView(null, null, zoomFactor);
        }

        private async Task RenderView()
        {
            if (pianoSheetInfo is null)
            {
                return;
            }
            StorageFile file1 = pianoSheetInfo.PageFiles[(3 * viewPhase) % pianoSheetInfo.NumberOfPages];
            StorageFile file2 = pianoSheetInfo.PageFiles[(3 * viewPhase + 1) % pianoSheetInfo.NumberOfPages];
            StorageFile file3 = pianoSheetInfo.PageFiles[(3 * viewPhase + 2) % pianoSheetInfo.NumberOfPages];
            using (IRandomAccessStream fileStream = await file1.OpenReadAsync())
            {
                EffectsBrush1.LoadImageFromStream(fileStream);
            }
            using (IRandomAccessStream fileStream = await file2.OpenReadAsync())
            {
                EffectsBrush2.LoadImageFromStream(fileStream);
            }
            using (IRandomAccessStream fileStream = await file3.OpenReadAsync())
            {
                EffectsBrush3.LoadImageFromStream(fileStream);
            }
            FitToScreen(ref SheetScroller1);
            FitToScreen(ref SheetScroller2);
            FitToScreen(ref SheetScroller3);
        }

        private async void OnScreenTapped(object sender, TappedRoutedEventArgs e)
        {
            viewPhase += 1;
            await RenderView();
            e.Handled = true;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                try
                {
                    displayRequest.RequestRelease();
                }
                catch (Exception) { }
                this.Frame.GoBack();
                e.Handled = true;
            }
        }
    }
}
