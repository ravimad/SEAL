using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

internal class XTag : IGlyphTag
{

}

public class XTagger : ITagger<XTag>
{
    private IClassifier m_classifier;
    internal XTagger(IClassifier classifier)
    {
        m_classifier = classifier;
    }

    IEnumerable<ITagSpan<XTag>> ITagger<XTag>.GetTags(NormalizedSnapshotSpanCollection spans)
    {
        foreach (SnapshotSpan span in spans)
        {
            foreach (ClassificationSpan classification in m_classifier.GetClassificationSpans(span))
            {
                yield return new TagSpan<XTag>(new SnapshotSpan(classification.Span.Start, 1), new XTag());

            }
        }
    }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    public void RaiseTagsChanged(SnapshotSpan span)
    {
        var handler = this.TagsChanged;
        if (handler != null)
            handler(this, new SnapshotSpanEventArgs(span));
    }
}

[Export(typeof(ITaggerProvider))]
[ContentType("code")]
[TagType(typeof(XTag))]
class TodoTaggerProvider : ITaggerProvider
{
    [Import]
    internal IClassifierAggregatorService AggregatorService;

    public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
    {
        if (buffer == null)
        {
            throw new ArgumentNullException("buffer");
        }

        SourceSinkEP.myXTagger = new XTagger(AggregatorService.GetClassifier(buffer));
        return SourceSinkEP.myXTagger as ITagger<T>;
    }
}

public static class SourceSinkEP
{
    public static EnvDTE.EditPoint sourceEP = null;
    public static EnvDTE.EditPoint sinkEP = null;
    public static EnvDTE.EditPoint functionEP = null;

    public static XTagger myXTagger;
}