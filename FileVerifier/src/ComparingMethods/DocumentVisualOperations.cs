using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace AvaloniaDraft.ComparingMethods;

public static class DocumentVisualOperations
{
    /// <summary>
    /// Segments a document image into points of interest and returns them as a list of rectangles. 
    /// </summary>
    /// <param name="imageFilePath">Path to the image.</param>
    /// <returns>List of rectangles representing each segment. Null if an error occured.</returns>
    public static List<Rectangle>? SegmentDocumentImage(string imageFilePath)
    {
        try
        {
            var img = CvInvoke.Imread(imageFilePath);

            if (img is null) return null;

            return GetRects(img);
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
    /// <returns>List of rectangles representing each segment. Null if an error occured.</returns>
    public static List<Rectangle>? SegmentDocumentImage(byte[] imageBytes)
    {
        try
        {
            var img = new Mat();

            CvInvoke.Imdecode(imageBytes, ImreadModes.Unchanged, img); //Reading the image

            return GetRects(img);
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
    /// <param name="presentation">Whether the document is a presentation, if yes will apply additional iteration for
    /// morphing resulting is better grouping for lager objects.</param>
    /// <returns>List of segments ad rectangles. Null if an error occured.</returns>
    private static List<Rectangle>? GetRects(Mat img, bool presentation = false)
    {
        try
        {
            //Converting image to grayscale
            var grayscale = new Mat();
            CvInvoke.CvtColor(img, grayscale, ColorConversion.Bgr2Gray);
            
            var darkBackground = HasDarkBackground(CvInvoke.Mean(grayscale).V0);

            var threshold = new Mat();
            //Finding best threshold using Otsu
            var thresholdValue = CvInvoke.Threshold(grayscale, new Mat(), 0, 255, ThresholdType.Otsu);
            
            //Thresholding
            CvInvoke.Threshold(grayscale, threshold, thresholdValue, 255,
                !darkBackground ? ThresholdType.Binary : ThresholdType.BinaryInv); //Using BinaryInv for light and Binary for dark backgrounds

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

            var finalRects = FilterRectangles(rects);
            
            //Sorting based on top left corner, left to right, top to bottom
            finalRects.Sort((a, b) => a.Y == b.Y ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));
            
            return finalRects;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Filters out all rectangles fully embedded inside other rectangles.
    /// </summary>
    /// <param name="rects">Original list</param>
    /// <returns>The new filtered list.</returns>
    private static List<Rectangle> FilterRectangles(List<Rectangle> rects)
    {
        var filteredRects = new List<Rectangle>();

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

            if (!nested) filteredRects.Add(rects[i]);
        }
        
        return filteredRects;
    }
    
    /// <summary>
    /// Pairs together matching rectangles using Intersection over Union and returns their overlap.
    /// </summary>
    /// <param name="rectsO">List of rectangles from the original image</param>
    /// <param name="rectsN">List of rectangles from the new image.</param>
    /// <returns>The paired rectangles + IoU value, and lists of unpaired rectangles.</returns>
    public static (List<(Rectangle, Rectangle, double)>, List<Rectangle>, List<Rectangle>) PairAndGetOverlapSegments(List<Rectangle> rectsO, List<Rectangle> rectsN)
    {
        var paired = new List<(Rectangle, Rectangle, double)>(); //Tuple of pairs
        var unpairedO = new List<Rectangle>(); //Unpaired from both lists
        var unpairedN = new List<Rectangle>(rectsN);
        var used = new HashSet<int>(); //Indexes of already paired rects

        foreach (var rO in rectsO)
        {
            var i = -1;
            var bestRes = 0.0;

            for (var j = 0; j < rectsN.Count; j++)
            {
                if(used.Contains(j)) continue;
                
                var res = CalculateIoU(rO, rectsN[j]);

                if (res > bestRes)
                {
                    bestRes = res;
                    i = j;
                }
            }

            if (i != -1 && bestRes > 0.2) //Threshold filtering out false parings 
            {
                paired.Add((rO, rectsN[i], bestRes));
                used.Add(i);
                unpairedN.Remove(rectsN[i]);
            }
            else
            {
                unpairedO.Add(rO);
            }
        }
        
        return (paired, unpairedO, unpairedN);
    }
    
    /// <summary>
    /// Calculates Intersection over Union between two rectangles.
    /// </summary>
    /// <returns></returns>
    private static double CalculateIoU(Rectangle rect1, Rectangle rect2)
    {
        var x1 = Math.Max(rect1.Left, rect2.Left);
        var x2 = Math.Min(rect1.Right, rect2.Right);
        var y1 = Math.Max(rect1.Top, rect2.Top);
        var y2 = Math.Min(rect1.Bottom, rect2.Bottom);
        
        var intersectionArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1); //Calculating the intersecting part
        var unionArea = (rect1.Width * rect1.Height) + (rect2.Width * rect2.Height) - intersectionArea; //Area of both rects
        
        return unionArea == 0 ? 0 : (double)intersectionArea / unionArea;
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

    /// <summary>
    /// Determines whether the image should be considered having a dark background. 
    /// </summary>
    /// <param name="pixelIntensity">Average pixel intensity of the grayscale image.</param>
    /// <returns>True/False</returns>
    private static bool HasDarkBackground(double pixelIntensity)
    {
        return pixelIntensity > 130;
    }
}