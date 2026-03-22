namespace Sparkfire.Utility
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string input) => string.IsNullOrWhiteSpace(input);

        public static string NextWord(this string input, in int startIndex) => input?.NextWord(startIndex, out _);
        public static string NextWord(this string input, in int startIndex, out int endIndex)
        {
            endIndex = input.IndexOf(' ', startIndex);
            if(endIndex < startIndex)
            {
                endIndex = input.Length - 1;
                return input[startIndex..].Trim();
            }
            return input[startIndex..endIndex].Trim();
        }

        // -----

        #region Enclosed Text

        /// <summary>
        /// Gets the substring of text inside the first valid pair of parentheses found
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// Substring within the next valid pair of parentheses, excluding the parentheses.
        /// Returns empty string if no valid parentheses pair is found
        /// </returns>
        public static string TextInsideParentheses(this string input) => input.TextInsideEnclosures(out _, out _, '(', ')');

        /// <summary>
        /// Gets the substring of text inside the first valid pair of parentheses found
        /// </summary>
        /// <param name="input"></param>
        /// <param name="openIndex">String index of the first open parentheses</param>
        /// <param name="closeIndex">String index of the close parentheses corresponding to the first one</param>
        /// <returns>
        /// Substring within the next valid pair of parentheses, excluding the parentheses.
        /// Returns empty string if no valid parentheses pair is found
        /// </returns>
        public static string TextInsideParentheses(this string input, out int openIndex, out int closeIndex) => input.TextInsideEnclosures(out openIndex, out closeIndex, '(', ')');

        // -----

        /// <summary>
        /// Gets the substring of text inside the first valid pair of brackets found
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// Substring within the next valid pair of brackets, excluding the brackets.
        /// Returns empty string if no valid brackets pair is found
        /// </returns>
        public static string TextInsideBrackets(this string input) => input.TextInsideEnclosures(out _, out _, '[', ']');

        /// <summary>
        /// Gets the substring of text inside the first valid pair of brackets found
        /// </summary>
        /// <param name="input"></param>
        /// <param name="openIndex">String index of the first open brackets</param>
        /// <param name="closeIndex">String index of the close brackets corresponding to the first one</param>
        /// <returns>
        /// Substring within the next valid pair of brackets, excluding the brackets.
        /// Returns empty string if no valid brackets pair is found
        /// </returns>
        public static string TextInsideBrackets(this string input, out int openIndex, out int closeIndex) => input.TextInsideEnclosures(out openIndex, out closeIndex, '[', ']');

        // -----

        /// <summary>
        /// Gets the substring of text inside the first valid pair of braces found
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// Substring within the next valid pair of braces, excluding the braces.
        /// Returns empty string if no valid braces pair is found
        /// </returns>
        public static string TextInsideBraces(this string input) => input.TextInsideEnclosures(out _, out _, '{', '}');

        /// <summary>
        /// Gets the substring of text inside the first valid pair of braces found
        /// </summary>
        /// <param name="input"></param>
        /// <param name="openIndex">String index of the first open braces</param>
        /// <param name="closeIndex">String index of the close brackets corresponding to the first one</param>
        /// <returns>
        /// Substring within the next valid pair of braces, excluding the braces.
        /// Returns empty string if no valid braces pair is found
        /// </returns>
        public static string TextInsideBraces(this string input, out int openIndex, out int closeIndex) => input.TextInsideEnclosures(out openIndex, out closeIndex, '{', '}');

        // ------------------------------

        private static string TextInsideEnclosures(this string input, out int openIndex, out int closeIndex, in char openChar, in char closeChar)
        {
            int openCharCount = 0, closeCharCount = 0;
            openIndex = -1;
            closeIndex = -1;
            if(input == null)
                return null;
            for(int i = 0; i < input.Length; i++)
            {
                if(input[i] == openChar)
                {
                    openCharCount++;
                    if(openIndex == -1)
                        openIndex = i;
                }
                else if(input[i] == closeChar)
                {
                    closeCharCount++;
                    if(closeCharCount > openCharCount)
                        return "";
                    if(closeCharCount == openCharCount)
                    {
                        closeIndex = i;
                        break;
                    }
                }
            }
            if(openIndex == -1 || closeIndex == -1)
                return "";
            return input[(openIndex + 1)..closeIndex];
        }

        #endregion
    }
}
