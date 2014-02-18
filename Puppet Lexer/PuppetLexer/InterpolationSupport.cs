using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;

namespace PuppetLexer
{
    // This module is an integral part of the Lexer.
    // It defines interpolation support
    // PERFORMANCE NOTE: There are 4 very similar methods in this module that are designed to be as
    // performant as possible. While it is possible to parameterize them into one common method, the overhead
    // of passing parameters and evaluating conditional logic has a negative impact on performance.
    //
    public class InterpolationSupport
    {
        private readonly Regex PATTERN_VARIABLE = new Regex("(::)?(\\w+::)*\\w+");

        // This is the starting point for a double quoted string with possible interpolation
        // The structure mimics that of the grammar.
        // The logic is explicit (where the former implementation used parameters/strucures) given to a
        // generic handler.
        // (This is both easier to understand and faster).

        public InterpolationSupport(ArrayList tokenQueue)
        {
            this.tokenQueue = tokenQueue;
        }

        //private locator locator;
        Hashtable lexingContext = new Hashtable();
        ArrayList tokenQueue;
        StringScanner scanner;
        Lexer lexer = new Lexer();

        public ArrayList InterpolateDq()
        {
            int before = scanner.Pos;

            //skip the leading " by doing a scan since the slurp_dqstring uses last matched when there is an error
            scanner.Scan(new Regex("\""));
            string[] slurpDQString = { "some", "thing" }; //TODO: implement slurp_dqstring()
            string value, text; 
            value = text = slurpDQString[0];
            string terminator = slurpDQString[1];
            int after = scanner.Pos;

            while (true) {
                switch (terminator) { 
                    case "\"":
                        return lexer.EmitCompleted(new string[] { "STRING", text, Convert.ToString(scanner.Pos - before) }, before);
                    case "${":
                        int count = (int)lexingContext["brace_count"];
                        lexingContext["brace_count"] = count + 1;
                        lexer.EmitCompleted(new string[] { "DQPRE", text, Convert.ToString(scanner.Pos - before) }, before);
                        //TODO: Implement this
                        //enqueUntil(count);
                        break;
                    case "$":
                        string varName = scanner.Scan(PATTERN_VARIABLE);
                        if(!(String.IsNullOrEmpty(varName))) {
                            lexer.EnqueueCompleted(new string[] {"DQMID", text, Convert.ToString(after-before-1) }, before);
                            lexer.EnqueueCompleted(new string[] {"VARIABLE", varName, Convert.ToString(scanner.Pos - after +1)}, after -1);                   
                        }
                        else {
                            text += value;
                            //value,terminator = self.send(slurpfunc)
                            after = scanner.Pos;
                        }
                        break;
                }
            }
            //intrepolateTailDQ(); TODO
        }

        public ArrayList InterpolateTailDq()
        {
            int before = scanner.Pos;

            //skip the leading " by doing a scan since the slurp_dqstring uses last matched when there is an error
            scanner.Scan(new Regex("\""));
            string[] slurpDQString = { "some", "thing" }; //TODO: implement slurp_dqstring()
            string value, text;
            value = text = slurpDQString[0];
            string terminator = slurpDQString[1];
            int after = scanner.Pos;

            while (true) {
                switch (terminator) { 
                    case "\"":
                        return lexer.EmitCompleted(new string[] { "DQPOST", text, Convert.ToString(scanner.Pos - before) }, before);
                    case "${":
                        int count = (int)lexingContext["brace_count"];
                        lexingContext["brace_count"] = count + 1;
                        lexer.EmitCompleted(new string[] { "DQMID", text, Convert.ToString(scanner.Pos - before) }, before);
                        //TODO: Implement this
                        //enqueUntil(count);
                        break;
                    case "$":
                        string varName = scanner.Scan(PATTERN_VARIABLE);
                        if(!(String.IsNullOrEmpty(varName))) {
                            lexer.EnqueueCompleted(new string[] {"DQMID", text, Convert.ToString(after-before-1) }, before);
                            lexer.EnqueueCompleted(new string[] {"VARIABLE", varName, Convert.ToString(scanner.Pos - after +1)}, after -1);                   
                        }
                        else {
                            text += value;
                            //value,terminator = self.send(slurpfunc)
                            after = scanner.Pos;
                        }
                        break;
                }
            }
            //intrepolateTailDQ(); TODO
        }

