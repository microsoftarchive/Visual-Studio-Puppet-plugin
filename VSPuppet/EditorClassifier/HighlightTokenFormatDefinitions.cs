/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace EditorClassifier
{
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Utilities;
    using System.ComponentModel.Composition;
    using System.Windows.Media;

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.KeywordName)]
    [UserVisible(true)]
    class KeywordFormatDefinition : MarkerFormatDefinition
    {
        public KeywordFormatDefinition()
        {
            //this.BackgroundColor = COLORINDEX.CI_USERTEXT_BK;
            this.ForegroundColor = Colors.Blue;
            this.DisplayName = Constants.KeywordDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.IdentifierName)]
    [UserVisible(true)]
    class IdentifierFormatDefinition : MarkerFormatDefinition
    {
        public IdentifierFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.LightSeaGreen;
            this.DisplayName = Constants.IdentifierDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.StringName)]
    [UserVisible(true)]
    class StringFormatDefinition : MarkerFormatDefinition
    {
        public StringFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.Red;
            this.DisplayName = Constants.StringDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.NumberName)]
    [UserVisible(true)]
    class NumberFormatDefinition : MarkerFormatDefinition
    {
        public NumberFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.DarkOrange;
            this.DisplayName = Constants.NumberDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.TextName)]
    [UserVisible(true)]
    class TextFormatDefinition : MarkerFormatDefinition
    {
        public TextFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.Black;
            this.DisplayName = Constants.TextDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.OperatorName)]
    [UserVisible(true)]
    class OperatorFormatDefinition : MarkerFormatDefinition
    {
        public OperatorFormatDefinition()
        {
            //this.BackgroundColor = Colors.DarkTurquoise;
            this.ForegroundColor = Colors.Peru;
            this.DisplayName = Constants.OperatorDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.DelimiterName)]
    [UserVisible(true)]
    class DelimiterFormatDefinition : MarkerFormatDefinition
    {
        public DelimiterFormatDefinition()
        {
            //this.BackgroundColor = Colors.DarkBlue;
            this.ForegroundColor = Colors.Blue;
            this.DisplayName = Constants.DelimiterDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.ClassrefName)]
    [UserVisible(true)]
    class ClassrefFormatDefinition : MarkerFormatDefinition
    {
        public ClassrefFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.MediumOrchid;
            this.DisplayName = Constants.ClassrefDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.BlockCommentName)]
    [UserVisible(true)]
    class BlockCommentFormatDefinition : MarkerFormatDefinition
    {
        public BlockCommentFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.Green;
            this.DisplayName = Constants.BlockCommentDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.LineCommentName)]
    [UserVisible(true)]
    class LineCommentFormatDefinition : MarkerFormatDefinition
    {
        public LineCommentFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.DarkGray;
            this.DisplayName = Constants.LineCommentDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.VariableName)]
    [UserVisible(true)]
    class VariableFormatDefinition : MarkerFormatDefinition
    {
        public VariableFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.LightBlue;
            this.DisplayName = Constants.VariableDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.RegexName)]
    [UserVisible(true)]
    class RegexFormatDefinition : MarkerFormatDefinition
    {
        public RegexFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.Salmon;
            this.DisplayName = Constants.RegexDisplayName;
            this.ZOrder = 5;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.ErrorName)]
    [UserVisible(true)]
    class ErrorFormatDefinition : MarkerFormatDefinition
    {
        public ErrorFormatDefinition()
        {
            //this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.Crimson;
            this.DisplayName = Constants.ErrorDisplayName;
            this.ZOrder = 5;
        }
    }


}
