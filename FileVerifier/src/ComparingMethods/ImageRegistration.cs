using System;
using System.Diagnostics.CodeAnalysis;
using AvaloniaDraft.FileManager;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace AvaloniaDraft.ComparingMethods;

[ExcludeFromCodeCoverage]
public static class ImageRegistration
{
    /// <summary>
    /// This was for testing purposes
    /// Calculates the similarity between two images by comparing their histograms for each color channel (Blue, Green, Red).
    /// It computes the histograms of the two images and compares them using the correlation method.
    /// </summary>
    /// <param name="pair">A pair of file paths containing the original and new images.</param>
    /// <returns>A similarity percentage between 0 and 100, where 100 means the images are identical, and 0 means they are completely different.</returns>
    public static double CalculateHistogramSimilarity(FilePair pair)
    {
        // Load images
        using Mat img1 = CvInvoke.Imread(pair.OriginalFilePath);
        using Mat img2 = CvInvoke.Imread(pair.NewFilePath);
        Console.WriteLine("Loaded images");
        // Ensure both images are the same size
        if (img1.Size != img2.Size)
        {
            CvInvoke.Resize(img2, img2, img1.Size);
        }
        
        Console.WriteLine("Calculating similarity");
        VectorOfMat channels1 = new VectorOfMat(3); 
        VectorOfMat channels2 = new VectorOfMat(3); 
        CvInvoke.Split(img1, channels1); 
        CvInvoke.Split(img2, channels2); 
        Console.WriteLine("Creating histogram");
        // Create histogram variables
        using Mat hist1 = new Mat();
        using Mat hist2 = new Mat();

        int histSize = 256; 
        float[] range = { 0, 256 };
        
        double finalScore = 0;
        int numChannels = 3;
        
        
        Console.WriteLine("Calclulating for every channel");
        for (int i = 0; i < numChannels; i++)
        {
            // Calculate histogram for each channel (RGB)
            CvInvoke.CalcHist(new VectorOfMat(channels1[i]), new [] { 0 }, null, hist1, new [] { histSize }, range, false);
            CvInvoke.CalcHist(new VectorOfMat(channels2[i]), new [] { 0 }, null, hist2, new [] { histSize }, range, false);
            
            // Normalize histograms
            CvInvoke.Normalize(hist1, hist1, 0, 1, NormType.MinMax);
            CvInvoke.Normalize(hist2, hist2, 0, 1, NormType.MinMax);

            // Compare histograms for each channel using Correlation method
            double score = CvInvoke.CompareHist(hist1, hist2, HistogramCompMethod.Correl);
            finalScore += score; 
        }
        Console.WriteLine("Calculating score");
        // Average the scores from each channel
        finalScore = (finalScore / numChannels) * 100;
        Console.WriteLine($"Score: {finalScore}");
        return Math.Max(0, Math.Min(100, finalScore)); // Clamp to 0-100%
    }
}






