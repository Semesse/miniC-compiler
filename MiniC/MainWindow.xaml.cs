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
        Lexer lexer;
        List<Token> tokens;
        SyntaxTree syntaxTree;
        static SolidColorBrush KeywordBrush = new SolidColorBrush(Color.FromRgb(0x2a, 0xa1, 0x98));
        static SolidColorBrush CommentBrush = new SolidColorBrush(Color.FromRgb(0x85, 0x99, 0x00));
        static SolidColorBrush LiteralBrush = new SolidColorBrush(Color.FromRgb(0xcb, 0x4b, 0x16));
        public MainWindow()
        {
            InitializeComponent();
            lexer = new Lexer();
            //SyntaxNode a = new Identifier("haha");
            //SetText(display, JsonConvert.SerializeObject(a, Formatting.Indented));
        }

        private string GetText(RichTextBox richText)
        {
            return new TextRange(richText.Document.ContentStart, richText.Document.ContentEnd).Text;
        }
        private static void GetPointer(TextPointer start, int startOffset, int endOffset, out TextPointer startPointer, out TextPointer endPointer)
        {
            int i = 0;
            startPointer = null;
            endPointer = null;
            while (i < startOffset && start != null)
            {
                if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textrun = start.GetTextInRun(LogicalDirection.Forward);
                    //start = start.GetNextContextPosition(LogicalDirection.Forward);
                    //i += start.GetTextInRun(LogicalDirection.Forward).Length;
                    start = start.GetNextInsertionPosition(LogicalDirection.Forward);
                    i++;
                }
                else if (start.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    break;
                else
                {
                    start = start.GetNextInsertionPosition(LogicalDirection.Forward);
                    i += 2;
                }
            }
            startPointer = start;
            while (i < endOffset)
            {
                if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    i++;
                }
                else if (start.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    break;
                start = start.GetNextInsertionPosition(LogicalDirection.Forward);
            }
            endPointer = start;
        }
        private void SetText(RichTextBox richText, string text)
        {
            richText.Document.Blocks.Clear();
            richText.Document.Blocks.Add(new Paragraph(new Run(text)));
        }
        private void SetColor(in TextPointer pointer, int start, int length, Brush brush)
        {
            TextPointer startPointer, endPointer;
            GetPointer(pointer, start, start + length, out startPointer, out endPointer);
            TextRange range = new TextRange(startPointer, endPointer);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            new TextRange(endPointer, endPointer.GetPositionAtOffset(1, LogicalDirection.Forward)).ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }
        private void SetColor(TextRange range, Brush brush)
        {
            range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            //range.End.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
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
            Tokenize();
            Parser p = new Parser(tokens);
            try
            {
                syntaxTree = p.Parse();
                SetText(display, JsonConvert.SerializeObject(syntaxTree.root, Formatting.Indented));
            }
            catch (ParseException e)
            {
                SetText(display, e.Message);
            }
        }

        private void Highlight(object sender, TextChangedEventArgs e)
        {
        }

        private void Highlight(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {

                lexer.SetSource(GetText(input));
                tokens = lexer.Tokenize();
                TextPointer pointer = input.Document.ContentStart, start, end;
                int lastEnd = 0;
                foreach (Token token in tokens)
                {
                    if (token.Type == TokenType.Keyword)
                    {
                        int tmp = token.Location - lastEnd;
                        GetPointer(pointer,
                            tmp, tmp + token.Value.Length,
                            out start, out end);
                        //input.Selection.Select(start, end);
                        //input.SelectionBrush = keywordBrush;
                        SetColor(new TextRange(start, end), KeywordBrush);
                        pointer = end;
                        lastEnd = token.Location + token.Value.Length - 1;
                        //input.Selection.Select(input.Document.ContentEnd, input.Document.ContentEnd);
                        //input.SelectionBrush = Brushes.Black;
                    }
                    else if (token.Type == TokenType.Comment || token.Type == TokenType.Macro)
                    {
                        int tmp = token.Location - lastEnd;
                        GetPointer(pointer,
                            tmp, tmp + token.Value.Length,
                            out start, out end);
                        //input.Selection.Select(start, end);
                        //input.SelectionBrush = keywordBrush;
                        SetColor(new TextRange(start, end), CommentBrush);
                        pointer = end;
                        lastEnd = token.Location + token.Value.Length;
                        //input.Selection.Select(input.Document.ContentEnd, input.Document.ContentEnd);
                        //input.SelectionBrush = Brushes.Black;
                    }
                    else if (token.Form.In(TokenForm.StringLiteral, TokenForm.CharLiteral, TokenForm.BooleanLiteral))
                    {
                        int tmp = token.Location - lastEnd;
                        GetPointer(pointer,
                            tmp, tmp + token.Value.Length,
                            out start, out end);
                        //input.Selection.Select(start, end);
                        //input.SelectionBrush = keywordBrush;
                        SetColor(new TextRange(start, end), LiteralBrush);
                        pointer = end;
                        lastEnd = token.Location + token.Value.Length - 1;
                        //input.Selection.Select(input.Document.ContentEnd, input.Document.ContentEnd);
                        //input.SelectionBrush = Brushes.Black;
                    }
                }
            }
            else if (e.Key == Key.Tab)
            {
                input.AppendText("\t");
                e.Handled = true;
            }
        }
    }
}
