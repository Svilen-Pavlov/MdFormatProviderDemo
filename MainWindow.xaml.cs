using System.IO;
using System.Windows;
using Telerik.Windows.Controls;

namespace MdFormatProviderDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            RadRichTextBox.DefaultTextRenderingMode = Telerik.Windows.Documents.UI.TextBlocks.TextBlockRenderingMode.TextBlockWithPropertyCaching;

            InitializeComponent();

            var provider = new MdFormatProvider();
            //this.radRichTextBox.Document = provider.Import(new FileStream("../../SampleData/Test.md", FileMode.Open));
            
            
        }

        private void btnImport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.radRichTextBox.Commands.OpenDocumentCommand.Execute();
        }
    }
}
