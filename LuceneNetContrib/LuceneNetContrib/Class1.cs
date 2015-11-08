using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuceneNetContrib
{
    public class EnglishSnowballAnalyzer : Lucene.Net.Analysis.Snowball.SnowballAnalyzer
    {
        public EnglishSnowballAnalyzer() : base("English", Lucene.Net.Analysis.StopAnalyzer.ENGLISH_STOP_WORDS)
        { }
    }
}