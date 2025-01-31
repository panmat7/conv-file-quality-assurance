using System;
using System.IO;
using System.Linq;
using Aspose.Slides;
using Aspose.Words;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using SixLabors.ImageSharp;

namespace AvaloniaDraft.ComparingMethods;

public static class ComperingMethods
{
    /// <summary>
    /// Returns the difference size between two files 
    /// </summary>
    /// <param name="files">The two files to be compared</param>
    /// <returns>The size difference in bytes</returns>
    public static long GetFileSizeDifference(FilePair files)
    {
        var originalSize = new FileInfo(files.OriginalFilePath).Length;
        var newSize = new FileInfo(files.NewFilePath).Length;
        
        return originalSize - newSize;
    }
    
    /// <summary>
    /// Returns the difference of resolution between two images
    /// </summary>
    /// <param name="files">The two image files to be compared</param>
    /// <returns> </returns>
    public static Tuple<int, int>? GetImageResolutionDifference(FilePair files)
    {
        try
        {
            using Image image1 = Image.Load(files.OriginalFilePath), image2 = Image.Load(files.NewFilePath);
            var difWidth = image1.Width - image2.Width;
            var difHeight = image1.Height - image2.Height;
            return new Tuple<int, int>(int.Abs(difWidth), int.Abs(difHeight));
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Returns the resolution of an image
    /// </summary>
    /// <param name="path">Absolute path to the image</param>
    /// <returns>Tuple containing the image's width and height</returns>
    public static Tuple<int, int>? GetImageResolution(string path)
    {
        try
        {
            using var image = Image.Load(path);
            return new Tuple<int, int>(image.Width, image.Height);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the difference in pages between two documents. 
    /// </summary>
    /// <param name="files">Files to be compared</param>
    /// <returns>Either a positive integer with the page difference, -1 meaning error while getting pages or null meaning not supported file type</returns>
    public static int? GetPageCountDifference(FilePair files)
    {
        var originalPageCount = 0;
        var newPageCount = 0;

        try
        {
            var originalPages = GetPageCount(files.OriginalFilePath, files.OriginalFileFormat);
            var newPages = GetPageCount(files.NewFilePath, files.NewFileFormat);
            
            if(originalPages == null || newPages == null) return null;
            if(originalPages == -1 || newPages == -1) return -1;
            
            return int.Abs((int)(originalPages - newPages));
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Returns the number of pages in a document
    /// </summary>
    /// <param name="path">Absolute path to the document</param>
    /// <param name="format">PRONOM code of the file type</param>
    /// <returns>Either a positive integer with page count, -1 meaning error while getting pages or null meaning not supported file type</returns>
    public static int? GetPageCount(string path, string format)
    {
        try
        {
            //Text documents
            if (FormatCodes.TextDocumentFormats.Contains(format))
            {
                var doc = new Document(path);
                return doc.PageCount;
            }

            //For powerpoint - return number of slides
            if (FormatCodes.PresentationFormats.Contains(format))
            {
                var presentation = new Presentation(path);
                return presentation.Slides.Count;
            }
        }
        catch
        {
            return null;
        }

        return -1;
    }
}