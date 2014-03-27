// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace Puppet.ParserGenerator
{

    /// <summary>
    /// Classes implementing this interface must supply a
    /// method that merges two location objects to return
    /// a new object of the same type.
    /// MPPG-generated parsers have the default location
    /// action equivalent to "@$ = @1.Merge(@N);" where N
    /// is the right-hand-side length of the production.
    /// </summary>
    /// <typeparam name="YYLTYPE"></typeparam>
    public interface IMerge<YYLTYPE>
    {
        YYLTYPE Merge(YYLTYPE last);
    }

    /// <summary>
    /// This is the default class that carries location
    /// information from the scanner to the parser.
    /// If you don't declare "%YYLTYPE Foo" the parser
    /// will expect to deal with this type.
    /// </summary>
    public class LexLocation : IMerge<LexLocation>
    {
        public int sLin; // start line
        public int sCol; // start column
        public int eLin; // end line
        public int eCol; // end column

        public LexLocation() 
        { 
        }

        public LexLocation(int sl, int sc, int el, int ec)
        { 
            sLin = sl; 
            sCol = sc; 
            eLin = el; 
            eCol = ec; 
        }

        public LexLocation Merge(LexLocation last)
        { 
            return new LexLocation(this.sLin, this.sCol, last.eLin, last.eCol); 
        }

        public override string ToString()
        {
            return string.Format("[{0}:{1}, {2}:{3}]", this.sLin, this.sCol, this.eLin, this.eCol);
        }
    }

    public interface IColorScan
    {
        void SetSource(string source, int offset);
        int GetNext(ref int state, out int start, out int end);
    }
   
    /// <summary>
    /// Abstract scanner class that MPPG expects its scanners to extend.
    /// </summary>
    /// <typeparam name="YYSTYPE"></typeparam>
    /// <typeparam name="YYLTYPE"></typeparam>
	public abstract class AScanner<YYSTYPE,YYLTYPE> 
        where YYSTYPE : struct
        where YYLTYPE : IMerge<YYLTYPE>
    {
		public YYSTYPE yylval;              // lexical value: set by scanner
        public YYLTYPE yylloc;              // location value: set by scanner

        public abstract int yylex();

        public virtual void yyerror(string format, params object[] args) {}
    }
}
