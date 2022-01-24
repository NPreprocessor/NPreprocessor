namespace NPreprocessor
{
    public static class StringExtensions
    {
        public static int LastIndexOf(this string txt, int max, char character)
        {
            for (var i = 0; i < txt.Length && i < max; i++)
            {
                if (txt[i] == character)
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool StartsWith(this string txt, int start, string prefix)
        {
            for (var i = 0; i < prefix.Length && i < txt.Length; i++)
            {
                if (prefix[i] != txt[i + start])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
