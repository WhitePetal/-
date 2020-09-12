using System;

namespace FilterWord
{
    class Program
    {
        private static FilterWord fw = new FilterWord();
        static void Main(string[] args)
        {
            fw.InitFilterWordCfg("filterword.xml");
            Console.WriteLine(fw.SerachFilterWordAndReplace("哈哈哈我操你妈哈哈哈"));
        }
    }
}
