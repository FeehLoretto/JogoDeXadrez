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
        public bool Xeque { get; private set; }
        public Peca VulneravelEnPassant { get; private set; }

        public PartidaDeXadrez()
        {
            Tab = new Tabuleiro(8, 8);
            Turno = 1;
            JogadorAtual = Cor.Azul;
            Terminada = false;
            Xeque = false;
            VulneravelEnPassant = null;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            ColocarPecas();
        }

        public Peca ExecutaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = Tab.RetirarPeca(origem);
            p.IncrementarQntMovimentos();
            Peca capturada = Tab.RetirarPeca(destino);
            Tab.ColocarPeca(p, destino);
            if (capturada != null)
            {
                capturadas.Add(capturada);
            }

            // #JogadaEspecial Roque Pequeno
            if (p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = Tab.RetirarPeca(origemT);
                T.IncrementarQntMovimentos();
                Tab.ColocarPeca(T, destinoT);
            }

            // #JogadaEspecial Roque Grande
            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = Tab.RetirarPeca(origemT);
                T.IncrementarQntMovimentos();
                Tab.ColocarPeca(T, destinoT);
            }

            // #JogadaEspecial En Passant
            if (p is Peao)
            {
                if (origem.Coluna != destino.Coluna && capturada == null)
                {
                    Posicao posP;
                    if (p.Cor == Cor.Azul)
                    {
                        posP = new Posicao(destino.Linha + 1, destino.Coluna);
                    } else
                    {
                        posP = new Posicao(destino.Linha - 1, destino.Coluna);
                    }
                    capturada = Tab.RetirarPeca(posP);
                    capturadas.Add(capturada);
                }
            }

            return capturada;
        }

        public void DesfazMovimento(Posicao origem, Posicao destino, Peca capturada)
        {
            Peca p = Tab.RetirarPeca(destino);
            p.DecrementarQntMovimentos();
            if (capturada != null)
            {
                Tab.ColocarPeca(capturada, destino);
                capturadas.Remove(capturada);
            }
            Tab.ColocarPeca(p, origem);

            // #JogadaEspecial Roque Pequeno
            if (p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = Tab.RetirarPeca(destinoT);
                T.IncrementarQntMovimentos();
                Tab.ColocarPeca(T, origemT);
            }

            // #JogadaEspecial Roque Grande
            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = Tab.RetirarPeca(destinoT);
                T.IncrementarQntMovimentos();
                Tab.ColocarPeca(T, origemT);
            }

            // #JogadaEspecial En Passant
            if (p is Peao)
            {
                if (origem.Coluna != destino.Coluna && capturada == VulneravelEnPassant)
                {
                    Peca peao = Tab.RetirarPeca(destino);
                    Posicao posP;
                    if (p.Cor == Cor.Azul)
                    {
                        posP = new Posicao(3, destino.Coluna);
                    }
                    else
                    {
                        posP = new Posicao(4, destino.Coluna);
                    }
                    Tab.ColocarPeca(peao, posP);
                    capturadas.Remove(capturada);
                }
            }
        }

        public void RealizaJogada(Posicao origem, Posicao destino)
        {
            Peca capturada = ExecutaMovimento(origem, destino);

            if (EstaEmXeque(JogadorAtual))
            {
                DesfazMovimento(origem, destino, capturada);
                throw new TabuleiroException("Você não pode se colocar em Xeque!");
            }
            if (EstaEmXeque(Adversaria(JogadorAtual)))
            {
                Xeque = true;
            } else
            {
                Xeque = false;
            }

            if (TesteXequeMate(Adversaria(JogadorAtual)))
            {
                Terminada = true;
            } else
            {
                Turno++;
                MudaJogador();
            }

            Peca p = Tab.Peca(destino);

            // #JogadaEspecial En Passant
            if (p is Peao && (destino.Linha == origem.Linha - 2 || destino.Linha == origem.Linha + 2))
            {
                VulneravelEnPassant = p;
            }
            else
            {
                VulneravelEnPassant = null;
            }
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
            if (!Tab.Peca(origem).MovimentoPossivel(destino))
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

        private Cor Adversaria(Cor cor)
        {
            if (cor == Cor.Azul)
            {
                return Cor.Vermelha;
            }
            return Cor.Azul;
        }

        private Peca Rei(Cor cor)
        {
            foreach (Peca x in PecasEmJogo(cor))
            {
                if (x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        public bool EstaEmXeque(Cor cor)
        {
            Peca R = Rei(cor);
            if (R == null)
            {
                throw new TabuleiroException("Não tem rei da cor" + cor + " no tabuleiro!");
            }
            foreach(Peca x in PecasEmJogo(Adversaria(cor)))
            {
                bool[,] mat = x.MovimentosPossiveis();
                if (mat[R.Posicao.Linha, R.Posicao.Coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool TesteXequeMate(Cor cor)
        {
            if (!EstaEmXeque(cor))
            {
                return false;
            }

            foreach (Peca x in PecasEmJogo(cor))
            {
                bool[,] mat = x.MovimentosPossiveis();
                for (int i = 0; i < Tab.Linhas; i++)
                {
                    for (int j = 0; j < Tab.Colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.Posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca capturada = ExecutaMovimento(origem, destino);
                            bool testeXeque = EstaEmXeque(cor);
                            DesfazMovimento(origem, destino, capturada);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void ColocarNovaPeca(char coluna, int linha, Peca peca)
        {
            Tab.ColocarPeca(peca, new PosicaoXadrez(coluna, linha).ToPosicao());
            pecas.Add(peca);
        }

        public void ColocarPecas()
        {
            ColocarNovaPeca('a', 1, new Torre(Tab, Cor.Azul));
            ColocarNovaPeca('b', 1, new Cavalo(Tab, Cor.Azul));
            ColocarNovaPeca('c', 1, new Bispo(Tab, Cor.Azul));
            ColocarNovaPeca('d', 1, new Dama(Tab, Cor.Azul));
            ColocarNovaPeca('e', 1, new Rei(Tab, Cor.Azul, this));
            ColocarNovaPeca('f', 1, new Bispo(Tab, Cor.Azul));
            ColocarNovaPeca('g', 1, new Cavalo(Tab, Cor.Azul));
            ColocarNovaPeca('h', 1, new Torre(Tab, Cor.Azul));
            ColocarNovaPeca('a', 2, new Peao(Tab, Cor.Azul, this));
            ColocarNovaPeca('b', 2, new Peao(Tab, Cor.Azul, this));
            ColocarNovaPeca('c', 2, new Peao(Tab, Cor.Azul, this));
            ColocarNovaPeca('d', 2, new Peao(Tab, Cor.Azul, this));
            ColocarNovaPeca('e', 2, new Peao(Tab, Cor.Azul, this));
            ColocarNovaPeca('f', 2, new Peao(Tab, Cor.Azul, this));
            ColocarNovaPeca('g', 2, new Peao(Tab, Cor.Azul, this));
            ColocarNovaPeca('h', 2, new Peao(Tab, Cor.Azul, this));

            ColocarNovaPeca('a', 8, new Torre(Tab, Cor.Vermelha));
            ColocarNovaPeca('b', 8, new Cavalo(Tab, Cor.Vermelha));
            ColocarNovaPeca('c', 8, new Bispo(Tab, Cor.Vermelha));
            ColocarNovaPeca('d', 8, new Dama(Tab, Cor.Vermelha));
            ColocarNovaPeca('e', 8, new Rei(Tab, Cor.Vermelha, this));
            ColocarNovaPeca('f', 8, new Bispo(Tab, Cor.Vermelha));
            ColocarNovaPeca('g', 8, new Cavalo(Tab, Cor.Vermelha));
            ColocarNovaPeca('h', 8, new Torre(Tab, Cor.Vermelha));
            ColocarNovaPeca('a', 7, new Peao(Tab, Cor.Vermelha, this));
            ColocarNovaPeca('b', 7, new Peao(Tab, Cor.Vermelha, this));
            ColocarNovaPeca('c', 7, new Peao(Tab, Cor.Vermelha, this));
            ColocarNovaPeca('d', 7, new Peao(Tab, Cor.Vermelha, this));
            ColocarNovaPeca('e', 7, new Peao(Tab, Cor.Vermelha, this));
            ColocarNovaPeca('f', 7, new Peao(Tab, Cor.Vermelha, this));
            ColocarNovaPeca('g', 7, new Peao(Tab, Cor.Vermelha, this));
            ColocarNovaPeca('h', 7, new Peao(Tab, Cor.Vermelha, this));
        }
    }
}
