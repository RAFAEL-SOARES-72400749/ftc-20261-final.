using System.Text.RegularExpressions;

namespace ImplementacaoAutomatos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            while (true)
            { 
                Console.WriteLine("------- Implementação de MT -------\n");
                Console.WriteLine("Selecione o tipo do Autômato");
                Console.WriteLine("1 - Máquina de Turing (MT)");
                Console.WriteLine("2 - Sair\n");
                Console.WriteLine("-- Rafael Soares Almeida Fonseca --");
                Console.WriteLine("-- Luiz Felipe de Souza Cassimiro --");
                if (!int.TryParse(Console.ReadLine(), out int escolha))
                    escolha = 0;

                switch (escolha)
                {
                    case 1:
                        Console.WriteLine();
                        MT(); //executando a MT
                        break;

                    case 2:
                        return;

                    default:
                        Console.WriteLine("\nOpção inválida!\n");
                        break;
                }
            }
        }

        static void MT()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("------- Máquina de Turing (MT) -------\n");
                Console.WriteLine("Definindo a MT para a linguagem L = {a^n b^n c^n | n >= 1}:\n");

                // cria o arquivo 'entradas_mt.txt' com os casos de teste  
                string arquivoEntradasMT = "entradas_mt.txt";
                if (!System.IO.File.Exists(arquivoEntradasMT))
                {
                    System.IO.File.WriteAllLines(arquivoEntradasMT, new string[] { "abc", "aabbcc", "abbc", "aabbc", "", "aaabbbccc" });
                }

                // carrega o dicionário das transições lá da MT
                var transicoes = InicializarTransicoesMT();

                Console.WriteLine("--- Processando Cadeias do Arquivo 'entradas_mt.txt' ---");
                string[] cadeias = System.IO.File.ReadAllLines(arquivoEntradasMT);

                foreach (string cadeia in cadeias)
                {
                    bool aceita = SimularMT(cadeia, transicoes);

                    Console.WriteLine($"Cadeia: '{cadeia}'");
                    Console.WriteLine($"Resultado: {(aceita ? "ACEITA" : "REJEITA")}");
                    Console.WriteLine(new string('-', 50));
                }

                Console.WriteLine("\nProcessamento finalizado. Aperte qualquer tecla para retornar.");
                Console.ReadKey();
                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro na MT: " + ex.Message);
                Console.ReadKey();
                Console.Clear();
            }
        }

        // configura as regrinhas de transição da MT 
        static Dictionary<(string estado, char simbolo), (string novoEstado, char novoSimbolo, char direcao)> InicializarTransicoesMT()
        {
            var t = new Dictionary<(string, char), (string, char, char)>();

            // Símbolo '_' representa o espaço em branco (Blank)
            // Estado de aceitação final será o 'qf'

            // q0: procura pelo primeiro 'a' ---
            t.Add(("q0", 'a'), ("q1", 'X', 'R')); 
            t.Add(("q0", 'Y'), ("q4", 'Y', 'R')); 

            // q1: passa reto por 'a' e 'Y' procurando pelo primeiro 'b'
            t.Add(("q1", 'a'), ("q1", 'a', 'R'));
            t.Add(("q1", 'Y'), ("q1", 'Y', 'R'));
            t.Add(("q1", 'b'), ("q2", 'Y', 'R')); 

            // q2: ignora 'b' e 'Z' procurando pelo primeiro 'c' 
            t.Add(("q2", 'b'), ("q2", 'b', 'R'));
            t.Add(("q2", 'Z'), ("q2", 'Z', 'R'));
            t.Add(("q2", 'c'), ("q3", 'Z', 'L')); 

            // q3: volta tudo para a esquerda procurando o último 'X' marcado 
            t.Add(("q3", 'b'), ("q3", 'b', 'L'));
            t.Add(("q3", 'Z'), ("q3", 'Z', 'L'));
            t.Add(("q3", 'a'), ("q3", 'a', 'L'));
            t.Add(("q3", 'Y'), ("q3", 'Y', 'L'));
            t.Add(("q3", 'X'), ("q0", 'X', 'R'));

            // q4: validação final (vê se restou algum 'b' ou 'c' perdido) 
            t.Add(("q4", 'Y'), ("q4", 'Y', 'R'));
            t.Add(("q4", 'Z'), ("q4", 'Z', 'R'));
            t.Add(("q4", '_'), ("qf", '_', 'R'));

            return t;
        }

        // faz a simulação da máquina de turing ( essa aqui eu tive que pedidar ajuda pra IA também, porque é meio complexo )
        static bool SimularMT(string cadeia, Dictionary<(string, char), (string, char, char)> transicoes)
        {
            // fita dinâmica usando Dictionary
            Dictionary<int, char> fita = new Dictionary<int, char>();
            for (int i = 0; i < cadeia.Length; i++)
            {
                fita[i] = cadeia[i];
            }

            string estadoAtual = "q0";
            int cabeca = 0; // posição inicial
            int passos = 0;
            int limitePassos = 1000; // trava de segurança contra loop infinito

            Console.WriteLine($"\nPassos de computação para: '{cadeia}'");
            Console.WriteLine($"  {estadoAtual} | Fita: {ExibirFita(fita, cabeca)}");

            // executa até chegar no estado final 'qf' ou estourar os passos
            while (estadoAtual != "qf" && passos < limitePassos)
            {
                passos++;

                // olha o que tá na fita. Se a chave não existir, o símbolo é branco '_'
                char simboloAtual = fita.ContainsKey(cabeca) ? fita[cabeca] : '_';

                // tenta achar a regra cadastrada no dicionário
                if (transicoes.TryGetValue((estadoAtual, simboloAtual), out var regra))
                {
                    string proximoEstado = regra.Item1;
                    char novoSimbolo = regra.Item2;
                    char direcao = regra.Item3;

                    fita[cabeca] = novoSimbolo; 

                    // move o cabeçote pra esquerda ou direita R = Direita / L = Esquerda
                    if (direcao == 'R') cabeca++;
                    if (direcao == 'L') cabeca--;

                    estadoAtual = proximoEstado;

                    // escreve no console o estado atual e como tá a fita
                    Console.WriteLine($"⊢ {estadoAtual} | Fita: {ExibirFita(fita, cabeca)}");
                }
                else
                {
                    // se não achou transição, a máquina para e rejeita
                    Console.WriteLine($"\n[Parada] Sem transição para o estado {estadoAtual} lendo '{simboloAtual}'");
                    return false;
                }
            }

            if (passos >= limitePassos)
            {
                Console.WriteLine("\n[Erro] Loop infinito detectado! A máquina estourou o limite de passos.");
                return false;
            }

            return estadoAtual == "qf";
        }

        // renderiza a fita colocando colchetes [] onde o cabeçote está apontando
        static string ExibirFita(Dictionary<int, char> fita, int cabeca)
        {
            int min = 0;
            int max = 0;

            // acha os extremos atuais da fita para desenhar na tela
            foreach (int k in fita.Keys)
            {
                if (k < min) min = k;
                if (k > max) max = k;
            }
            if (cabeca < min) min = cabeca;
            if (cabeca > max) max = cabeca;

            // margenzinha de sobra visual nas pontas
            min -= 1;
            max += 1;

            string res = "";
            for (int i = min; i <= max; i++)
            {
                char c = fita.ContainsKey(i) ? fita[i] : '_';
                if (i == cabeca)
                    res += $"[{c}]";
                else
                    res += c;
            }
            return res;
        }
    }
}