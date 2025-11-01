namespace Sparkfire.Utility
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string input) => string.IsNullOrWhiteSpace(input);

        /// <summary>
        /// Gets the substring of text inside the first valid pair of parentheses found
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// Substring within the next valid pair of parentheses, excluding the parentheses.
        /// Returns empty string if no valid parentheses pair is found
        /// </returns>
        public static string TextInsideParentheses(this string input) => input.TextInsideParentheses(out _, out _);

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
        public static string TextInsideParentheses(this string input, out int openIndex, out int closeIndex)
        {
            int openParenCount = 0, closeParenCount = 0;
            openIndex = -1;
            closeIndex = -1;
            for(int i = 0; i < input.Length; i++)
            {
                if(input[i] == '(')
                {
                    openParenCount++;
                    if(openIndex == -1)
                        openIndex = i;
                }
                else if(input[i] == ')')
                {
                    closeParenCount++;
                    if(closeParenCount > openParenCount)
                        return "";
                    if(closeParenCount == openParenCount)
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

        // ------------------------------

        /// <summary>
        /// Gets the substring of text inside the first valid pair of brackets found
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// Substring within the next valid pair of brackets, excluding the brackets.
        /// Returns empty string if no valid brackets pair is found
        /// </returns>
        public static string TextInsideBrackets(this string input) => input.TextInsideBrackets(out _, out _);

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
        public static string TextInsideBrackets(this string input, out int openIndex, out int closeIndex)
        {
            int openBracketCount = 0, closeBracketCount = 0;
            openIndex = -1;
            closeIndex = -1;
            for(int i = 0; i < input.Length; i++)
            {
                if(input[i] == '[')
                {
                    openBracketCount++;
                    if(openIndex == -1)
                        openIndex = i;
                }
                else if(input[i] == ']')
                {
                    closeBracketCount++;
                    if(closeBracketCount > openBracketCount)
                        return "";
                    if(closeBracketCount == openBracketCount)
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
    }
}
