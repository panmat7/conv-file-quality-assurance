using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace AvaloniaDraft.ComparingMethods;

public static class DocumentSegmentation
{
    /// <summary>
    /// Segments a document image into points of interest and returns them as a list of rectangles. 
    /// </summary>
    /// <param name="imageFilePath">Path to the image.</param>
    /// <param name="darkBackground">Whether the document has a white or black background.</param>
    /// <returns>List of rectangles representing each segment. Null if an error occured.</returns>
    public static List<Rectangle>? SegmentDocumentImage(string imageFilePath, bool darkBackground = false)
    {
        try
        {
            var img = CvInvoke.Imread(imageFilePath);

            if (img is null) return null;

            return GetRects(img, darkBackground);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Segments a document image into points of interest and returns them as a list of rectangles. 
    /// </summary>
    /// <param name="imageBytes">The image as a byte array.</param>
    /// <param name="darkBackground">Whether the document has a white or black background.</param>
    /// <returns>List of rectangles representing each segment. Null if an error occured.</returns>
    public static List<Rectangle>? SegmentDocumentImage(byte[] imageBytes, bool darkBackground = false)
    {
        try
        {
            var img = new Mat();

            CvInvoke.Imdecode(imageBytes, ImreadModes.Unchanged, img); //Reading the image

            return GetRects(img, darkBackground);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Preforms the entire image segmentation on the inputted image.
    /// </summary>
    /// <param name="img">The image as Emgu.Cv.Mat.</param>
    /// <param name="darkBackground">Whether the document has a white or black background.</param>
    /// <param name="presentation">Whether the document is a presentation, if yes will apply additional iteration for
    /// morphing resulting is better grouping for lager objects.</param>
    /// <returns>List of segments ad rectangles. Null if an error occured.</returns>
    private static List<Rectangle>? GetRects(Mat img, bool darkBackground = false, bool presentation = false)
    {
        try
        {
            //Converting image to grayscale
            var grayscale = new Mat();
            CvInvoke.CvtColor(img, grayscale, ColorConversion.Bgr2Gray);

            var threshold = new Mat();
            //Finding best threshold using Otsu
            var thresholdValue = CvInvoke.Threshold(grayscale, new Mat(), 0, 255, ThresholdType.Otsu);
            
            //Thresholding
            CvInvoke.Threshold(grayscale, threshold, thresholdValue, 255,
                !darkBackground ? ThresholdType.BinaryInv : ThresholdType.Binary); //Using BinaryInv for light and Binary for dark backgrounds

            //Morphing to connect parts together
            var kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(-1, -1));
            var morph = new Mat();
            CvInvoke.MorphologyEx(threshold, morph, MorphOp.Dilate, kernel, new Point(-1, -1), (presentation ? 4 : 3), BorderType.Default,
                new MCvScalar());

            //Finding contours
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(morph, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            var rects = new List<Rectangle>();

            for (var i = 0; i < contours.Size; i++)
            {
                var rect = CvInvoke.BoundingRectangle(contours[i]);

                if (rect.Width > 10 && rect.Height > 10)
                    rects.Add(rect); //Not adding to smallest of rectangles to avoid potential noise 
            }

            rects.Sort((a, b) => (b.Width * b.Height).CompareTo(a.Width * a.Height)); //Sort by area

            var finalRects = new List<Rectangle>();

            //Filtering out any smaller rectangles nested inside other
            for (var i = 0; i < rects.Count; i++)
            {
                var nested = false;
                for (var j = 0; j < i; j++)
                {
                    if (rects[j].Contains(rects[i]))
                    {
                        nested = true;
                        break;
                    }
                }

                if (!nested) finalRects.Add(rects[i]);
            }

            return finalRects;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Separates out the segments based on the given rectangles and returns them as bytes.
    /// </summary>
    /// <param name="filePath">Path to the image to be segmented.</param>
    /// <param name="rects">Segment coordinates.</param>
    /// <returns>List of all segment pictures, or null if an error occured.</returns>
    public static List<byte[]>? GetSegmentPictures(string filePath, List<Rectangle> rects)
    {
        try
        {
            if(filePath == "" || rects.Count == 0) return null;
            
            var img = CvInvoke.Imread(filePath);
            if (img is null) return null;

            var segments = new List<byte[]>();

            foreach (var rect in rects)
            {
                var cropped = new Mat(img, rect);
                var buf = new VectorOfByte();

                CvInvoke.Imencode(".png", cropped, buf);

                segments.Add(buf.ToArray());
            }

            return segments;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Separates out the segments based on the given rectangles and returns them as bytes.
    /// </summary>
    /// <param name="imageBytes">Image to be segmented.</param>
    /// <param name="rects">Segment coordinates.</param>
    /// <returns>List of all segment pictures, or null if an error occured.</returns>
    public static List<byte[]>? GetSegmentPictures(byte[] imageBytes, List<Rectangle> rects)
    {
        try
        {
            var img = new Mat();
            CvInvoke.Imdecode(imageBytes, ImreadModes.Unchanged, img);

            var segments = new List<byte[]>();

            foreach (var rect in rects)
            {
                var cropped = new Mat(img, rect);
                var buf = new VectorOfByte();

                CvInvoke.Imencode(".png", cropped, buf);

                segments.Add(buf.ToArray());
            }

            return segments;
        }
        catch
        {
            return null;
        }
    }
}