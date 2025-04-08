# conv-file-quality-assurance
This application performs quality checks on files mostly for files converted from a fileformat to an archival format. 
The checks consists of metadata, size, color space, pixelvalue, fonts, number of pages and animations. 

## Comparing methods
* File Size: Compares the file sizes of each file.

* Pixel by pixel: Checks every pixel in an image against another image. It calculates the eucludian distance between the colors of each pixel.

* Color Space: Checks for missing or different color spaces. 

* Fonts: Checks for missing or different fonts.

* Number of pages: Checks for missing or extra pages.

* Animations: Checks if presenting formats include animations.

* Image resolution: Checks for different resolkution in images

* Visual Document comparison: 

* Trancsparancy checks: Checks if images or text-document include transparancy. 

* Table break checks: Checks if spreadsheet formats converted to PDF has table breaks. 

* Metadata checks: Checks for missing or different metadata on images.

* Metadata cheks (extracted): Extract images from PDF, openoffice and microsoft documents and checks for missing or different metadata. 




## Supported file formats

![Supported File formats](https://github.com/panmat7/conv-file-quality-assurance/blob/pbp/SupportedFileFormats.png?raw=true)

## Supported comparison methods

![Supported Comparison methods](https://github.com/panmat7/conv-file-quality-assurance/blob/pbp/SupportedComparisonMethods.png?raw=true)



## Windows
### Dependencies Windows
- .NET 8+
- Siegfried 
- Other dependencies

### Installation Windows
1. Install Siegfried from their website (https://www.itforarchivists.com/) and add it to path enviromental variables. 
2. Clone the repository 
```sh
git clone https://github.com/panmat7/conv-file-quality-assurance.git
```
3. Build and run the application


## Ubuntu 22.04
### Dependencies Ubuntu
- .NET 8+
- Siegfried
- Emgu.cv - Emgu.cv 
- ExifTool

### Installation Ubuntu
1. Install Siegfried from their website (https://www.itforarchivists.com/)
2. Install Emgu.cv (https://www.emgu.com/wiki/index.php/Download_And_Installation)
 - If you can't run the Point by point comparison, check for missing dependencies (https://www.emgu.com/wiki/index.php/Download_And_Installation#System.DllNotFoundException)
3. Install ExifTool (https://exiftool.org/install.html#Unix)

4. Clone the repository 
```sh
git clone https://github.com/panmat7/conv-file-quality-assurance.git
```
5. Build and run the application
```sh
cd conv-file-quality-assurance
dotnet build
```



## Usage



## Comparison criteria
This is a preliminary draft of what can be used to compare a converted file with an original in another format:

- Size: If the size of the document (image) is stated - do the sizes of the original and copy match?
- Image resolution: is it possible to identify image resolution in the two formats being compared and is the resolution the same? (dots per inch)
- Color space: If possible, we want to identify the color profile in the original against the color profile in the copy. Do these match?
- Fonts Are fonts built into the copy? And does it match the original's use of fonts?
- Point by point - comparison: If we look at the original and converted file as two point matrices - Do the pixels in the two files match in terms of color? (requires size and resolution to match)
- Number of pages: Same number of pages in original and copy.
- Animations: Identify if the original uses animations. This will often disappear upon conversion.
