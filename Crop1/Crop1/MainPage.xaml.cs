using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Input;
using System.Threading.Tasks;
using Windows.Graphics.Display;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Crop1
{
  
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private TranslateTransform dragTranslation;
        private TranslateTransform dragTranslationElips;

        private CompositeTransform deltaTransform;
        private CompositeTransform deltaTransformElips;

        private TransformGroup rectangleTransforms;
        private TransformGroup elipsTransform;
        private bool b = true;
        private bool t = true;
        public MainPage()
        {
            this.InitializeComponent();

            dragTranslation = new TranslateTransform();
            deltaTransform = new CompositeTransform();
            rectangleTransforms = new TransformGroup();

            dragTranslationElips = new TranslateTransform();
            deltaTransformElips = new CompositeTransform();
            elipsTransform = new TransformGroup();


            //rectangleGrid transform
            rectangleTransforms.Children.Add(dragTranslation);
            rectangleTransforms.Children.Add(deltaTransform);
            //Elips transform
            elipsTransform.Children.Add(dragTranslationElips);
            elipsTransform.Children.Add(deltaTransformElips);

            rectangleGrid.RenderTransform = rectangleTransforms;

            elips.RenderTransform = elipsTransform;

            rectangleGrid.RenderTransform = rectangleTransforms;
            elips.RenderTransform = elipsTransform;

            rect.ManipulationDelta += Rect_ManipulationDelta;
            elips.ManipulationDelta += Elips_ManipulationDelta;
        }

        private void Elips_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Scale;
            if ((rect.Height + 2 * e.Delta.Translation.Y) > rect.MinHeight && (rect.Height + 2 * e.Delta.Translation.Y) < rect.MaxHeight)
            {
                rect.Height += 2 * e.Delta.Translation.Y;
                dragTranslationElips.Y += e.Delta.Translation.Y;
            }
            if ((rect.Width + 2 * e.Delta.Translation.X) > rect.MinWidth && (rect.Width + 2 * e.Delta.Translation.X) < rect.MaxWidth)
            {
                rect.Width += 2 * e.Delta.Translation.X;
                dragTranslationElips.X += e.Delta.Translation.X;
            }
        }
        private void Rect_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Scale;

            // Move rectangle.
            dragTranslation.X += e.Delta.Translation.X;
            dragTranslation.Y += e.Delta.Translation.Y; 
        }


        #region Crop Cut Rotate
        private void Crop_Click(object sender, RoutedEventArgs e)
        {
             if (imageControl.Source != null)
            {
                if (t)
                {
                    t = false;
                    rect.Visibility = Visibility.Visible;
                    elips.Visibility = Visibility.Visible;

                }
                else
                {
                    t = true;
                    rect.Visibility = Visibility.Collapsed;
                    elips.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            if (!t)
            {
                GeneralTransform transformToVisual = rect.TransformToVisual(GGrid);
                Rect tempRect = transformToVisual.TransformBounds(new Rect(0,0,rect.Width , rect.Height));
                RectangleGeometry geometry = new RectangleGeometry();
                geometry.Rect = tempRect;
                GGrid.Clip = geometry;
                rect.Visibility = Visibility.Collapsed;
                elips.Visibility = Visibility.Collapsed;

            }
        }
        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            if (imageControl.Source != null)
            {
                if (b)
                {
                    b = false;
                    slid.Visibility = Visibility.Visible;
                    GGrid.Children.Add(slid);
                }
                else
                {
                    b = true;
                    slid.Visibility = Visibility.Collapsed;
                    GGrid.Children.Remove(slid);
                }
            }
        }
#endregion


        #region Open Save
        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            StorageFile inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }
            SoftwareBitmap softwareBitmap;
            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
    softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            SoftwareBitmapSource source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);
            imageControl.Source = source;
        }


        private async void Save(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(g);
            IBuffer pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileSavePicker.FileTypeChoices.Add("JPEG files", new List<string>() {  ".png"});
            fileSavePicker.SuggestedFileName = "image";

            StorageFile outputFile = await fileSavePicker.PickSaveFileAsync();
            if (outputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
               
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    (uint)renderTargetBitmap.PixelWidth,
                    (uint)renderTargetBitmap.PixelHeight,
                    96,96,
                    pixelBuffer.ToArray());
                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    switch (err.HResult)
                    {
                        case unchecked((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
                                                         // If the encoder does not support writing a thumbnail, then try again
                                                         // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw err;
                    }
                }

            }
        }



        #endregion

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var a = Window.Current.Bounds;

        }
    }
}
