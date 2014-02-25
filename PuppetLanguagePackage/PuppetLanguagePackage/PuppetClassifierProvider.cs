using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Pupprt
{

    using System.ComponentModel.Composition;
    using System.Windows.Media;

    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Utilities;


    [Export(typeof(EditorFormatDefinition))]
    [Name(PuppetFormatDefinitions.BraceMatching)]
    [UserVisible(true)]
    internal class BraceMatchingFormatDefinition : MarkerFormatDefinition
    {
        /// <summary>
        /// In the constructor for HighlightWordFormatDefinition, define its display name and appearance.
        /// The Background property defines the fill color, while the Foreground property defines the border color.
        /// </summary>
        public BraceMatchingFormatDefinition()
        {
            BackgroundColor = Color.FromRgb(14, 69, 131);
            ForegroundColor = Color.FromRgb(173, 192, 211);
            DisplayName = "Puppet Brace Matching";
            ZOrder = 5;
        }
    }


    internal class BraceMatchingTag : TextMarkerTag
    {
        /// <summary>
        /// In the constructor, pass in the name of the format definition.
        /// </summary>
        public BraceMatchingTag()
            : base(PuppetFormatDefinitions.BraceMatching)
        {
        }
    }


    [Export(typeof(IViewTaggerProvider))]
    [ContentType(PuppetLanguage.ContentType)]
    [TagType(typeof(BraceMatchingTag))]
    internal class BraceMatchingTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (textView == null)
                return null;

            // provide highlighting only on the top-level buffer
            if (textView.TextBuffer != buffer)
                return null;

            return new BraceMatchingTagger(textView, buffer) as ITagger<T>;
        }
    }


    internal class BraceMatchingTagger : ITagger<TextMarkerTag>
    {
        private ITextView View { get; set; }

        private ITextBuffer SourceBuffer { get; set; }

        private SnapshotPoint? CurrentChar { get; set; }

        private readonly Dictionary<char, char> m_braceList;

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        internal BraceMatchingTagger(ITextView view, ITextBuffer sourceBuffer)
        {
            // here the keys are the open braces, and the values are the close braces
            m_braceList = new Dictionary<char, char> {{'{', '}'}, {'[', ']'}, {'(', ')'}};

            View = view;
            SourceBuffer = sourceBuffer;
            CurrentChar = null;

        }
    }








    internal static class PuppetLanguage
    {
        public const string ContentType = "PuppetContentType";

        public const string FileExtension = ".proto";

        [Export]
        [Name(ContentType)]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition PuppetContentTypeDefinition = null;

    }



    public enum CodeType
    {
        Text = 0,         // simply text
        Keyword = 1,      // a keyword
        Comment = 2,      // a comment
        Identifier = 3,   // an identifier
        String = 4,       // a string
        Number = 5,       // a number
        Enums = 6,        // Enum fields
        SymDef = 7,       // symbol definition
        SymRef = 8,       // symbol reference
        FieldRule = 9,    // required, optional, repeated
        TopLevelCmd = 10, // package, import, enum, message, option, service
        Namespace = 11,   // name of the package
        Error = 12,       // error tag
    }

    [Export(typeof(ITaggerProvider))]
    [ContentType(PuppetLanguage.ContentType)]
    [Name("PuppetSyntaxProvider")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class PuppetClassifierProvider : ITaggerProvider
    {
        /// <summary>
        /// Import the classification registry to be used for getting a reference
        /// to the custom classification type later.
        /// </summary>
        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry { get; set; }

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory { get; set; }

        [Import]
        internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            // create a single tagger for each buffer.
            Func<ITagger<T>> sc = () => new PuppetClassifier(buffer, aggregatorFactory, ClassificationTypeRegistry) as ITagger<T>;

            var ct = ContentTypeRegistryService.ContentTypes;

            return buffer.Properties.GetOrCreateSingletonProperty(sc);
        }
    }


    public class PuppetTokenTag : ITag
    {
        public CodeType _type { get; private set; }

        public PuppetTokenTag(CodeType type)
        {
            _type = type;
        }
    }

    /// <summary>
    /// The error Tag to show the red squiggle line and hold the error message
    /// </summary>
    public class PuppetErrorTag : PuppetTokenTag
    {
        public string _message { get; private set; }

        public PuppetErrorTag(string message)
            : base(CodeType.Error)
        {
            _message = message;
        }
    }


    internal sealed class PuppetClassifier : ITagger<ClassificationTag>
    {
        readonly ITextBuffer _buffer;

        readonly ITagAggregator<PuppetTokenTag> _aggregator;

        private readonly Guid _outputPaneGuid = new Guid("{7451EC7F-98F4-48F3-9600-78DDFD826BBC}");
        private const string _outputWindowName = "Puppet";

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        readonly IDictionary<CodeType, IClassificationType> _puppetTypes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <param name="aggregatorFactory"></param>
        /// <param name="typeService"></param>
        internal PuppetClassifier(ITextBuffer buffer, IBufferTagAggregatorFactoryService aggregatorFactory, IClassificationTypeRegistryService typeService)
        {
            IVsOutputWindowPane myOutputPane;
            _buffer = buffer;
            _aggregator = aggregatorFactory.CreateTagAggregator<PuppetTokenTag>(buffer);

            _aggregator.TagsChanged += _aggregator_TagsChanged;

            IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            if (outputWindow != null)
            {
                outputWindow.CreatePane(ref _outputPaneGuid, _outputWindowName, 1, 1);
                outputWindow.GetPane(ref _outputPaneGuid, out myOutputPane);
            }

            // create mapping from token types to classification types
            _puppetTypes = new Dictionary<CodeType, IClassificationType>();
            _puppetTypes[CodeType.Text] = typeService.GetClassificationType(PuppetFormatDefinitions.Text);
            _puppetTypes[CodeType.Keyword] = typeService.GetClassificationType(PuppetFormatDefinitions.Keyword);
            _puppetTypes[CodeType.Comment] = typeService.GetClassificationType(PuppetFormatDefinitions.Comment);
            _puppetTypes[CodeType.Identifier] = typeService.GetClassificationType(PuppetFormatDefinitions.Identifier);
            _puppetTypes[CodeType.String] = typeService.GetClassificationType(PuppetFormatDefinitions.String);
            _puppetTypes[CodeType.Number] = typeService.GetClassificationType(PuppetFormatDefinitions.Number);
            _puppetTypes[CodeType.Error] = typeService.GetClassificationType(PuppetFormatDefinitions.Text);

            _puppetTypes[CodeType.Enums] = typeService.GetClassificationType(PuppetFormatDefinitions.Enum);
            _puppetTypes[CodeType.SymDef] = typeService.GetClassificationType(PuppetFormatDefinitions.SymDef);
            _puppetTypes[CodeType.SymRef] = typeService.GetClassificationType(PuppetFormatDefinitions.SymRef);
            _puppetTypes[CodeType.FieldRule] = typeService.GetClassificationType(PuppetFormatDefinitions.FieldRule);
            _puppetTypes[CodeType.TopLevelCmd] = typeService.GetClassificationType(PuppetFormatDefinitions.TopLevelCmd);
            _puppetTypes[CodeType.Namespace] = typeService.GetClassificationType(PuppetFormatDefinitions.Keyword);
        }

        void _aggregator_TagsChanged(object sender, TagsChangedEventArgs e)
        {
            var temp = TagsChanged;
            if (temp != null)
            {
                NormalizedSnapshotSpanCollection spans = e.Span.GetSpans(_buffer.CurrentSnapshot);
                if (spans.Count > 0)
                {
                    SnapshotSpan span = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End);
                    temp(this, new SnapshotSpanEventArgs(span));
                }
            }
        }

        /// <summary>
        /// Translate each TokenColor to an appropriate ClassificationTag
        /// </summary>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var tagSpan in _aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return new TagSpan<ClassificationTag>(tagSpans[0], new ClassificationTag(_puppetTypes[tagSpan.Tag._type]));
            }
        }
    }



    internal sealed class PuppetFormatDefinitions
    {
        internal const string Highlight = "puppet.highlightword";

        internal const string BraceMatching = "puppet.braceMatching";

        internal const string Enum = "puppet.enum";

        internal const string FieldRule = "puppet.FieldRule";

        internal const string TopLevelCmd = "puppet.TopLevelCmd";

        internal const string SymDef = "Symbol Definition";

        internal const string SymRef = "Symbol Reference";

        internal const string Comment = "Comment";

        internal const string Number = "Number";

        internal const string String = "String";

        internal const string Operator = "Operator";

        internal const string Keyword = "Keyword";

        internal const string Identifier = "Identifier";

        internal const string Text = "Text";
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PuppetFormatDefinitions.Enum)]
    [Name(PuppetFormatDefinitions.Enum)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
    internal sealed class PuppetEnum : ClassificationFormatDefinition
    {
        public PuppetEnum()
        {
            IsBold = false;
            ForegroundColor = Colors.Olive;
            DisplayName = "Puppet Enum";
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PuppetFormatDefinitions.FieldRule)]
    [Name(PuppetFormatDefinitions.FieldRule)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
    internal sealed class PuppetFieldRule : ClassificationFormatDefinition
    {
        public PuppetFieldRule()
        {
            IsBold = false;
            ForegroundColor = Colors.Orchid;
            DisplayName = "Puppet Field Rule";
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PuppetFormatDefinitions.TopLevelCmd)]
    [Name(PuppetFormatDefinitions.TopLevelCmd)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
    internal sealed class PuppetTopLevelCmd : ClassificationFormatDefinition
    {
        public PuppetTopLevelCmd()
        {
            IsBold = false;
            ForegroundColor = Colors.DodgerBlue;
            DisplayName = "Puppet Top Level Cmd";
        }
    }

    internal static class PuppetClassificationTypes
    {
        /// <summary>
        /// Defines the "Puppet Enum" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PuppetFormatDefinitions.Enum)]
        internal static ClassificationTypeDefinition PuppetEnum = null;

        /// <summary>
        /// Defines the "Puppet FieldRule" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PuppetFormatDefinitions.FieldRule)]
        internal static ClassificationTypeDefinition PuppetFieldRule = null;

        /// <summary>
        /// Defines the "Puppet TopLevelCmd" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PuppetFormatDefinitions.TopLevelCmd)]
        internal static ClassificationTypeDefinition PuppetTopLevelCmd = null;
    }


}
