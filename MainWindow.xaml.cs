﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.FormatProviders;
using Telerik.Windows.Documents.FormatProviders.Pdf;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.RichTextBoxCommands;

namespace MdFormatProviderDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string path;
        private IDocumentFormatProvider provider;
        public MainWindow()
        {
            RadRichTextBox.DefaultTextRenderingMode = Telerik.Windows.Documents.UI.TextBlocks.TextBlockRenderingMode.TextBlockWithPropertyCaching;

            InitializeComponent();

            var provider = new MdFormatProvider();
            this.radRichTextBox.RegisteredApplicationCommands.Remove(ApplicationCommands.Save); // I have used the SaveSaveAsExample from https://github.com/telerik/xaml-sdk/tree/master/RichTextBox/SaveAndSaveAs

            this.radRichTextBox.CommandExecuted += radRichTextBox_CommandExecuted;

        }

        private void btnImport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.radRichTextBox.Commands.OpenDocumentCommand.Execute();
        }

        void radRichTextBox_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            if (e.Command is NewDocumentCommand)
            {
                // When a new document is opened the values of the path and the provider should be cleared.
                this.path = null;
                this.provider = null;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            this.SaveAs();
        }

        private void SaveAs()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            var formatProviders = DocumentFormatProvidersManager.FormatProviders;

            string filter = string.Join("|", formatProviders.Where(fp => fp.CanExport)
                                                            .OrderBy(fp => fp.Name)
                                                            .Select(fp => this.GetFilter(fp))
                                                            .ToArray());
            saveDialog.Filter = filter;

            bool? dialogResult = saveDialog.ShowDialog();
            if (dialogResult == true)
            {
                string extension = System.IO.Path.GetExtension(saveDialog.SafeFileName);
                this.GetProvider(extension);

                Stream outputStream = saveDialog.OpenFile();
                this.path = saveDialog.FileName;

                this.Export(outputStream);
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string filter = string.Join("|", DocumentFormatProvidersManager.FormatProviders
                                                                           .Where(fp => fp.CanImport)
                                                                           .OrderBy(fp => fp.Name)
                                                                           .Select(fp => this.GetFilter(fp))
                                                                           .ToArray()) + "|All Files|*.*";
            ofd.Filter = filter;

            if (ofd.ShowDialog() == true)
            {
                string extension = Path.GetExtension(ofd.SafeFileName).ToLower();

                this.provider = DocumentFormatProvidersManager.GetProviderByExtension(extension);

                if (this.provider == null)
                {
                    MessageBox.Show(LocalizationManager.GetString("Documents_OpenDocumentCommand_UnsupportedFileFormat"));
                    return;
                }

                try
                {
                    Stream stream;
                    stream = ofd.OpenFile();
                    using (stream)
                    {
                        RadDocument document = provider.Import(stream);
                        this.radRichTextBox.Document = document;

                        // Preserve the file name to use it when saving
                        this.path = ofd.FileName;
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show(LocalizationManager.GetString("Documents_OpenDocumentCommand_TheFileIsLocked"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LocalizationManager.GetString("Documents_OpenDocumentCommand_TheFileCannotBeOpened"));
                }
            }
        }

        private void GetProvider(string extension)
        {
            this.provider = DocumentFormatProvidersManager.GetProviderByExtension(extension);

            if (this.provider == null)
            {
                MessageBox.Show(LocalizationManager.GetString("Documents_SaveCommand_UnsupportedFileFormat"));
            }

            if (this.provider is IConfigurablePdfFormatProvider)
            {
                IConfigurablePdfFormatProvider pdfFormatProvider = (IConfigurablePdfFormatProvider)this.provider;
                pdfFormatProvider.ExportSettings.CommentsExportMode =
                    this.radRichTextBox.ShowComments ? PdfCommentsExportMode.NativePdfAnnotations : PdfCommentsExportMode.None;
            }
        }

        private void Export(Stream outputStream)
        {
            try
            {
                using (outputStream)
                {
                    this.provider.Export(this.radRichTextBox.Document, outputStream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizationManager.GetString("Documents_SaveCommand_UnableToSaveFile"));
            }
        }

        private string GetFilter(IDocumentFormatProvider formatProvider)
        {
            return
                formatProvider.FilesDescription +
                " (" +
                string.Join(", ", formatProvider.SupportedExtensions.Select(ext => "*" + ext).ToArray()) +
                ")|" +
                string.Join(";", formatProvider.SupportedExtensions.Select(ext => "*" + ext).ToArray());
        }
    }
}
