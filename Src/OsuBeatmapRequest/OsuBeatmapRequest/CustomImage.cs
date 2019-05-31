using OsuBeatmapRequest.OsuRelated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OsuBeatmapRequest
{
    class CustomImage : Image
    {
        public Covers ImageCover
        {
            get { return (Covers)GetValue(ImageCoverProperty); }
            set { SetValue(ImageCoverProperty, value); }
        }
        public static readonly DependencyProperty ImageCoverProperty =
            DependencyProperty.Register("ImageCover", typeof(Covers), typeof(CustomImage), new PropertyMetadata(ImageCoverChanged));

        private static async void ImageCoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ins = d as CustomImage;
            var info = e.NewValue as Covers;
            MemoryStream memStream = null;
            BitmapImage imgSrc = null;

            try
            {
                await Task.Run(() =>
                {
                    if (info != null)
                    {
                        if (!string.IsNullOrEmpty(info.cover2x))
                        {
                            memStream = GetOnlineImage(info.cover2x);
                        }
                        else if (!string.IsNullOrEmpty(info.cover))
                        {
                            memStream = GetOnlineImage(info.cover);
                        }
                        else if (!string.IsNullOrEmpty(info.card))
                        {
                            memStream = GetOnlineImage(info.card);
                        }

                        if (memStream != null)
                        {
                            memStream.Position = 0;
                            imgSrc = new BitmapImage();
                            imgSrc.BeginInit();
                            imgSrc.StreamSource = memStream;
                            imgSrc.EndInit();
                            imgSrc.Freeze();
                        }
                    }
                });

                if (imgSrc != null)
                {
                    imgSrc = imgSrc.Clone();
                    ins.Source = imgSrc;
                }
                else
                {
                    ins.Source = new BitmapImage();
                }
            }
            catch (Exception err)
            {
                ins.Source = new BitmapImage();
            }
        }

        private static MemoryStream GetOnlineImage(string url)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                using (var resp = req.GetResponse())
                {
                    using (var stream = resp.GetResponseStream())
                    {
                        var memStream = new MemoryStream();
                        stream.CopyTo(memStream);
                        return memStream;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
