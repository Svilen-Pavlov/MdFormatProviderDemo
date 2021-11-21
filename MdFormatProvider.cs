using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GroupDocs.Conversion;
using GroupDocs.Conversion.Options.Convert;
using Telerik.Windows.Documents.FormatProviders;
using Telerik.Windows.Documents.FormatProviders.OpenXml.Docx;
using Telerik.Windows.Documents.Model;

namespace MdFormatProviderDemo
{
    //Used this library: https://github.com/aspose-pdf 
    //Install using: PM > Install-Package GroupDocs.Conversion -Version 21.10.0
    //I have used the DocFormatProviderDemo from https://github.com/telerik/xaml-sdk/tree/master/RichTextBox/DocFormatProviderDemo
    [CustomDocumentFormatProvider]
    public class MdFormatProvider : DocumentFormatProviderBase
    {
        private static readonly string name = "MdFormatProvider";
        private static readonly IEnumerable<string> supportedExtensions = new string[] { ".md" };
        private readonly DocxFormatProvider docxProvider;

        public MdFormatProvider()
        {
            this.docxProvider = new DocxFormatProvider();
        }

        public override string Name
        {
            get
            {
                return MdFormatProvider.name;
            }
        }

        public override string FilesDescription
        {
            get
            {
                return "Md Documents";
            }
        }

        public override IEnumerable<string> SupportedExtensions
        {
            get
            {
                return MdFormatProvider.supportedExtensions;
            }
        }

        public override bool CanImport
        {
            get { return true; }
        }

        public override bool CanExport
        {
            get { return true; }
        }

        public override RadDocument Import(Stream input)
        {
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempFilePath = Path.Combine(executableLocation, "temp.md");

            //This library does not seem to have a way to directly feed its Converter class a Stream, which is mentally challenged.
            //So we have to create a File first...
            using (var fileStream = File.Create(tempFilePath))
            {
                input.Seek(0, SeekOrigin.Begin);
                input.CopyTo(fileStream);
            }

            Converter converter = new Converter(tempFilePath); // This process is quite slow. 
            File.Delete(tempFilePath);

            var convertOptions = converter.GetPossibleConversions()["docx"].ConvertOptions;
            var convertedName = "converted.docx";

            converter.Convert(convertedName, convertOptions);
            string savePath = Path.Combine(executableLocation, convertedName);

            RadDocument docXdocument;
            using (var neededDocXStream = new FileStream(savePath, FileMode.Open))
            {
                docXdocument = this.docxProvider.Import(neededDocXStream);
            }
            File.Delete(savePath);

            return docXdocument;
        }

        public override void Export(RadDocument document, Stream output)
        {
            var inputByteArray = docxProvider.Export(document);

            var userSelectedSavePath = (output as FileStream).Name;
            output.Close();
            File.Delete(userSelectedSavePath);

            string[] pathParts = userSelectedSavePath.Split('\\').ToArray();
            string userSelectedFileName = pathParts[pathParts.Length - 1];

            string tempFileBaseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); 
            string tempFileAbsolutePath = Path.Combine(tempFileBaseDirectory, "temp.docx"); 
            File.WriteAllBytes(tempFileAbsolutePath, inputByteArray); 

            Converter converter = new Converter(tempFileAbsolutePath);
            File.Delete(tempFileAbsolutePath); 

            var convertOptions = converter.GetPossibleConversions()["md"].ConvertOptions;
            converter.Convert(userSelectedFileName, convertOptions);

            string convertedFileAbsolutePath = Path.Combine(tempFileBaseDirectory, userSelectedFileName);
            File.Move(convertedFileAbsolutePath, userSelectedSavePath); 


        }
    }
}
