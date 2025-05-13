# conv-file-quality-assurance

**conv-file-quality-assurance** is a cross-platform application designed to verify the integrity and quality of files that have been converted from their original format to archival formats (e.g., PDF, TIFF, PNG). It performs a series of in-depth comparisons and checks—such as visual differences, metadata mismatches, and structural inconsistencies—to ensure the preservation process retains fidelity.

---
![Home Screen](./readmeImg/Program.png)

## Table of contents
- [Background & Motivation](#background--motivation)  
- [Key Features](#key-features)  
- [Comparison Methods](#comparison-methods)  
- [Screenshots & UI Overview](#screenshots--ui-overview)  
- [Supported File Formats](#supported-file-formats)  
- [Installation](#installation)  
  - [Windows](#windows)  
  - [Ubuntu 22.04](#ubuntu-2204)  
- [Usage Guide](#usage-guide)  
- [Known Issues & Limitations](#known-issues--limitations)  
- [Testing & Validation](#testing--validation)  
- [License](#license)  
- [Third-Party Licenses](#third-party-licenses)  
- [Contributing](#contributing)  

---

## 📖 Background & Motivation
---

## 🚀 Key features
- Pixel-by-pixel visual comparisons of images or document pages
- Detection of metadata differences (resolution, color space, bit depth, etc.)
- Table break detection in spreadsheets converted to PDFs
- Font, animation, and transparency detection
- PDF page count comparisons
- Extraction and verification of embedded images
- JSON report generation with severity-based error classification

---

## 🧪 Comparison Methods 
Each method performs a specific type of comparison between original and converted files:

- **File Size** – Compares raw file sizes.
- **Pixel-by-Pixel** – Calculates Euclidean distance between color values for each pixel.
- **Color Space** – Identifies missing or altered color spaces.
- **Fonts** – Flags missing or replaced fonts.
- **Number of Pages** – Detects discrepancies in page count.
- **Animations** – Flags the presence or absence of animations in presentation files.
- **Image Resolution** – Compares DPI and resolution metadata.
- **Visual Document Comparison** – Hybrid method: compares document layout + pixel comparisons.
- **Transparency Check** – Detects use of transparency layers.
- **Table Break Check** – Flags if tables/images are split across multiple pages in PDFs.
- **Metadata Comparison** – Compares physical dimensions, bit depth, resolution, etc.
- **Extracted Metadata Check** – Extracts images from documents and checks embedded metadata  
  _NOTE: This method may trigger frequently; severity could be reduced._

---

## 🖼️ Screenshots & UI Overview

### Home Screen  
![Home Screen](./readmeImg/Program.png)

### Settings Tab  
Customize which checks are performed.  
![Settings Tab](./readmeImg/Settings.png)

### Report Tab  
Visual interface for exploring the generated JSON reports.  
![Report Tab](./readmeImg/ReportTab.png)

### Test Analysis Tab  
Get a summary of test failures by severity.  
![Test Analysis](./readmeImg/TestAnalysis.png)

---

## 📂 Supported File Formats

![Supported File formats](./readmeImg/SupportedFileFormats.png)

---

## 🔎 Supported Comparison methods

![Supported Comparison methods](./readmeImg/SupportedComparisonMethods.png)

---

## 💻 Installation

### ✅ Windows

The application has been tested and is working on Windows 10 and 11.

#### Dependencies 
- [.NET 8+](https://dotnet.microsoft.com/en-us/download)
- [Siegfried](https://www.itforarchivists.com/) (Add to system PATH)

#### Installation Steps
```bash
git clone --recursive https://github.com/panmat7/conv-file-quality-assurance.git
cd conv-file-quality-assurance
dotnet build
```

---

### 🐧 Ubuntu 22.04


#### Dependencies 
- .NET 8+
- [Siegfried](https://www.itforarchivists.com/)
- [Emgu.CV](https://www.emgu.com/wiki/index.php/Download_And_Installation)
- [ExifTool](https://exiftool.org/install.html#Unix)

#### Installation Steps
1. **Install Siegfried**  
   Download and install from: https://www.itforarchivists.com/
2. **Install Emgu.CV**  
   Follow instructions here: https://www.emgu.com/wiki/index.php/Download_And_Installation  
    - If you can't run the Point by point or Visual document comparison, it is probably because of missing dependencies. Check the provided recource and follow the instructions (https://www.emgu.com/wiki/index.php/Download_And_Installation#System.DllNotFoundException)
3. **Install ExifTool**  
   Using `apt`:
   ```bash
   sudo apt-get install libimage-exiftool-perl
   ```
   Or follow the instructions at: https://exiftool.org/install.html#Unix

4. **Install .NET 8 SDK**  

5. **Clone and Build the Project** 
```sh
git clone --recursive https://github.com/panmat7/conv-file-quality-assurance.git
cd conv-file-quality-assurance
dotnet build
```


## Usage Instructions
### Usage NB!

* The console window will print the progress of the verification every 5 minutes, and a estimated time left.
* If you try to load and verify many thousands of files, it may take a while for the loading and verification to finish.
* Errors have three levels of severity, Low, Medium and High. Low should be treated as more of a warning.


## 📘 Usage Guide

1. **Choose original and converted folders.**  
   To extract metadata only, select a single directory and press **"Extract"**.

2. **Click "Load"** to pair original and converted files.  
   Optionally load from a **checkpoint JSON** to resume interrupted runs.
   - To do this, check **"Start from Checkpoint"** and select a saved checkpoint JSON report.

3. **Configure checks** using the **Settings** tab or **Quick Settings**.
   - You can also exclude certain file formats from verification in the Settings tab.

4. **Click "Start"** to begin the verification process.

5. Once complete, a JSON report will be saved in the `reports/` folder (created in the working directory).

6. **View the report**:
   - Go to the **"Report"** tab.
   - Click **"Load from JSON"** and select the report to view results.

7. **Test Analysis Tab**:
   - Navigate to the **"Test Analysis"** tab to view a general overview of test failures by severity.

> ⏳ **Note**: The console prints progress every 5 minutes with estimated time remaining.  
> ⚠️ **Severity Levels**: Errors are categorized as **Low**, **Medium**, or **High**.  
>    - **Low** should be considered warnings and not always critical.

---

## ⚠️ Known Issues & Limitations

- **Table break check** currently works only for PDFs with standard A4 or letter-sized pages.
- **Extracted metadata comparison** may over-report differences. We are evaluating lowering the severity of these alerts.
- Some Emgu.CV-based checks may not function if system dependencies are missing (especially on Linux).

---

## ✅ Testing & Validation

- Application manually tested on:
  - **Windows 10 & 11**
  - **Ubuntu 22.04**
- JSON output reports pass internal consistency checks.
- Planned: Schema validation for report files to ensure correctness.

---

## 📄 License

This project is licensed under the [GNU Affero General Public License v3.0](LICENSE).

---

## 📦 Third-Party Licenses

This project uses external libraries such as Emgu.CV, ExifTool, and Siegfried.  
See the [NOTICE](NOTICE) file for third-party license information.

---

## 🤝 Contributing

We are not accepting contributions for now. 
