using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TwainWpf;
using TwainWpf.TwainNative;
using TwainWpf.Win32;
using TwainWpf.Wpf;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static AreaSettings AreaSettings = new AreaSettings(Units.Centimeters, 0.1f, 5.7f, 0.1F + 2.6f, 5.7f + 2.6f);

        private Twain _twain;
        private ScanSettings _settings;
        private readonly IList<Bitmap> _resultBitmaps = new List<Bitmap>(); 
        private Bitmap resultImage;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += delegate
            {
                _twain = new Twain(new WindowMessageHook(this));
                _twain.TransferImage += delegate(object sender, TransferImageEventArgs args)
                {
                    if (args.Image != null)
                    {
                        resultImage = args.Image;
                        _resultBitmaps.Add(resultImage);
                        IntPtr hbitmap = new Bitmap(args.Image).GetHbitmap();
                        MainImage.Source = Imaging.CreateBitmapSourceFromHBitmap(
                                hbitmap,
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                        Gdi32Native.DeleteObject(hbitmap);
                    }
                };
                _twain.ScanningComplete += delegate
                {
                    IsEnabled = true;
                    MessageBox.Show(string.Format("{0} images scanned", _resultBitmaps.Count));
                };

                var sourceList = _twain.SourceNames;
                ManualSource.ItemsSource = sourceList;

                if (sourceList != null && sourceList.Count > 0)
                    ManualSource.SelectedItem = sourceList[0];
            };
        }

        private void OnSelectSourceButtonClick(object sender, RoutedEventArgs e)
        {
            _twain.SelectSource();
        }

        private void scanButton_Click(object sender, RoutedEventArgs e)
        {
            if (_resultBitmaps.Count > 0)
                _resultBitmaps.Clear();

            IsEnabled = false;

            _settings = new ScanSettings
            {
                UseDocumentFeeder = UseAdfCheckBox.IsChecked,
                ShowTwainUi = UseUICheckBox.IsChecked ?? false,
                ShowProgressIndicatorUi = ShowProgressCheckBox.IsChecked,
                UseDuplex = UseDuplexCheckBox.IsChecked,
                Resolution = (BlackAndWhiteCheckBox.IsChecked ?? false)
                                 ? ResolutionSettings.Fax
                                 : ResolutionSettings.ColourPhotocopier,
                Area = !(GrabAreaCheckBox.IsChecked ?? false) ? null : AreaSettings,
                ShouldTransferAllPages = true,
                Rotation = new RotationSettings
                {
                    AutomaticRotate = AutoRotateCheckBox.IsChecked ?? false,
                    AutomaticBorderDetection = AutoDetectBorderCheckBox.IsChecked ?? false
                }
            };

            try
            {
                if (SourceUserSelected.IsChecked == true)
                    _twain.SelectSource(ManualSource.SelectedItem.ToString());
                _twain.StartScanning(_settings);
            }
            catch (TwainException ex)
            {
                MessageBox.Show(ex.Message);
            }

            IsEnabled = true;
        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (resultImage != null)
            {
                var saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true)
                    resultImage.Save(saveFileDialog.FileName);
            }
        }
    }
}
