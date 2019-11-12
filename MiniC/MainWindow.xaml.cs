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
using Newtonsoft.Json;

namespace MiniC
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Token> tokens;
        SyntaxTree syntaxTree;
        public MainWindow()
        {
            InitializeComponent();
            //SyntaxNode a = new Identifier("haha");
            //SetText(display, JsonConvert.SerializeObject(a, Formatting.Indented));
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
        private void Tokenize(object sender, RoutedEventArgs e)
        {
            Tokenize();
        }
        private void Parse(object sender, RoutedEventArgs e)
        {
            Parse();
        }
        private void Tokenize()
        {
            Lexer l = new Lexer(GetText(input));
            tokens = l.Tokenize();
            string v = string.Join("\r\n", l.Tokenize());
            if (display != null)
            {
                SetText(display, v);
            }
        }
        private void Parse()
        {
            if(tokens == null)
            {
                Tokenize();
            }
            Parser p = new Parser(tokens);
            try
            {
                syntaxTree = p.Parse();
                SetText(display, JsonConvert.SerializeObject(syntaxTree.root,Formatting.Indented));
            }catch(ParseException e)
            {
                SetText(display, e.Message);
            }
        }
    }
}
