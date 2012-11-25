using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SlXnaApp1.Infrastructure
{
    public static class MosaicPictureGenerator
    {
        public static string GenereateMosaicPicture(IList<BitmapImage> images, string filename)
        {
            string key = string.Empty;

            try
            {
                // Generate avatar in bitmap.
                var bmp = new WriteableBitmap(PICTURE_DIMENSION, PICTURE_DIMENSION);
                bmp.Clear();
                int x = 0;
                int y = 0;

                int w = 0;
                int h = 0;
                if (images.Count == 1)
                {
                    w = PICTURE_DIMENSION;
                    h = PICTURE_DIMENSION;
                }
                else if (images.Count == 2)
                {
                    w = PART_DIMENSION;
                    h = PICTURE_DIMENSION;
                }
                else
                {
                    w = PART_DIMENSION;
                    h = PART_DIMENSION;
                }

                for (int i = 0; i < images.Count && i < 4; )
                {
                    if (images[i] == null)
                        continue;

                    var wb = new WriteableBitmap(images[i]);

                    if (wb.PixelHeight < h || wb.PixelWidth < w)
                    {
                        wb = wb.Resize(PICTURE_DIMENSION, PICTURE_DIMENSION, WriteableBitmapExtensions.Interpolation.Bilinear);
                    }

                    i++;

                    bmp.Blit(new Rect(x, y, w, h), wb,
                        new Rect(0, 0, w, h));

                    if (w == PART_DIMENSION)
                        x += w + 2; // Handle empty space between photos
                    else
                        x += w;

                    if (x >= PICTURE_DIMENSION)
                    {
                        x = 0;

                        if (h == PART_DIMENSION)
                            y += h + 2; // Handle empty space between photos
                        else
                            y += h;
                    }

                    if (y >= PICTURE_DIMENSION)
                        break;
                }

                if (_SaveJpeg(bmp, filename))
                    key = filename;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GenereateMosaicPicture failed." + ex.Message);
            }

            return key;
        }

        private static bool _SaveJpeg(WriteableBitmap image, string filename)
        {
            IsolatedStorageFile filesystem = null;
            IsolatedStorageFileStream fs = null;
            bool result = false;

            try
            {
                filesystem = IsolatedStorageFile.GetUserStoreForApplication();

                if (!filesystem.FileExists(filename + ".jpg"))
                {
                    fs = new IsolatedStorageFileStream(filename + ".jpg", FileMode.CreateNew, filesystem);
                    image.SaveJpeg(fs, PICTURE_DIMENSION, PICTURE_DIMENSION, 0, 70);
                }

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_SaveJpeg failed: " + ex.Message);
            }
            finally
            {
                if (filesystem != null)
                    filesystem.Dispose();

                if (fs != null)
                    fs.Dispose();
            }

            return result;
        }

        private const int PART_DIMENSION = 35;
        private const int PICTURE_DIMENSION = 74;
    }
}
