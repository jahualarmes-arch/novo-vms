using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace VMS_AlarmesJahu.App.Views.Controls;

/// <summary>
/// Utilitário para converter bytes JPEG em imagens WPF
/// </summary>
public static class ImageUtil
{
    /// <summary>
    /// Converte array de bytes JPEG para BitmapImage WPF
    /// </summary>
    public static BitmapImage? BytesToBitmapImage(byte[]? jpegData)
    {
        if (jpegData == null || jpegData.Length == 0)
            return null;

        try
        {
            var bitmap = new BitmapImage();
            using (var ms = new MemoryStream(jpegData))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
            }
            bitmap.Freeze(); // Importante para thread-safety
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converte array de bytes JPEG para BitmapSource (mais rápido, sem freeze)
    /// </summary>
    public static BitmapSource? BytesToBitmapSource(byte[]? jpegData)
    {
        if (jpegData == null || jpegData.Length == 0)
            return null;

        try
        {
            using var ms = new MemoryStream(jpegData);
            var decoder = new JpegBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            var frame = decoder.Frames[0];
            frame.Freeze();
            return frame;
        }
        catch
        {
            return null;
        }
    }
}
