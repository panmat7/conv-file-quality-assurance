# conv-file-quality-assurance
This application performs quality checks on files mostly for files converted from a fileformat to an archival format. 

## Features

## Supported file formats





## Installation Windows
1. Install Siegfried from their website (https://www.itforarchivists.com/) and add it to path enviromental variables. 
2. Clone the repository 
```sh
git clone https://github.com/panmat7/conv-file-quality-assurance.git
```
3. Run the application
### Dependencies Windows
- .NET 8+
- Siegfried 
- Other dependencies



## Installation Ubuntu 22.04
1. Install Siegfried from their website (https://www.itforarchivists.com/)
2. Install Emgu.cv (https://www.emgu.com/wiki/index.php/Download_And_Installation)
3. Install ExifTool (https://exiftool.org/install.html#Unix)

### Dependencies Ubuntu
- .NET 8+
- Siegfried
- Emgu.cv - Emgu.cv 
- ExifTool

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
