using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MiniC.Compiler;

namespace MiniC
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string GetText(RichTextBox richText)
        {
            return new TextRange(richText.Document.ContentStart, richText.Document.ContentEnd).Text;
        }

        private void SetText(RichTextBox richText, string text)
        {
            richText.Document.Blocks.Clear();
            richText.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        private void Parse(object sender, RoutedEventArgs e)
        {
            Lexer l = new Lexer(GetText(input));
            string v = string.Join("\r\n", l.Tokenize());
            if (display != null)
            {
                SetText(display, v);
            }
        }
    }
}
