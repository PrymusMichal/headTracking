using headTracking.SwipeType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WeCantSpell.Hunspell;

namespace headTracking
{
    class InputAnalyze
    {
        Dictionary<char, Button> keyboardGrid;
        WordList wordDictionary;
        public InputAnalyze(Dictionary<char, Button> keyboardGrid, WordList wordList)
        {
            this.keyboardGrid = keyboardGrid;
            this.wordDictionary = wordList;
        }
        public char settleButton(double x, double y)
        {
            foreach (var button in keyboardGrid)
            {
                if (button.Value.X1 - 150 < x && button.Value.X2 - 50 > x && button.Value.Y1 < y && button.Value.Y2 > y)
                {
                    return button.Key;
                }
            }
            return '`';
        }
        public IEnumerable<string> analyzeInputNeargye(Dictionary<long, double[]> moveHistory)
        {
            char test = '`';
            char consecutiveChar = '0';
            int counter = 0;
            try
            {
                List<KeyValuePair<char, int>> list = new List<KeyValuePair<char, int>>();
                for (int i = 0; i < moveHistory.Count(); i++)
                {
                    int count = 1;
                    while (settleButton(moveHistory.ElementAt(i).Value[0], moveHistory.ElementAt(i).Value[1]) == settleButton(moveHistory.ElementAt(i + 1).Value[0], moveHistory.ElementAt(i + 1).Value[1]))
                    {
                        i++;
                        count++;
                        if (i + 1 == moveHistory.Count())
                        {
                            break;
                        }
                    }
                    list.Add(new KeyValuePair<char, int>(settleButton(moveHistory.ElementAt(i).Value[0], moveHistory.ElementAt(i).Value[1]), count));
                }

                for (int i = 0; i < list.Count() - 2; i++)
                {
                    if (i + 2 > list.Count() - 1)
                        break;
                    if (list.ElementAt(i + 1).Value < 10 && list.ElementAt(i + 0).Key == list.ElementAt(i + 2).Key)
                    {

                        list.RemoveAt(i + 1);
                    }
                }

                string word = "";
                foreach (var letter in list)
                {
                    if (letter.Value > 0)
                    {
                        for (int i = 0; i < letter.Value; i++)
                        {
                            word += letter.Key;
                        }
                    }
                }

                moveHistory.Clear();
                if (word.Length > 0)
                {
                    var swype = new MatchSwipeType(File.ReadAllLines("EnglishDictionary.txt"));
                    return swype.GetSuggestion(word, 5);
                }
                else
                {
                    return null;
                }
            } catch(Exception e)
            {
                return null;
            }
        }

        public IEnumerable<string> analyzeInput(Dictionary<long, double[]> moveHistory)
        {
            char test = '`';
            char consecutiveChar = '0';
            int counter = 0;
            List<KeyValuePair<char, int>> list = new List<KeyValuePair<char, int>>();
            foreach (var position in moveHistory)
            {
                test = settleButton(position.Value[0], position.Value[1]);
                if (test == '`')
                    continue;
                if (test == consecutiveChar && test != '`')
                {
                    counter++;
                    continue;
                }
                if (test != consecutiveChar && test != '`')
                {
                    list.Add(new KeyValuePair<char, int>(consecutiveChar, counter));
                    counter = 0;
                    consecutiveChar = test;
                }

            }

            var average = list.Average(x => x.Value);
            string word = "";
            foreach (var letter in list)
            {
                if (letter.Value > 25)
                {
                    word += letter.Key;
                }
            }
            moveHistory.Clear();
            IEnumerable<string> suggest = wordDictionary.Suggest(word);
            if (suggest.Any())
            {
                return suggest;
            }
            else
                return null;


        }
    }
}
