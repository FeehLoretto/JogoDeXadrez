using System;
using tabuleiro;
using jogoXadrez;

namespace Xadrez
{
    class Program
    {
        static void Main(string[] args)
        {
            PosicaoXadrez pos = new PosicaoXadrez('c', 1);
            Console.WriteLine(pos);
            Console.WriteLine(pos.ToPosicao());
            Console.ReadLine();
        } 
    }
}
