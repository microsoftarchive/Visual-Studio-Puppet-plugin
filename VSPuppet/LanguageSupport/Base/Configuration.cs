/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/
namespace Puppet
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.TextManager.Interop;

    public static partial class Configuration
    {
        public struct TokenDefinition
        {
            public TokenDefinition(TokenType type, TokenColor color, TokenTriggers triggers)
            {
                this.TokenType = type;
                this.TokenColor = color;
                this.TokenTriggers = triggers;
            }

            public TokenType TokenType;
            public TokenColor TokenColor;
            public TokenTriggers TokenTriggers;
        }


        private static TokenDefinition defaultDefinition = new TokenDefinition(TokenType.Text, TokenColor.Text, TokenTriggers.None);

        private static Dictionary<int, TokenDefinition> definitions = new Dictionary<int, Configuration.TokenDefinition>();

/*
        private static List<IVsColorableItem> colorableItems = new List<IVsColorableItem>();

        public static IList<IVsColorableItem> ColorableItems
        {
            get { return Configuration.colorableItems; }
        }

        public static TokenColor CreateColor(string name, COLORINDEX foreground, COLORINDEX background)
        {
            return Configuration.CreateColor(name, foreground, background, false, false);
        }

        public static TokenColor CreateColor(string name, COLORINDEX foreground, COLORINDEX background, bool bold, bool strikethrough)
        {
            Configuration.colorableItems.Add(new ColorableItem(name, foreground, background, bold, strikethrough));
            return (TokenColor)Configuration.colorableItems.Count;
        }
*/
        public static void ColorToken(int token, TokenType type, TokenColor color, TokenTriggers trigger)
        {
            Configuration.definitions[token] = new TokenDefinition(type, color, trigger);
        }

        public static TokenDefinition GetDefinition(int token)
        {
            TokenDefinition result;
            return Configuration.definitions.TryGetValue(token, out result) ? result : Configuration.defaultDefinition;
        }
    }

    public class ColorableItem : IVsColorableItem
    {
        private string displayName;
        private COLORINDEX background;
        private COLORINDEX foreground;
        private uint fontFlags = (uint) FONTFLAGS.FF_DEFAULT;

        public ColorableItem(string displayName, COLORINDEX foreground, COLORINDEX background, bool bold, bool strikethrough)
        {
            this.displayName = displayName;
            this.background = background;
            this.foreground = foreground;

            if (bold)
                this.fontFlags = this.fontFlags | (uint)FONTFLAGS.FF_BOLD;
            if (strikethrough)
                this.fontFlags = this.fontFlags | (uint)FONTFLAGS.FF_STRIKETHROUGH;
        }

        #region IVsColorableItem Members

        public int GetDefaultColors(COLORINDEX[] piForeground, COLORINDEX[] piBackground)
        {
            if (null == piForeground)
            {
                throw new ArgumentNullException("piForeground");
            }
            if (0 == piForeground.Length)
            {
                throw new ArgumentOutOfRangeException("piForeground");
            }
            piForeground[0] = foreground;

            if (null == piBackground)
            {
                throw new ArgumentNullException("piBackground");
            }
            if (0 == piBackground.Length)
            {
                throw new ArgumentOutOfRangeException("piBackground");
            }
            piBackground[0] = background;

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int GetDefaultFontFlags(out uint pdwFontFlags)
        {
            pdwFontFlags = this.fontFlags;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int GetDisplayName(out string pbstrName)
        {
            pbstrName = displayName;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        #endregion IVsColorableItem Members
    }
}