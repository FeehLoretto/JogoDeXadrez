using System.Collections.Generic;
using tabuleiro;

namespace jogoXadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro Tab { get; private set; }
        public int Turno { get; private set; }
        public Cor JogadorAtual { get; private set; }
        public bool Terminada { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;

        public PartidaDeXadrez()
        {
            Tab = new Tabuleiro(8, 8);
            Turno = 1;
            JogadorAtual = Cor.Azul;
            Terminada = false;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            ColocarPecas();
        }

        public void ExecutaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = Tab.RetirarPeca(origem);
            p.IncrementarQntMovimentos();
            Peca capturada = Tab.RetirarPeca(destino);
            Tab.ColocarPeca(p, destino);
            if (capturada != null)
            {
                capturadas.Add(capturada);
            }
        }

        public void RealizaJogada(Posicao origem, Posicao destino)
        {
            ExecutaMovimento(origem, destino);
            Turno++;
            MudaJogador();
        }

        public void ValidarPosicaoOrigem(Posicao pos)
        {
            if (Tab.Peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição escolhida");
            }
            if (JogadorAtual != Tab.Peca(pos).Cor)
            {
                throw new TabuleiroException("A peça escolhida não é sua!");
            }
            if (!Tab.Peca(pos).ExisteMovimentosPossiveis())
            {
                throw new TabuleiroException("Não existem movimentos possíveis para a peça escolhida!");
            }
        }

        public void ValidaPosicaoDestino(Posicao origem, Posicao destino)
        {
            if (!Tab.Peca(origem).PodeMoverPara(destino))
            {
                throw new TabuleiroException("Posição de destino inválida!");
            }
        }

        private void MudaJogador()
        {
            if(JogadorAtual == Cor.Azul)
            {
                JogadorAtual = Cor.Vermelha;
            } else
            {
                JogadorAtual = Cor.Azul;
            }
        }

        public HashSet<Peca> PecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in capturadas)
            {
                if (x.Cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> PecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.Cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(PecasCapturadas(cor));
            return aux;
        }

        public void ColocarNovaPeca(char coluna, int linha, Peca peca)
        {
            Tab.ColocarPeca(peca, new PosicaoXadrez(coluna, linha).ToPosicao());
            pecas.Add(peca);
        }

        public void ColocarPecas()
        {
            ColocarNovaPeca('c', 1, new Torre(Tab, Cor.Azul));
            ColocarNovaPeca('c', 2, new Torre(Tab, Cor.Azul));
            ColocarNovaPeca('d', 2, new Torre(Tab, Cor.Azul));
            ColocarNovaPeca('e', 2, new Torre(Tab, Cor.Azul));
            ColocarNovaPeca('e', 1, new Torre(Tab, Cor.Azul));
            ColocarNovaPeca('d', 1, new Rei(Tab, Cor.Azul));

            ColocarNovaPeca('c', 8, new Torre(Tab, Cor.Vermelha));
            ColocarNovaPeca('c', 7, new Torre(Tab, Cor.Vermelha));
            ColocarNovaPeca('d', 7, new Torre(Tab, Cor.Vermelha));
            ColocarNovaPeca('e', 7, new Torre(Tab, Cor.Vermelha));
            ColocarNovaPeca('e', 8, new Torre(Tab, Cor.Vermelha));
            ColocarNovaPeca('d', 8, new Rei(Tab, Cor.Vermelha));
        }
    }
}
