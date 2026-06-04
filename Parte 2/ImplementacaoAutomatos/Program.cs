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
                Console.WriteLine("------- Implementação de AP -------\n");
                Console.WriteLine("Selecione o tipo do Autômato");
                Console.WriteLine("1 - Autômato de Pilha Determinístico (APD)");
                Console.WriteLine("2 - Sair\n");
                Console.WriteLine("-- Rafael Soares Almeida Fonseca --");
                Console.WriteLine("-- Luiz Felipe de Souza Cassimiro --");
                if (!int.TryParse(Console.ReadLine(), out int escolha))
                    escolha = 0;

                switch (escolha)
                {
                    case 1:
                        Console.WriteLine();
                        APD(); //executando o AP
                        break;

                    case 2:
                        return;

                    default:
                        Console.WriteLine("\nOpção inválida!\n");
                        break;
                }
            }
        }

        static List<string> Q = new List<string>(); // conjunto de estados
        static List<char> Sigma = new List<char>(); // alfabeto
        static Dictionary<(string estado, char simbolo), string> delta = new Dictionary<(string, char), string>();
        static string q0; // estado inicial
        static List<string> F = new List<string>(); // estados de aceitação

        // lista onde vai armazenar o rastro de estados visitados na última execução
        static List<string> rastroAtual = new List<string>();

        static void APD()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("------- Autômato de Pilha (AP) -------\n");
                Console.WriteLine("Escolha qual linguagem deseja simular:");
                Console.WriteLine("1 - L2 = {a^n b^n | n >= 1} (AP Determinístico)");
                Console.WriteLine("2 - L3 = Palíndromos (AP Não-Determinístico)");

                if (!int.TryParse(Console.ReadLine(), out int opcaoAP)) opcaoAP = 1;

                Console.Clear();
                string arquivoEntradas;

                if (opcaoAP == 1)
                {
                    ExibirInformacoesAP_L2();
                    arquivoEntradas = "entradas_ap_l2.txt";
                    if (!System.IO.File.Exists(arquivoEntradas))
                    {
                        System.IO.File.WriteAllLines(arquivoEntradas, new string[] { "ab", "aabb", "aaabbb", "aab", "abb", "ba", "", "abab" });
                    }
                }
                else
                {
                    ExibirInformacoesAP_L3();
                    arquivoEntradas = "entradas_ap_l3.txt";
                    if (!System.IO.File.Exists(arquivoEntradas))
                    {
                        // Casos de teste obrigatórios para palíndromos
                        System.IO.File.WriteAllLines(arquivoEntradas, new string[] { "a", "aba", "abba", "ab", "aab", "" });
                    }
                }

                Console.WriteLine($"--- Processando Cadeias do Arquivo '{arquivoEntradas}' ---");
                string[] cadeias = System.IO.File.ReadAllLines(arquivoEntradas);

                foreach (string cadeia in cadeias)
                {
                    if (!Regex.IsMatch(cadeia, @"^[ab]*$") && cadeia != "")
                    {
                        Console.WriteLine($"Cadeia: '{cadeia}'\nResultado: REJEITA (Fora do alfabeto)\n" + new string('-', 50));
                        continue;
                    }

                    // manda para a função correta baseada na escolha
                    bool aceita = (opcaoAP == 1) ? SimularAP_L2(cadeia) : SimularAP_L3(cadeia);

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
                Console.WriteLine("Erro no AP: " + ex.Message);
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void ExibirInformacoesAP_L2()
        {
            Console.WriteLine("Definindo o Autômato para a linguagem L2 = {a^n b^n | n >= 1}:");
            Console.WriteLine("Estados: {q0, q1, q2} | Aceitação: Exclusivamente por Pilha Vazia ");
            Console.WriteLine("Alfabeto: {a, b} | Alfabeto da Pilha: {A, Z}\n");
        }

        static bool SimularAP_L2(string cadeia)
        {
            // usando Stack<char> para simular a pilha
            Stack<char> pilha = new Stack<char>();
            pilha.Push('Z'); // 'Z' é o nosso Z0 (símbolo inicial da pilha)

            string estadoAtual = "q0";
            string palavraRestante = cadeia;

            // Se a cadeia for vazia logo de cara, a linguagem exige n >= 1, então já rejeita direto 
            if (cadeia == "") return false;

            Console.WriteLine($"\nPassos de computação para a cadeia: '{cadeia}'");
            Console.WriteLine($"  [{estadoAtual}, {palavraRestante}, {ImprimirPilha(pilha)}]");

            // cadeia + 1 para forçar a leitura do 'λ' (movimento vazio) no final
            for (int i = 0; i <= cadeia.Length; i++)
            {
                // Se já lemos toda a palavra, o próximo símbolo simulado é o vazio 'λ'
                char simboloLido = (i < cadeia.Length) ? cadeia[i] : 'λ';

                if (simboloLido != 'λ')
                    palavraRestante = palavraRestante.Remove(0, 1);

                string proximoEstado = FuncaoTransicaoL2(estadoAtual, simboloLido, pilha);

                if (proximoEstado == "erro")
                {
                    Console.WriteLine($"\nSem transição definida para δ({estadoAtual}, {simboloLido}, {pilha.Peek()})");
                    return false; 
                }

                estadoAtual = proximoEstado;

                // escreve a configuração instantânea
                Console.WriteLine($"⊢ [{estadoAtual}, {(palavraRestante == "" ? "λ" : palavraRestante)}, {ImprimirPilha(pilha)}]");

                if (pilha.Count == 0)
                    break;
            }

            return pilha.Count == 0;
        }

        static string FuncaoTransicaoL2(string estadoAnterior, char simboloLido, Stack<char> pilha)
        {
            if (pilha.Count == 0) return "erro";
            char topo = pilha.Peek(); // Olha o topo

            switch (estadoAnterior)
            {
                case "q0":
                    if (simboloLido == 'a' && topo == 'Z')
                    {
                        // q0 lê 'a', topo 'Z' -> empilha 'A' (pilha fica AZ)
                        pilha.Push('A');
                        return "q0";
                    }
                    if (simboloLido == 'a' && topo == 'A')
                    {
                        // q0 lê 'a', topo 'A' -> empilha 'A' (pilha fica AA)
                        pilha.Push('A');
                        return "q0";
                    }
                    if (simboloLido == 'b' && topo == 'A')
                    {
                        // q0 lê 'b', topo 'A' -> desempilha 'A', muda pro estado q1
                        pilha.Pop();
                        return "q1";
                    }
                    break;

                case "q1":
                    if (simboloLido == 'b' && topo == 'A')
                    {
                        // q1 lê 'b', topo 'A' -> desempilha 'A', continua em q1
                        pilha.Pop();
                        return "q1";
                    }
                    if (simboloLido == 'λ' && topo == 'Z')
                    {
                        // q1 lê vazio 'λ', topo 'Z' -> desempilha 'Z' pra esvaziar a pilha (aceitação!)
                        pilha.Pop();
                        return "q2"; 
                    }
                    break;
            }

            return "erro";
        }

        static void ExibirInformacoesAP_L3()
        {
            Console.WriteLine("Definindo o Autômato para a linguagem L3 (Palíndromos):");
            Console.WriteLine("Estados: {q0, q1, q2} | Aceitação: Exclusivamente por Pilha Vazia ");
            Console.WriteLine("Alfabeto: {a, b} | Alfabeto da Pilha: {A, B, Z}\n");
            Console.WriteLine("Nota: Por ser Não-Determinístico, o console exibirá apenas o rastro do caminho que deu certo.\n");
        }

        static bool SimularAP_L3(string cadeia)
        {
            if (cadeia == "") return false;

            Stack<char> pilhaInicial = new Stack<char>();
            pilhaInicial.Push('Z'); 

            // guardar o histórico do caminho correto 
            List<string> rastroSucesso = new List<string>();

            Console.WriteLine($"\nBuscando caminho de aceitação para: '{cadeia}'");

            bool aceita = ExplorarCaminhosL3(cadeia, 0, "q0", pilhaInicial, rastroSucesso);

            if (aceita)
            {
                foreach (string passo in rastroSucesso)
                    Console.WriteLine(passo);
                return true;
            }
            else
            {
                Console.WriteLine("  Nenhum caminho válido esvaziou a pilha.");
                return false;
            }
        }

        // simula os múltiplos caminhos do Não-Determinismo
        static bool ExplorarCaminhosL3(string cadeia, int indiceAtual, string estadoAtual, Stack<char> pilhaAtual, List<string> rastroAtual)
        {
            string palavraRestante = indiceAtual < cadeia.Length ? cadeia.Substring(indiceAtual) : "λ";
            rastroAtual.Add($"⊢ [{estadoAtual}, {palavraRestante}, {ImprimirPilha(pilhaAtual)}]");

            // CONDIÇÃO DE ACEITAÇÃO: Pilha Vazia (Independe de estado final) 
            if (pilhaAtual.Count == 0) return true;

            char topo = pilhaAtual.Peek();
            char simboloLido = indiceAtual < cadeia.Length ? cadeia[indiceAtual] : 'λ';

            if (estadoAtual == "q0")
            {
                if (simboloLido != 'λ')
                {
                    Stack<char> novaPilha = CloneStack(pilhaAtual);
                    novaPilha.Push(simboloLido == 'a' ? 'A' : 'B');
                    if (ExplorarCaminhosL3(cadeia, indiceAtual + 1, "q0", novaPilha, rastroAtual)) return true;
                }

                if (simboloLido != 'λ')
                {
                    Stack<char> novaPilha = CloneStack(pilhaAtual);
                    if (ExplorarCaminhosL3(cadeia, indiceAtual + 1, "q1", novaPilha, rastroAtual)) return true;
                }

                Stack<char> novaPilhaVazia = CloneStack(pilhaAtual);
                if (ExplorarCaminhosL3(cadeia, indiceAtual, "q1", novaPilhaVazia, rastroAtual)) return true;
            }
            else if (estadoAtual == "q1")
            {
                if (simboloLido == 'a' && topo == 'A')
                {
                    Stack<char> novaPilha = CloneStack(pilhaAtual);
                    novaPilha.Pop();
                    if (ExplorarCaminhosL3(cadeia, indiceAtual + 1, "q1", novaPilha, rastroAtual)) return true;
                }
                else if (simboloLido == 'b' && topo == 'B')
                {
                    Stack<char> novaPilha = CloneStack(pilhaAtual);
                    novaPilha.Pop();
                    if (ExplorarCaminhosL3(cadeia, indiceAtual + 1, "q1", novaPilha, rastroAtual)) return true;
                }

                if (simboloLido == 'λ' && topo == 'Z')
                {
                    Stack<char> novaPilha = CloneStack(pilhaAtual);
                    novaPilha.Pop();
                    if (ExplorarCaminhosL3(cadeia, indiceAtual, "q2", novaPilha, rastroAtual)) return true;
                }
            }

            // Se chegou até aqui, nenhum caminho deu certo. 
            rastroAtual.RemoveAt(rastroAtual.Count - 1);
            return false;
        }

        // Método auxiliar essencial para o Não-Determinismo: 
        // Cria uma cópia exata da Stack para que uma tentativa errada não suje a pilha da outra tentativa.
        static Stack<char> CloneStack(Stack<char> original)
        {
            var array = original.ToArray();
            Array.Reverse(array);
            return new Stack<char>(array);
        }

        static string ImprimirPilha(Stack<char> pilha)
        {
            if (pilha.Count == 0) return "λ";

            // A classe Stack<> no C# enumera automaticamente do topo para a base
            return string.Join("", pilha);
        }

    }
}