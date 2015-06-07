using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Estream.Cart42.Web.Helpers
{
    public static class ImageHelpers
    {
        public static Image Resize(this Image image, int width, int height, bool crop = false)
        {
            int sourceWidth = image.Width;
            int sourceHeight = image.Height;
            double destX = 0;
            double destY = 0;

            double nScale;
            double nScaleW = (width/(double) sourceWidth);
            double nScaleH = (height/(double) sourceHeight);
            if (!crop)
            {
                nScale = Math.Min(nScaleH, nScaleW);
            }
            else
            {
                nScale = Math.Max(nScaleH, nScaleW);
                destY = (height - sourceHeight*nScale)/2;
                destX = (width - sourceWidth*nScale)/2;
            }

            if (nScale > 1)
                nScale = 1;

            var destWidth = (int) Math.Round(sourceWidth*nScale);
            var destHeight = (int) Math.Round(sourceHeight*nScale);

            Bitmap bmPhoto;
            try
            {
                bmPhoto = new Bitmap(destWidth + (int) Math.Round(2*destX), destHeight + (int) Math.Round(2*destY));
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("destWidth:{0}, destX:{1}, destHeight:{2}, desxtY:{3}, Width:{4}, Height:{5}",
                        destWidth, destX, destHeight, destY, width, height), ex);
            }
            using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.CompositingQuality = CompositingQuality.HighQuality;
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.SmoothingMode = SmoothingMode.HighQuality;

                var to = new Rectangle((int) Math.Round(destX), (int) Math.Round(destY), destWidth, destHeight);
                var from = new Rectangle(0, 0, sourceWidth, sourceHeight);
                grPhoto.DrawImage(image, to, from, GraphicsUnit.Pixel);

                return bmPhoto;
            }
        }

/*
        public static Image Resize(this Image image, int? width, int? height, bool crop = false)
        {
            if (crop && width.HasValue && height.HasValue)
                return image.ResizeCrop(width.Value, height.Value);

            int originalWidth = image.Width;
            int originalHeight = image.Height;
            float percentWidth = (width ?? int.MaxValue) / (float)originalWidth;
            float percentHeight = (height ?? int.MaxValue) / (float)originalHeight;
            float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
            var newWidth = (int)(originalWidth * percent);
            var newHeight = (int)(originalHeight * percent);

            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }

        private static Image ResizeCrop(this Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            Rectangle srcRect = image.Width >= image.Height
                                    ? new Rectangle((image.Width - image.Height) / 2, 0, image.Height, image.Height)
                                    : new Rectangle(0, (image.Height - image.Width) / 2, image.Width, image.Width);

            var bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
            g.Dispose();

            return bmp;
        }
*/
    }
}