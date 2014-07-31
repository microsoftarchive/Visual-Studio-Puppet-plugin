// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace EditorClassifier
{
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Utilities;
    using System.ComponentModel.Composition;
    using System.Windows.Media;

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.ClassrefDisplayName)]
    [UserVisible(true)]
    [ClassificationType(ClassificationTypeNames = Constants.ClassrefName)]
    [Order(After = Priority.Default, Before = Priority.High)]
    class ClassrefFormatDefinition : ClassificationFormatDefinition
    {
        public ClassrefFormatDefinition()
        {
            this.ForegroundColor = Colors.MediumOrchid;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.BlockCommentDisplayName)]
    [UserVisible(true)]
    [ClassificationType(ClassificationTypeNames = Constants.BlockCommentName)]
    [Order(After = Priority.Default, Before = Priority.High)]
    class BlockCommentFormatDefinition : ClassificationFormatDefinition
    {
        public BlockCommentFormatDefinition()
        {
            this.ForegroundColor = Colors.Green;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.LineCommentDisplayName)]
    [UserVisible(true)]
    [ClassificationType(ClassificationTypeNames = Constants.LineCommentName)]
    [Order(After = Priority.Default, Before = Priority.High)]
    class LineCommentFormatDefinition : ClassificationFormatDefinition
    {
        public LineCommentFormatDefinition()
        {
            this.ForegroundColor = Colors.Green;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.VariableDisplayName)]
    [UserVisible(true)]
    [ClassificationType(ClassificationTypeNames = Constants.VariableName)]
    [Order(After = Priority.Default, Before = Priority.High)]
    class VariableFormatDefinition : ClassificationFormatDefinition
    {
        public VariableFormatDefinition()
        {
            this.ForegroundColor = Colors.Goldenrod;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.RegexDisplayName)]
    [UserVisible(true)]
    [ClassificationType(ClassificationTypeNames = Constants.RegexName)]
    [Order(After = Priority.Default, Before = Priority.High)]
    class RegexFormatDefinition : ClassificationFormatDefinition
    {
        public RegexFormatDefinition()
        {
            this.ForegroundColor = Colors.Brown;
        }
    }
}
