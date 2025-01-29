// See https://aka.ms/new-console-template for more information
/*
using System.Diagnostics;
using System.Drawing;
using Microsoft.Office.Interop.Word;
using UglyToad.PdfPig;
using Aspose.Words;
using Document = Aspose.Words.Document;
using Shape = Aspose.Words.Drawing.Shape;

class FileDataFormat
{
    static void Main(string[] args)
    {
        Console.WriteLine("This program will write out information about different files depending on their format.");
        Console.WriteLine("Please enter folder path:");
        string folderPath = Console.ReadLine();

        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath, "");
            foreach (string file in files)
            {
                Console.WriteLine("FILE: " + file);
                Console.WriteLine("File size: " + new FileInfo(file).Length / 1024 + "KB");
                Console.Write("The file is an ");
                switch (Path.GetExtension(file))
                {
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                    case ".bmp":
                        Console.WriteLine("Image");
                        using (Image img = Image.FromFile(file))
                        {
                            Console.WriteLine("Format: " + img.RawFormat);
                            Console.WriteLine("Resolution: " + img.Width + " x " + img.Height);
                            Console.WriteLine("DPI: " + img.HorizontalResolution + " x " + img.VerticalResolution);
                            Console.WriteLine("Pixel Format: " + img.PixelFormat);
                        }
                        break;
                    case ".pdf":
                        Console.WriteLine("PDF");
                        using (PdfDocument doc = PdfDocument.Open(file))
                        {
                            Console.WriteLine("Pages Count: " + doc.NumberOfPages);
                            Console.WriteLine("Document Information: " + doc.Information);
                            Console.WriteLine("Document Pages:");
                            var pageCounter = 0;
                            var imgCounter = 0;
                            foreach (var page in doc.GetPages())
                            {
                                pageCounter++;
                                Console.WriteLine("Page #" + pageCounter);
                                Console.WriteLine("\tPage Dimensions: " + page.Height + "x" + page.Width);
                                Console.WriteLine("\tPage Images:");
                                var dirPath = folderPath + "\\Images";
                                Directory.CreateDirectory(dirPath);
                                foreach (var pdfImg in page.GetImages())
                                {
                                    imgCounter++;
                                    Console.WriteLine("\t\tImage " + imgCounter);
                                    Console.WriteLine("\t\tImage Position: " + pdfImg.Bounds);
                                    Console.WriteLine("\t\tSample Resolution: " + pdfImg.WidthInSamples + "x" + pdfImg.HeightInSamples);
                                    Console.WriteLine("\t\tImage Dictionary" + pdfImg.ImageDictionary);
                                    Console.WriteLine("\t\tBits per Component: " + pdfImg.BitsPerComponent);
                                    Console.WriteLine("\t\tSAVING IMAGE");

                                    var imgData = pdfImg.RawBytes;

                                    //JPG signature
                                    if (imgData.Length > 2 && imgData[0] == 0xFF && imgData[1] == 0xD8)
                                    {
                                        using FileStream fs = new FileStream(dirPath + "\\img" + imgCounter + ".jpg",
                                            FileMode.Create, FileAccess.Write);
                                        using(BinaryWriter bw = new BinaryWriter(fs))
                                        {
                                            bw.Write(pdfImg.RawBytes.ToArray());
                                            bw.Flush();
                                        }
                                    } else if (imgData.Length >= 8 &&
                                               imgData[0] == 0x89 &&
                                               imgData[1] == 0x50 &&
                                               imgData[2] == 0x4E &&
                                               imgData[3] == 0x47 &&
                                               imgData[4] == 0x0D &&
                                               imgData[5] == 0x0A &&
                                               imgData[6] == 0x1A &&
                                               imgData[7] == 0x0A)
                                    {
                                        using FileStream fs = new FileStream(dirPath + "\\img" + imgCounter + ".png",
                                            FileMode.Create, FileAccess.Write);
                                        using(BinaryWriter bw = new BinaryWriter(fs))
                                        {
                                            byte[] bytes = [];
                                            if (pdfImg.TryGetPng(out bytes))
                                            {
                                                bw.Write(bytes);
                                                bw.Flush();
                                            }
                                        }
                                    }
                                    
                                    // using(var memoryStream = new MemoryStream(pdfImg.RawBytes.ToArray()))
                                    // {
                                    //     using (var bitmap = new Bitmap(memoryStream))
                                    //     {
                                    //         bitmap.Save(dirPath + "\\img" + imgCounter + ".jpg", ImageFormat.Jpeg);
                                    //     }
                                    // }
                                }
                                if (imgCounter == 0)
                                {
                                    Console.WriteLine("\t\tNo images found");
                                }
                            }
                        }
                        break;
                    case ".doc":
                    case ".docx":
                    case ".odt":
                        Console.WriteLine("Word/OpenOffice Document");
                        try
                        {
                            var doc = new Document(file);
                            Console.WriteLine("Page Count: " + doc.PageCount);
                            Console.WriteLine("Page Information: ");
                            NodeCollection nodes = doc.GetChildNodes(NodeType.Shape, true);
                            var dirPath = "";
                            if (Path.GetExtension(file) == ".docx" || Path.GetExtension(file) == ".doc")
                            {
                                dirPath = folderPath + "\\ImagesDoc";
                            }
                            else
                            {
                                dirPath = folderPath + "\\ImagesOdt";
                            }
                            var imageI = 0;
                            foreach (var node in nodes)
                            {
                                var shape = (Shape)node;
                                if (!shape.HasImage) continue;
                                imageI++;
                                Console.WriteLine("Image " + imageI);
                                shape.ImageData.Save(dirPath + "\\img" + imageI + "." + shape.ImageData.ImageType);
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Word Document Not Found");
                        }

                        break;
                    default:
                        Console.WriteLine("Not supported file type");
                        break;
                }
                
                
                    
                Console.WriteLine("");
            }
        }
        else
        {
            Console.WriteLine("Folder does not exist or invalid.");
        }
    }
}
*/