        public void InterpolateUq()
        {
            int before = scanner.Pos;

            //skip the leading " by doing a scan since the slurp_dqstring uses last matched when there is an error
            string[] slurpUQString = { "some", "thing" }; //TODO: implement slurp_uqstring()
            string value, text;
            value = text = slurpUQString[0];
            string terminator = slurpUQString[1];
            int after = scanner.Pos;
            bool flag = true;

            while (flag)
            {
                switch (terminator)
                {
                    case "":
                        lexer.EnqueueCompleted(new string[] { "STRING", text, Convert.ToString(scanner.Pos - before) }, before);
                        flag = false;
                        break;
                    case "${":
                        int count = (int)lexingContext["brace_count"];
                        lexingContext["brace_count"] = count + 1;
                        lexer.EmitCompleted(new string[] { "DQPRE", text, Convert.ToString(scanner.Pos - before) }, before);
                        //TODO: Implement this
                        //enqueUntil(count);
                        break;
                    case "$":
                        string varName = scanner.Scan(PATTERN_VARIABLE);
                        if (!(String.IsNullOrEmpty(varName)))
                        {
                            lexer.EnqueueCompleted(new string[] { "DQPRE", text, Convert.ToString(after - before - 1) }, before);
                            lexer.EnqueueCompleted(new string[] { "VARIABLE", varName, Convert.ToString(scanner.Pos - after + 1) }, after - 1);
                        }
                        else
                        {
                            text += value;
                            //value,terminator = self.send(slurpfunc)
                            after = scanner.Pos;
                        }
                        break;
                }
            }
            InterpolateTailUq(); 
        }

        public void InterpolateTailUq()
        {
            int before = scanner.Pos;

            //skip the leading " by doing a scan since the slurp_dqstring uses last matched when there is an error
           
            string[] slurpUQString = { "some", "thing" }; //TODO: implement slurp_uqstring()
            string value, text;
            value = text = slurpUQString[0];
            string terminator = slurpUQString[1];
            int after = scanner.Pos;
            bool flag = true;

            while (flag)
            {
                switch (terminator)
                {
                    case "":
                        lexer.EnqueueCompleted(new string[] { "DQPOST", text, Convert.ToString(scanner.Pos - before) }, before);
                        flag = false;
                        break;
                    case "${":
                        int count = (int)lexingContext["brace_count"];
                        lexingContext["brace_count"] = count + 1;
                        lexer.EnqueueCompleted(new string[] { "DQMID", text, Convert.ToString(scanner.Pos - before) }, before);
                        //TODO: Implement this
                        //enqueUntil(count);
                        break;
                    case "$":
                        string varName = scanner.Scan(PATTERN_VARIABLE);
                        if (!(String.IsNullOrEmpty(varName)))
                        {
                            lexer.EnqueueCompleted(new string[] { "DQMID", text, Convert.ToString(after - before - 1) }, before);
                            lexer.EnqueueCompleted(new string[] { "VARIABLE", varName, Convert.ToString(scanner.Pos - after + 1) }, after - 1);
                        }
                        else
                        {
                            text += value;
                            //value,terminator = slurp_uqstring;
                            after = scanner.Pos;
                        }
                        break;
                }
            }
            //intrepolateTailUQ(); TODO
        }

        //Enqueues lexed tokens until either end of input, or the given brace_count is reached

        public void EnqueueUntil(int braceCount)
        {
            scanner.Skip(lexer.PATTERN_WS);
            int queueSize = tokenQueue.Count;
            while (!scanner.eos)
            {
                ArrayList token = lexer.LexToken();
                if (token.Count > 0)
                {
                    string tokenName = (string)token[0];
                    lexingContext["after"] = tokenName;
                    if (tokenName == "RBRACE" && (int)lexingContext["brace_count"] == braceCount)
                    {
                        if (tokenQueue.Count - queueSize == 1)
                        {
                            //tokenQueue[queueSize - 1]  = transformToVariable(tokenQueue[queueSize - 1]);
                        }
                        return;
                    }
                    tokenQueue.Add(token);
                }
                else
                {
                    scanner.Skip(lexer.PATTERN_WS);
                }
            }
        }

        private ArrayList transform_to_variable(ArrayList token)
        {
            string tokenName = (string)token[0];

            if (tokenName == "NUMBER" || tokenName == "NAME" || lexer.KEYWORDS.ContainsKey(tokenName))
            {
                tokenValue tokenValue = (tokenValue)token[1];
                string[] tokenArray = tokenValue.tokenArray;
                return new ArrayList { "VARIABLE", new tokenValue(new string[] { "VARIABLE", tokenArray[1], tokenArray[2] }, tokenValue.offset, tokenValue.locator) };
            }
            else
            {
                return token;
            }
        }

        //Interpolates unquoted string and transfers the result to the given lexer
        //(This is used when a second lexer instance is used to lex a substring)

        public void InterpolateUqTo(Lexer lexer)
        {
            InterpolateUq();
            ArrayList tokenQueue = this.tokenQueue;
            while (tokenQueue.Count == 0)
            {
                lexer.Enqueue((ArrayList)tokenQueue[0]);
                tokenQueue.RemoveAt(0);
            }
        }
    }
}
