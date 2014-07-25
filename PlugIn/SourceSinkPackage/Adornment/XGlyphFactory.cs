using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using EnvDTE;

internal class XGlyphFactory : IGlyphFactory
{
    const double m_glyphSize = 12.0;


    public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
    {
        // Ensure we can draw a glyph for this marker. 
        if (tag == null || !(tag is XTag))
        {
            return null;
        }

        Brush color;
        string filename = GetFileName(line);

        if (SourceSinkEP.sourceEP != null && line.Start.GetContainingLine().LineNumber == SourceSinkEP.sourceEP.Line - 1 && filename.ToLower() == SourceSinkEP.sourceEP.Parent.Parent.FullName.ToLower())
        {
            color = Brushes.Red;
        }
        else if (SourceSinkEP.sinkEP != null && line.Start.GetContainingLine().LineNumber == SourceSinkEP.sinkEP.Line - 1 && filename.ToLower() == SourceSinkEP.sinkEP.Parent.Parent.FullName.ToLower())
        {
            color = Brushes.DarkSeaGreen;
        }
        else if (SourceSinkEP.functionEP != null && line.Start.GetContainingLine().LineNumber == SourceSinkEP.functionEP.Line - 1 && filename.ToLower() == SourceSinkEP.functionEP.Parent.Parent.FullName.ToLower())
        {
            color = Brushes.BlueViolet;
        }
        else
        {
            return null;
        }

        System.Windows.Shapes.Ellipse ellipse = new Ellipse();
        ellipse.Fill = color;
        ellipse.Height = m_glyphSize;
        ellipse.Width = m_glyphSize;

        return ellipse;
    }


    public static string GetFileName(IWpfTextViewLine line)
    {
        ITextBuffer TextBuffer = line.Snapshot.TextBuffer;
        ITextDocument textDoc;
        var rc = TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDoc);
        if (rc == true)
            return textDoc.FilePath;
        else
            return "";
    }

}


[Export(typeof(IGlyphFactoryProvider))]
[Name("TodoGlyph")]
[Order(After = "VsTextMarker")]
[ContentType("code")]
[TagType(typeof(XTag))]
internal sealed class XGlyphFactoryProvider : IGlyphFactoryProvider
{
    public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
    {
        return new XGlyphFactory();
    }
}



