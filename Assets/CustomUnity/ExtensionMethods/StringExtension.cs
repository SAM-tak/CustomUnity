namespace CustomUnity
{
    /// <summary>
    /// System.String Extension
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// return count of line
        /// </summary>
        /// <param name="self">string</param>
        /// <returns>Line count</returns>
        public static int LineCount(this string self)
        {
            int ret = 1;
            foreach(var i in self) if(i == '\n') ret++;
            return ret;
        }

        /// <summary>
        /// return count of line
        /// </summary>
        /// <param name="self">string</param>
        /// <param name="wrapColumn">wrap column number</param>
        /// <returns>Line count</returns>
        public static int LineCount(this string self, int wrapColumn)
        {
            int ret = 1;
            int c = 1;
            foreach(var i in self) {
                if(c > wrapColumn) {
                    c = 1;
                    ret++;
                    continue;
                }
                if(i == '\n') ret++;
                c++;
            }
            return ret;
        }
    }
}
