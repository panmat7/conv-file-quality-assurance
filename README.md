# conv-file-quality-assurance
This application performs quality checks on files mostly for files converted from a fileformat to an archival format. 
The checks consists of metadata, size, color space, pixelvalue, fonts, number of pages and animations. 

![Home Screen](https://github.com/panmat7/conv-file-quality-assurance/blob/main/Program.png?raw=true)

## Comparing methods
* File Size: Compares the file sizes of each file.

* Pixel by pixel: Checks every pixel in an image against another image. It calculates the eucludian distance between the colors of each pixel.

* Color Space: Checks for missing or different color spaces. 

* Fonts: Checks for missing or different fonts.

* Number of pages: Checks for missing or extra pages.

* Animations: Checks if presenting formats include animations.

* Image resolution: Checks for different resolution in images.

* Visual Document comparison: 

* Trancsparancy checks: Checks if images or text-document include transparancy. 

* Table break checks: Checks if spreadsheet formats converted to PDF has table breaks. 

* Metadata checks: Checks for missing or different metadata on images.

* Metadata cheks (extracted): Extract images from PDF, openoffice and microsoft documents and checks for missing or different metadata. 




## Supported file formats

![Supported File formats](https://github.com/panmat7/conv-file-quality-assurance/blob/main/SupportedFileFormats.png?raw=true)

## Supported comparison methods

![Supported Comparison methods](https://github.com/panmat7/conv-file-quality-assurance/blob/main/SupportedComparisonMethods.png?raw=true)



## Windows
### Dependencies Windows
* .NET 8+
* Siegfried 

### Installation Windows
1. Install .Net 8 from Microsoft's website (https://dotnet.microsoft.com/en-us/download)
1. Install Siegfried from their website (https://www.itforarchivists.com/) and add it to path enviromental variables. 
2. Clone the repository 
```sh
git clone https://github.com/panmat7/conv-file-quality-assurance.git
```
3. Build and run the application


## Ubuntu 22.04
### Dependencies Ubuntu
* .NET 8+
* Siegfried
* Emgu.cv - Emgu.cv 
* ExifTool

### Installation Ubuntu
1. Install Siegfried from their website (https://www.itforarchivists.com/)
2. Install Emgu.cv (https://www.emgu.com/wiki/index.php/Download_And_Installation)
    - If you can't run the Point by point or Visual document comparison, check for missing dependencies (https://www.emgu.com/wiki/index.php/Download_And_Installation#System.DllNotFoundException)
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
### Usage NB!

* The console window will print the progress of the verification every 5 minutes, and a estimated time left.
* If you try to load and verify many thousands of files, it may take a while for the loading and verification to finnish.


### Usage guide

1. Choose the folder for original files, and for the new files.
2. Click the "Load" button to create the file pairs.
3. Use the Quick settings or the "Settings" tab to choose what comparison methods you want to run. 
    - In the settings tab you can ignore File formats to verify.


The Settings tab:
![Home Screen](https://github.com/panmat7/conv-file-quality-assurance/blob/main/Settings.png?raw=true)


4. Click the "Start" button to start the verfication process. 

5. After the process is done, a JSON report will be genereated in the reports folder:
```sh
conv-file-quality-assurance\FileVerifier\reports
```
6. To view the report in the application go to the "Report" tab click the "Load from JSON" button and choose the report.

The Report tab with an JSON report imported:
![Home Screen](https://github.com/panmat7/conv-file-quality-assurance/blob/main/ReportTab.png?raw=true)

