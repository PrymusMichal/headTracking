﻿using System;


namespace headTracking.SwipeType
{
    internal static class TextDistance
    {
        internal static int GetDamerauLevenshteinDistance(this string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                return (t ?? "").Length;
            }
            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            if (s.Length > t.Length)
            {
                var temp = s;
                s = t;
                t = temp;
            }
            int sLen = s.Length;
            int tLen = t.Length;

            while ((sLen > 0) && (s[sLen - 1] == t[tLen - 1]))
            {
                sLen--;
                tLen--;
            }

            int start = 0;
            if ((s[0] == t[0]) || (sLen == 0))
            {
                while ((start < sLen) && (s[start] == t[start]))
                {
                    start++;
                }
                sLen -= start;
                tLen -= start;

                if (sLen == 0)
                {
                    return tLen;
                }

                t = t.Substring(start, tLen);
            }

            var v0 = new int[tLen];
            var v2 = new int[tLen];
            for (int j = 0; j < tLen; j++)
            {
                v0[j] = j + 1;
            }

            char sChar = s[0];
            int current = 0;
            for (int i = 0; i < sLen; i++)
            {
                char prevsChar = sChar;
                sChar = s[start + i];
                char tChar = t[0];
                int left = i;
                current = i + 1;
                int nextTransCost = 0;
                for (int j = 0; j < tLen; j++)
                {
                    int above = current;
                    int thisTransCost = nextTransCost;
                    nextTransCost = v2[j];
                    v2[j] = current = left;
                    left = v0[j];
                    char prevtChar = tChar;
                    tChar = t[j];
                    if (sChar == tChar)
                    {
                        continue;
                    }
                    if (left < current)
                    {
                        current = left;
                    }
                    if (above < current)
                    {
                        current = above;
                    }
                    current++;
                    if ((i != 0) && (j != 0) && (sChar == prevtChar) && (prevsChar == tChar))
                    {
                        thisTransCost++;
                        if (thisTransCost < current)
                        {
                            current = thisTransCost;
                        }
                    }
                    v0[j] = current;
                }
            }

            return current;
        }
    }
}
