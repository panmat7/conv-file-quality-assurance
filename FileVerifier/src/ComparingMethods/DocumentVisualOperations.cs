using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using AvaloniaDraft.Helpers;
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
            
            Console.WriteLine(CvInvoke.Mean(grayscale).V0);
            var lightBackground = HasLightBackground(CvInvoke.Mean(grayscale).V0);

            var threshold = new Mat();
            //Finding best threshold using Otsu
            var thresholdValue = CvInvoke.Threshold(grayscale, new Mat(), 0, 255, ThresholdType.Otsu);
            
            //Thresholding
            CvInvoke.Threshold(grayscale, threshold, thresholdValue, 255,
                lightBackground ? ThresholdType.BinaryInv : ThresholdType.Binary); //Using BinaryInv for light and Binary for dark backgrounds

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
    /// Separates out the segments based on the given rectangles and returns them as bytes.
    /// </summary>
    /// <param name="imageBytes">Image to be segmented.</param>
    /// <param name="rect">Segment coordinates.</param>
    /// <returns>List of all segment pictures, or null if an error occured.</returns>
    public static byte[]? GetSegmentPictures(byte[] imageBytes, Rectangle rect)
    {
        try
        {
            var img = new Mat();
            CvInvoke.Imdecode(imageBytes, ImreadModes.Unchanged, img);
            
            var cropped = new Mat(img, rect);
            var buf = new VectorOfByte();

            CvInvoke.Imencode(".png", cropped, buf);

            return buf.ToArray();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines whether the image should be considered having a light background. 
    /// </summary>
    /// <param name="pixelIntensity">Average pixel intensity of the grayscale image.</param>
    /// <returns>True/False</returns>
    public static bool HasLightBackground(double pixelIntensity)
    {
        return pixelIntensity > 130;
    }

    /// <summary>
    /// Returns indexes for page checks at a specified interval, up to the page count.
    /// </summary>
    /// <param name="pageCount">Number of pages in the document.</param>
    /// <param name="atATime">Number of page to be handled at a time.</param>
    /// <returns>List of tuples, containing the starting and ending index of the check interval.</returns>
    public static List<(int, int)> GetPageCheckIndexes(int pageCount, int atATime)
    {
        var result = new List<(int, int)>();
        var start = 0;
        while (start < pageCount)
        {
            var end = Math.Min(start + (atATime - 1), pageCount - 1);
            result.Add((start, end));
            start += atATime;
        }
        
        return result;
    }

    /// <summary>
    /// Determine whether a segment is relevant for pixel-to-pixel comparison based on the number of color clusters.
    /// </summary>
    /// <param name="seg">Segment to be considered.</param>
    /// <returns>True/False, null if an error occured.</returns>
    public static bool? DetermineSegmentRelevance(byte[] seg)
    {
        try
        {
            var img = new Mat();
            CvInvoke.Imdecode(seg, ImreadModes.Unchanged, img);

            //Converting to grayscale
            var brg = new Mat();
            switch (img.NumberOfChannels)
            {
                case 1: CvInvoke.CvtColor(img, brg, ColorConversion.Gray2Bgr); break; //Grayscale
                case 3: brg = img; break; //RGB
                case 4: CvInvoke.CvtColor(img, brg, ColorConversion.Bgra2Bgr); break; //RGBA
                default: return null;
            }

            var lab = new Mat();
            CvInvoke.CvtColor(brg, lab, ColorConversion.Bgr2Lab);

            //Flattening
            var reshaped = lab.Reshape(1, lab.Rows * lab.Cols);
            reshaped.ConvertTo(reshaped, DepthType.Cv32F); //float32

            var labels = new Mat();
            var centers = new Mat();
            const int k = 4;

            CvInvoke.Kmeans(
                data: reshaped,
                k: k,
                bestLabels: labels,
                termcrit: new MCvTermCriteria(5, 0.75),
                attempts: 2,
                flags: KMeansInitType.PPCenters,
                centers: centers
            );

            var counts = new int[k];
            var totalPixels = labels.Rows;

            unsafe
            {
                var labelData = (int*)labels.DataPointer.ToPointer();
                for (var i = 0; i < totalPixels; i++)
                {
                    counts[labelData[i]]++;
                }
            }

            Array.Sort(counts);
            Array.Reverse(counts);

            var dominanceRatio = (counts[0] + counts[1]) / (double)totalPixels;

            CvInvoke.Imshow("img", img);
            CvInvoke.WaitKey(0);
            CvInvoke.DestroyAllWindows();

            return dominanceRatio < 0.8;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates an error list from marked pages.
    /// </summary>
    /// <param name="errorPages">List of hashsets containing pages for each error.</param>
    /// <returns>List of errors.</returns>
    public static List<Error> WriteErrors(List<HashSet<int>> errorPages)
    {
        //Writing errors
        var errors = new List<Error>();
        if (errorPages[0].Count > 0)
            errors.Add(new Error(
                "Mismatch in detected points of interest",
                "The original or/and new document contain points of interest that could not have been paired. " +
                "This could be caused by added/removed noise in the resulting file, something being missing/added or " +
                "large differences in document structure.",
                ErrorSeverity.High,
                ErrorType.Visual,
                "Pages: " + string.Join(", ", errorPages[0])
            ));

        if (errorPages[1].Count > 1)
            errors.Add(new Error(
                "Misaligned points of interest",
                "Some segments of the document have been moved above the allowed value.",
                ErrorSeverity.High,
                ErrorType.Visual,
                "Pages: " + string.Join(", ", errorPages[1])
            ));
        
        if(errorPages[2].Count > 1)
            errors.Add(new Error(
                "Error getting pages",
                "There occured an error when trying to compare images of the segments visually, using Point by Point " +
                "comparison. This is an internal error, possibly caused some issue in the file.",
                ErrorSeverity.Medium,
                ErrorType.FileError,
                "Pages: " + string.Join(", ", errorPages[2])
            ));
        
        if(errorPages[3].Count > 1)
            errors.Add(new Error(
                "Visual Segment Comparison failed",
                "At least one segment failed the visual comparison.",
                ErrorSeverity.High,
                ErrorType.Visual,
                "Pages: " + string.Join(", ", errorPages[3])
            ));
        
        return errors;
    }
}