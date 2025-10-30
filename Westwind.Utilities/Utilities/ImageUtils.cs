#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          � West Wind Technologies, 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/12/2009
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

#if NETFULL
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
#endif

using System;
using System.Collections.Generic;
using System.IO;

namespace Westwind.Utilities
{
    /// <summary>
    /// Summary description for wwImaging.
    /// </summary>
    public static class ImageUtils
    {

#if NETFULL
        /// <summary>
        /// Creates a resized bitmap from an existing image on disk. Resizes the image by 
        /// creating an aspect ratio safe image. Image is sized to the larger size of width
        /// height and then smaller size is adjusted by aspect ratio.
        /// 
        /// Image is returned as Bitmap - call Dispose() on the returned Bitmap object
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>Bitmap or null</returns>
        public static Bitmap ResizeImage(string filename, int width, int height, InterpolationMode mode = InterpolationMode.HighQualityBicubic)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(filename))
                {
                    return ResizeImage(bmp, width, height, mode);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resizes an image from byte array and returns a Bitmap.
        /// Make sure you Dispose() the bitmap after you're done 
        /// with it!
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap ResizeImage(byte[] data, int width, int height, InterpolationMode mode = InterpolationMode.HighQualityBicubic)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(new MemoryStream(data)))
                {
                    return ResizeImage(bmp, width, height, mode);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resizes an image and saves the image to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="outputFilename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="jpegCompressionMode">
        /// If using a jpeg image 
        /// </param>
        /// <returns></returns>
        public static bool ResizeImage(string filename, string outputFilename,
                                        int width, int height,
                                        InterpolationMode mode = InterpolationMode.HighQualityBicubic,
                                        int jpegCompressionMode = 85)
        {

            using (var bmpOut = ResizeImage(filename, width, height, mode))
            {
                var imageFormat = GetImageFormatFromFilename(filename);
                if (imageFormat == ImageFormat.Emf)
                    imageFormat = bmpOut.RawFormat;

                if(imageFormat == ImageFormat.Jpeg)
                    SaveJpeg(bmpOut, outputFilename, jpegCompressionMode);
                else
                    bmpOut.Save(outputFilename, imageFormat);
            }

            return true;
        }


        /// <summary>
        /// Resizes an image from a bitmap.
        /// Note image will resize to the larger of the two sides
        /// </summary>
        /// <param name="bmp">Bitmap to resize</param>
        /// <param name="width">new width</param>
        /// <param name="height">new height</param>
        /// <returns>resized or original bitmap. Be sure to Dispose this bitmap</returns>
        public static Bitmap ResizeImage(Bitmap bmp, int width, int height,
                                         InterpolationMode mode = InterpolationMode.HighQualityBicubic)
        {
            Bitmap bmpOut = null;

            try
            {
                decimal ratio;
                int newWidth = 0;
                int newHeight = 0;

                // If the image is smaller than a thumbnail just return original size
                if (bmp.Width < width && bmp.Height < height)
                {
                    newWidth = bmp.Width;
                    newHeight = bmp.Height;
                }
                else
                {
                    if (bmp.Width == bmp.Height)
                    {
                        if (height > width)
                        {
                            newHeight = height;
                            newWidth = height;
                        }
                        else
                        {
                            newHeight = width;
                            newWidth = width;
                        }
                    }
                    else if (bmp.Width >= bmp.Height)
                    {
                        ratio = (decimal)width / bmp.Width;
                        newWidth = width;
                        decimal lnTemp = bmp.Height * ratio;
                        newHeight = (int)lnTemp;
                    }
                    else
                    {
                        ratio = (decimal)height / bmp.Height;
                        newHeight = height;
                        decimal lnTemp = bmp.Width * ratio;
                        newWidth = (int)lnTemp;
                    }
                }
                                    
                bmpOut = new Bitmap(newWidth, newHeight);
                bmpOut.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

                using (Graphics g = Graphics.FromImage(bmpOut))
                {
                    g.InterpolationMode = mode;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                    g.DrawImage(bmp, 0, 0, newWidth, newHeight);
                }
            }
            catch
            {
                return null;
            }

            return bmpOut;
        }

        /// <summary>
        /// Adjusts an image to a specific aspect ratio by clipping
        /// from the center outward - essentially capturing the center
        /// to fit the width/height of the aspect ratio.
        /// </summary>
        /// <param name="imageStream">Stream to an image</param>
        /// <param name="ratio">Aspect ratio default is 16:9</param>
        /// <param name="resizeWidth">Optionally resize with to this width (if larger than height)</param>
        /// <param name="resizeHeight">Optionally resize to this height (if larger than width)</param>
        /// <returns>Bitmap image - make sure to dispose this image</returns>
        public static Bitmap AdjustImageToRatio(Stream imageStream, decimal ratio = 16M / 9M, int resizeWidth = 0,
            int resizeHeight = 0)
        {
            if (imageStream == null)
                return null;

            decimal width = 0;
            decimal height = 0;


            Bitmap bmpOut = null;
            Bitmap bitmap = null;

            try
            {
                bitmap = new Bitmap(imageStream);

                height = bitmap.Height;
                width = bitmap.Width;

                if (width >= height * ratio)
                {
                    // clip width
                    decimal clipWidth = height * ratio;
                    decimal clipX = (width - clipWidth) / 2;

                    bmpOut = new Bitmap((int) clipWidth, (int) height);
                    bmpOut.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

                    using (Graphics g = Graphics.FromImage(bmpOut))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;


                        var sourceRect = new Rectangle((int) clipX, 0, (int) clipWidth, (int) height);
                        var targetRect = new Rectangle(0, 0, (int) clipWidth, (int) height);

                        g.DrawImage(bitmap, targetRect, sourceRect, GraphicsUnit.Pixel);
                    }
                }
                else if (width < height * ratio)
                {
                    // clip height
                    decimal clipHeight = width / ratio;
                    decimal clipY = (height - clipHeight) / 2;

                    bmpOut = new Bitmap((int) width, (int) clipHeight);
                    bmpOut.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

                    using (Graphics g = Graphics.FromImage(bmpOut))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        var sourceRect = new Rectangle(0, (int) clipY, (int) width, (int) clipHeight);
                        var targetRect = new Rectangle(0, 0, (int) width, (int) clipHeight);

                        g.DrawImage(bitmap, targetRect, sourceRect, GraphicsUnit.Pixel);
                    }
                }
                else
                    bmpOut = bitmap;

                if (resizeWidth == 0 || resizeWidth == 0)
                    return bmpOut;

                var resizedImage = ResizeImage(bmpOut, resizeWidth, resizeHeight);
                return resizedImage;
            }
            finally
            {
                bitmap?.Dispose();
                bmpOut?.Dispose();
            }
        }


        /// <summary>
        /// Adjusts an image to a specific aspect ratio by clipping
        /// from the center outward - essentially capturing the center
        /// to fit the width/height of the aspect ratio.
        /// </summary>
        /// <param name="imageContent"></param>
        /// <param name="ratio"></param>
        /// <param name="resizeWidth"></param>
        /// <param name="resizeHeight"></param>
        /// <returns></returns>
        public static Bitmap AdjustImageToRatio(byte[] imageContent, decimal ratio = 16M / 9M, int resizeWidth = 0,
            int resizeHeight = 0)
        {
            using (var ms = new MemoryStream(imageContent))
            {
                return AdjustImageToRatio(ms, ratio, resizeWidth, resizeHeight);
            }
        }


        /// <summary>
        /// Saves a jpeg BitMap  to disk with a jpeg quality setting.
        /// Does not dispose the bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap to save</param>
        /// <param name="outputFileName">file to write it to</param>
        /// <param name="jpegQuality"></param>
        /// <returns></returns>
        public static bool SaveJpeg(Bitmap bmp, string outputFileName, long jpegQuality = 90)
        {
            try
            {
                //get the jpeg codec
                ImageCodecInfo jpegCodec = null;
                if (Encoders.ContainsKey("image/jpeg"))
                    jpegCodec = Encoders["image/jpeg"];

                EncoderParameters encoderParams = null;
                if (jpegCodec != null)
                {
                    //create an encoder parameter for the image quality
                    EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, jpegQuality);

                    //create a collection of all parameters that we will pass to the encoder
                    encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = qualityParam;
                }
                bmp.Save(outputFileName, jpegCodec, encoderParams);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves a jpeg BitMap  to disk with a jpeg quality setting.
        /// Does not dispose the bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap to save</param>
        /// <param name="outputStream">Binary stream to write image data to</param>
        /// <param name="jpegQuality"></param>
        /// <returns></returns>
        public static bool SaveJpeg(Bitmap bmp, Stream imageStream, long jpegQuality = 90)
        {
            try
            {
                //get the jpeg codec
                ImageCodecInfo jpegCodec = null;
                if (Encoders.ContainsKey("image/jpeg"))
                    jpegCodec = Encoders["image/jpeg"];

                EncoderParameters encoderParams = null;
                if (jpegCodec != null)
                {
                    //create an encoder parameter for the image quality
                    EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, jpegQuality);

                    //create a collection of all parameters that we will pass to the encoder
                    encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = qualityParam;
                }
                bmp.Save(imageStream, jpegCodec, encoderParams);
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Rotates an image and writes out the rotated image to a file.
        /// </summary>
        /// <param name="filename">The original image to roatate</param>
        /// <param name="outputFilename">The output file of the rotated image file. If not passed the original file is overwritten</param>
        /// <param name="type">Type of rotation to perform</param>
        /// <returns></returns>
        public static bool RoateImage(string filename, string outputFilename = null,
                                      RotateFlipType type = RotateFlipType.Rotate90FlipNone,
                                      int jpegCompressionMode = 85)
        {
            Bitmap bmpOut = null;

            if (string.IsNullOrEmpty(outputFilename))
                outputFilename = filename;

            try
            {
                ImageFormat imageFormat;
                using (Bitmap bmp = new Bitmap(filename))
                {
                    imageFormat = GetImageFormatFromFilename(filename);
                    if (imageFormat == ImageFormat.Emf)
                        imageFormat = bmp.RawFormat;

                    bmp.RotateFlip(type);

                    using (bmpOut = new Bitmap(bmp.Width, bmp.Height))
                    {
                        bmpOut.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

                        Graphics g = Graphics.FromImage(bmpOut);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bmp, 0, 0, bmpOut.Width, bmpOut.Height);
                        
                        if (imageFormat == ImageFormat.Jpeg)
                            SaveJpeg(bmpOut, outputFilename, jpegCompressionMode);
                        else
                            bmpOut.Save(outputFilename, imageFormat);                                
                    }                    
                }

            }
            catch (Exception ex)
            {
                var msg = ex.GetBaseException();
                return false;
            }

            return true;
        }

        public static byte[] RoateImage(byte[] data, RotateFlipType type = RotateFlipType.Rotate90FlipNone)
        {
            Bitmap bmpOut = null;

            try
            {
                Bitmap bmp = new Bitmap(new MemoryStream(data));

                ImageFormat imageFormat;
                imageFormat = bmp.RawFormat;
                bmp.RotateFlip(type);

                bmpOut = new Bitmap(bmp.Width, bmp.Height);
                bmpOut.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

                Graphics g = Graphics.FromImage(bmpOut);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, 0, 0, bmpOut.Width, bmpOut.Height);

                bmp.Dispose();

                using (var ms = new MemoryStream())
                {
                    bmpOut.Save(ms, imageFormat);
                    bmpOut.Dispose();

                    ms.Flush();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.GetBaseException();
                return null;
            }

        }


        /// <summary>
        /// Opens the image and writes it back out, stripping any Exif data
        /// </summary>
        /// <param name="imageFile">Image to remove exif data from</param>
        /// <param name="imageQuality">image quality 0-100 (100 no compression)</param>
        public static void StripJpgExifData(string imageFile, int imageQuality = 90)
        {
            using (var bmp = new Bitmap(imageFile))
            {
                using (var bmp2 = new Bitmap(bmp, bmp.Width, bmp.Height))
                {
                    bmp.Dispose();
                    SaveJpeg(bmp2, imageFile, imageQuality);
                }
            }
        }


        /// <summary>
        /// If the image contains image rotation Exif data, apply the image rotation and
        /// remove the Exif data. Optionally also allows for image resizing in the same
        /// operation.
        /// </summary>
        /// <param name="imageFile">Image file to work on</param>
        /// <param name="imageQuality">Jpg</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void NormalizeJpgImageRotation(string imageFile, int imageQuality = 90, int width = -1, int height = -1)
        {
            using (var bmp = new Bitmap(imageFile))
            {
                Bitmap bmp2;
                using (bmp2 = new Bitmap(bmp, bmp.Width, bmp.Height))
                {
                    if (bmp.PropertyItems != null)
                    {
                        foreach (var item in bmp.PropertyItems)
                        {
                            if (item.Id == 0x112)
                            {
                                int orientation = item.Value[0];
                                if (orientation == 6)
                                    bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                if (orientation == 8)
                                    bmp2.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            }
                        }
                    }

                    bmp.Dispose();

                    if (width > 0 || height > 0)
                        bmp2 = ResizeImage(bmp2, width, height);

                    SaveJpeg(bmp2, imageFile, imageQuality);
                }
            }
        }



        /// <summary>
        /// A quick lookup for getting image encoders
        /// </summary>
        public static Dictionary<string, ImageCodecInfo> Encoders
        {
            //get accessor that creates the dictionary on demand
            get
            {
                //if the quick lookup isn't initialised, initialise it
                if (_encoders != null)
                    return _encoders;

                _encoders = new Dictionary<string, ImageCodecInfo>();

                //get all the codecs
                foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                {
                    //add each codec to the quick lookup
                    _encoders.Add(codec.MimeType.ToLower(), codec);
                }

                //return the lookup
                return _encoders;
            }
        }
        private static Dictionary<string, ImageCodecInfo> _encoders = null;


        /// <summary>
        /// Tries to return an image format 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Image format or ImageFormat.Emf if no match was found</returns>
        public static ImageFormat GetImageFormatFromFilename(string filename)
        {            
            string ext = Path.GetExtension(filename).ToLower();

            ImageFormat imageFormat;

            if (ext == ".jpg" || ext == ".jpeg")
                imageFormat = ImageFormat.Jpeg;
            else if (ext == ".png")
                imageFormat = ImageFormat.Png;
            else if (ext == ".gif")
                imageFormat = ImageFormat.Gif;
            else if (ext == ".bmp")
                imageFormat = ImageFormat.Bmp;
            else
                imageFormat = ImageFormat.Emf;

            return imageFormat;
        }
#endif

        /// <summary>
        /// Returns the image media type for a give file extension based
        /// on a filename or url passed in.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetImageMediaTypeFromFilename(string file)
        {
            if (string.IsNullOrEmpty(file))
                return file;

            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".jpg" || ext == ".jpeg")
                return "image/jpeg";
            if (ext == ".png")
                return "image/png";
            if (ext == ".apng")
                return "image/apgn";
            if (ext == ".gif")
                return "image/gif";
            if (ext == ".bmp")
                return "image/bmp";
            if (ext == ".tif" || ext == ".tiff")
                return "image/tiff";
            if (ext == ".webp")
                return "image/webp";
            if (ext == ".ico")
                return "image/x-icon";


            return "application/image";
        }


        /// <summary>
        /// Returns a file extension for a media type/content type.
        /// </summary>
        /// <param name="mediaType">The media type to convert from</param>
        /// <returns>A file extension or null if no image type is found</returns>
        public static string GetExtensionFromMediaType(string mediaType)
        {
            if (mediaType == "image/jpeg")
                return "jpg";
            if (mediaType == "image/png")
                return "png";
            if (mediaType == "image/apng")
                return "apng";
            if (mediaType == "image/bmp")
                return "bmp";
            if (mediaType == "image/tiff")
                return "tif";
            if (mediaType == "image/svg+xml")
                return "svg";
            if (mediaType == "image/webp")
                return "webp";
            if (mediaType == "image/x-icon")
                return "ico";


            return null;
        }

        /// <summary>
        /// Determines whether a filename has a typical image extension
        /// </summary>
        /// <param name="filename">File name to check for image</param>
        /// <returns>true or false</returns>
        public static bool IsImage(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return false;

            string ext = Path.GetExtension(filename).ToLower();
            if (ext == ".jpg" || ext == ".jpeg" ||
                ext == ".png" ||
                ext == ".apng" ||
                ext == ".gif" ||
                ext == ".bmp" ||
                ext == ".tif" || ext == ".tiff" ||
                ext == ".webp" ||
                ext == ".ico")
                return true;

            return false;
        }

        /// <summary>
        /// Does a byte array check to if the data is an image type
        /// Supports PNG, JPEG, GIF, BMP, WebP and AVIF formats.
        /// </summary>
        /// <param name="data">Buffer only needs the first 15 bytes</param>
        /// <returns>true or false</returns>
        public static bool IsImage(byte[] data)
        {
            if (data == null || data.Length < 4)
                return false;

            // PNG (also covers APNG, which is an extension of PNG)
            if (data.Length >= 8 &&
                data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E &&
                data[3] == 0x47 && data[4] == 0x0D && data[5] == 0x0A &&
                data[6] == 0x1A && data[7] == 0x0A)
            {
                return true;
            }

            // JPEG
            if (data.Length >= 4 &&
                data[0] == 0xFF && data[1] == 0xD8 &&
                data[data.Length - 2] == 0xFF && data[data.Length - 1] == 0xD9)
            {
                return true;
            }

            // GIF
            if (data.Length >= 6 &&
                data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 &&
                data[3] == 0x38 && (data[4] == 0x39 || data[4] == 0x37) && data[5] == 0x61)
            {
                return true;
            }

            // BMP
            if (data.Length >= 2 &&
                data[0] == 0x42 && data[1] == 0x4D)
            {
                return true;
            }

            // WebP: starts with 'RIFF' + 4 bytes + 'WEBP'
            if (data.Length >= 12 &&
                data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
            {
                return true;
            }

            // AVIF: starts with 'ftyp' + 'avif' or 'avis' box brand
            // ISO BMFF files start with 4-byte box size then 'ftyp'
            if (data.Length >= 12 &&
                data[4] == 0x66 && data[5] == 0x74 && data[6] == 0x79 && data[7] == 0x70)
            {
                // Check major brand
                string brand = System.Text.Encoding.ASCII.GetString(data, 8, 4);
                if (brand == "avif" || brand == "avis")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
