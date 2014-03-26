// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
namespace EditorClassifier
{
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Utilities;
    using System.ComponentModel.Composition;
    using System.Windows.Media;

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.KeywordName)]
    [UserVisible(true)]
    class KeywordFormatDefinition : ClassificationFormatDefinition
    {
        public KeywordFormatDefinition()
        {
            this.ForegroundColor = Colors.Blue;
            this.DisplayName = Constants.KeywordDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.IdentifierName)]
    [UserVisible(true)]
    class IdentifierFormatDefinition : ClassificationFormatDefinition
    {
        public IdentifierFormatDefinition()
        {
            this.ForegroundColor = Colors.Black;
            this.DisplayName = Constants.IdentifierDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.StringName)]
    [UserVisible(true)]
    class StringFormatDefinition : ClassificationFormatDefinition
    {
        public StringFormatDefinition()
        {
            this.ForegroundColor = Colors.Green;
            this.DisplayName = Constants.StringDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.NumberName)]
    [UserVisible(true)]
    class NumberFormatDefinition : ClassificationFormatDefinition
    {
        public NumberFormatDefinition()
        {
            this.ForegroundColor = Colors.Red;
            this.DisplayName = Constants.NumberDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.TextName)]
    [UserVisible(true)]
    class TextFormatDefinition : ClassificationFormatDefinition
    {
        public TextFormatDefinition()
        {
            this.ForegroundColor = Colors.Black;
            this.DisplayName = Constants.TextDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.OperatorName)]
    [UserVisible(true)]
    class OperatorFormatDefinition : ClassificationFormatDefinition
    {
        public OperatorFormatDefinition()
        {
            this.ForegroundColor = Colors.Blue;
            this.DisplayName = Constants.OperatorDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.DelimiterName)]
    [UserVisible(true)]
    class DelimiterFormatDefinition : ClassificationFormatDefinition
    {
        public DelimiterFormatDefinition()
        {
            this.ForegroundColor = Colors.Blue;
            this.DisplayName = Constants.DelimiterDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.ClassrefName)]
    [UserVisible(true)]
    class ClassrefFormatDefinition : ClassificationFormatDefinition
    {
        public ClassrefFormatDefinition()
        {
            this.ForegroundColor = Colors.MediumOrchid;
            this.DisplayName = Constants.ClassrefDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.BlockCommentName)]
    [UserVisible(true)]
    class BlockCommentFormatDefinition : ClassificationFormatDefinition
    {
        public BlockCommentFormatDefinition()
        {
            this.ForegroundColor = Colors.Green;
            this.DisplayName = Constants.BlockCommentDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.LineCommentName)]
    [UserVisible(true)]
    class LineCommentFormatDefinition : ClassificationFormatDefinition
    {
        public LineCommentFormatDefinition()
        {
            this.ForegroundColor = Colors.DarkGray;
            this.DisplayName = Constants.LineCommentDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.VariableName)]
    [UserVisible(true)]
    class VariableFormatDefinition : ClassificationFormatDefinition
    {
        public VariableFormatDefinition()
        {
            this.ForegroundColor = Colors.Goldenrod;
            this.DisplayName = Constants.VariableDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.RegexName)]
    [UserVisible(true)]
    class RegexFormatDefinition : ClassificationFormatDefinition
    {
        public RegexFormatDefinition()
        {
            this.ForegroundColor = Colors.Brown;
            this.DisplayName = Constants.RegexDisplayName;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.ErrorName)]
    [UserVisible(true)]
    class ErrorFormatDefinition : ClassificationFormatDefinition
    {
        public ErrorFormatDefinition()
        {
            this.ForegroundColor = Colors.Red;
            this.IsItalic = true;
            this.IsBold = true;
            this.DisplayName = Constants.ErrorDisplayName;
        }
    }
}
