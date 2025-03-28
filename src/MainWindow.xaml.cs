using Microsoft.Win32;
using System.Drawing.Imaging; // For ImageFormat, EncoderParameters
using System.Windows;
using ImageMagick;
using System.Windows.Controls;
using System;

namespace ImageConverterApp
{
    public partial class MainWindow : Window
    {
        private string? inputFilePath; // Nullable to avoid constructor initialization warning
        private string? outputFilePath; // Nullable to avoid constructor initialization warning

        public MainWindow()
        {
            InitializeComponent();
        }

        // Select input image
        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                inputFilePath = openFileDialog.FileName;
                InputFileText.Text = inputFilePath;
            }
        }

        // Select output file location
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JPG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png|WebP Image (*.webp)|*.webp",
                DefaultExt = FormatComboBox.SelectedIndex switch
                {
                    0 => "jpg",
                    1 => "png",
                    2 => "webp",
                    _ => "jpg"
                }
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                outputFilePath = saveFileDialog.FileName;
                OutputFileText.Text = outputFilePath;
            }
        }

        // Convert the image
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(outputFilePath))
            {
                StatusText.Text = "Please select input and output files.";
                return;
            }

            try
            {
                StatusText.Text = "Converting...";
                string selectedFormat = (FormatComboBox.SelectedItem as ComboBoxItem)?.Content.ToString().ToLower() ?? "jpg";

                if (selectedFormat == "jpg")
                {
                    using (System.Drawing.Image image = System.Drawing.Image.FromFile(inputFilePath))
                    {
                        EncoderParameters encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)QualitySlider.Value);
                        ImageCodecInfo jpgCodec = GetEncoder(ImageFormat.Jpeg);
                        image.Save(outputFilePath, jpgCodec, encoderParams);
                    }
                }
                else if (selectedFormat == "png")
                {
                    using (System.Drawing.Image image = System.Drawing.Image.FromFile(inputFilePath))
                    {
                        image.Save(outputFilePath, ImageFormat.Png);
                    }
                }
                else if (selectedFormat == "webp")
                {
                    using (var magickImage = new MagickImage(inputFilePath))
                    {
                        magickImage.Quality = (int)QualitySlider.Value;
                        magickImage.Write(outputFilePath, MagickFormat.WebP);
                    }
                }

                StatusText.Text = "Conversion complete!";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        // Helper method to get encoder for a format
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            throw new InvalidOperationException($"No encoder found for format {format}");
        }
    }
}
