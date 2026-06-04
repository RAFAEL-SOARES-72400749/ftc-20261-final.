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

        static void AFD()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("------- Autômato Finito Determinístico (AFD) -------\n");

                // ja começa usando o AFD L1
                InicializarL1();

                // olha se tem o .json se tiver carrega ele
                if (System.IO.File.Exists("afd.json"))
                {
                    CarregarJson("afd.json");
                }

                // imprime tabela, (é isso)
                ExibirDiagrama();

                // Cria o arquivo 'entradas.txt' com as cadeias de teste padrão do PDF caso ele não exista
                string arquivoEntradas = "entradas.txt";
                if (!System.IO.File.Exists(arquivoEntradas))
                {
                    System.IO.File.WriteAllLines(arquivoEntradas, new string[] { "ab", "aab", "bab", "ababab", "ba", "", "b" });
                }

                // lê e processa 'entradas.txt'
                Console.WriteLine("--- Processando Cadeias do Arquivo 'entradas.txt' ---");
                string[] cadeias = System.IO.File.ReadAllLines(arquivoEntradas);

                foreach (string cadeia in cadeias)
                {
                    // validação de símbolos inválidos (Função Total)
                    bool cadeiaValida = true;
                    foreach (char c in cadeia)
                    {
                        if (!Sigma.Contains(c))
                        {
                            cadeiaValida = false;
                            break;
                        }
                    }

                    if (!cadeiaValida && cadeia != "")
                    {
                        Console.WriteLine($"Cadeia: '{cadeia}'");
                        Console.WriteLine("Resultado: REJEITA (Erro: Contém símbolos fora do alfabeto!)");
                        Console.WriteLine(new string('-', 50));
                        continue;
                    }

                    // executa a simulação símbolo a símbolo
                    bool aceita = Aceitar(cadeia);

                    // exibe a cadeia, o rastro percorrido e o resultado
                    Console.WriteLine($"Cadeia: '{cadeia}'");
                    Console.WriteLine($"Rastro: {string.Join(" -> ", rastroAtual)}");
                    Console.WriteLine($"Resultado: {(aceita ? "ACEITA" : "REJEITA")}");
                    Console.WriteLine(new string('-', 50));
                }

                Console.WriteLine("\nProcessamento finalizado. Aperte qualquer tecla para retornar.");
                Console.ReadKey();
                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no AFD: " + ex.Message);
                Console.ReadKey();
                Console.Clear();
            }
        }

        // configura o AFD pra L1
        static void InicializarL1()
        {
            Q = new List<string> { "q0", "q1", "q2" };
            Sigma = new List<char> { 'a', 'b' };
            q0 = "q0";
            F = new List<string> { "q2" };

            delta = new Dictionary<(string, char), string>
            {
                { ("q0", 'a'), "q1" },
                { ("q0", 'b'), "q0" },
                { ("q1", 'a'), "q1" },
                { ("q1", 'b'), "q2" },
                { ("q2", 'a'), "q1" },
                { ("q2", 'b'), "q0" }
            };
        }

        // leitura da cadeia simbolo por simbolo
        static bool Aceitar(string cadeia)
        {
            rastroAtual.Clear();
            string estadoAtual = q0;
            rastroAtual.Add(estadoAtual);

            for (int i = 0; i < cadeia.Length; i++)
            {
                char simbolo = cadeia[i];

                if (delta.TryGetValue((estadoAtual, simbolo), out string proximoEstado))
                {
                    estadoAtual = proximoEstado;
                    rastroAtual.Add(estadoAtual);
                }
                else
                {
                    return false; // Se não achar transição
                }
            }

            // Aceita se o estado final da computação pertence ao conjunto F
            return F.Contains(estadoAtual);
        }

        // imprime a tabela no console (básico por favor)
        static void ExibirDiagrama()
        {
            Console.WriteLine("--- Tabela de Transições do AFD ---");
            Console.Write("Estado\t");
            foreach (char c in Sigma)
            {
                Console.Write($"|Lê '{c}'\t");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 45));

            foreach (string q in Q)
            {
                string indicador = (q == q0 ? "-> " : "") + (F.Contains(q) ? "*" : "");
                Console.Write($"{indicador}{q}\t");

                foreach (char c in Sigma)
                {
                    if (delta.TryGetValue((q, c), out string destino))
                        Console.Write($"|   {destino}\t");
                    else
                        Console.Write("|   -\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine(new string('-', 45) + "\n");
        }

        // pega o JSON e carrega ele no AFD ( aqui eu tive que pedir ajuda, para IA )
        static void CarregarJson(string caminho)
        {
            string json = System.IO.File.ReadAllText(caminho);

            // olha o estado inicial
            Match initMatch = Regex.Match(json, @"""estadoInicial""\s*:\s*""([^""]+)""");
            if (initMatch.Success) q0 = initMatch.Groups[1].Value;

            // organiza os estados
            Match estadosMatch = Regex.Match(json, @"""estados""\s*:\s*\[([^\]]+)\]");
            if (estadosMatch.Success)
            {
                Q.Clear();
                foreach (Match m in Regex.Matches(estadosMatch.Groups[1].Value, @"""([^""]+)"""))
                    Q.Add(m.Groups[1].Value);
            }

            // Mapeia o alfabeto
            Match alfabetoMatch = Regex.Match(json, @"""alfabeto""\s*:\s*\[([^\]]+)\]");
            if (alfabetoMatch.Success)
            {
                Sigma.Clear();
                foreach (Match m in Regex.Matches(alfabetoMatch.Groups[1].Value, @"""([^""]+)"""))
                    if (m.Groups[1].Value.Length > 0) Sigma.Add(m.Groups[1].Value[0]);
            }

            // Mapeia estados de aceitação
            Match aceitacaoMatch = Regex.Match(json, @"""estadosAceitacao""\s*:\s*\[([^\]]+)\]");
            if (aceitacaoMatch.Success)
            {
                F.Clear();
                foreach (Match m in Regex.Matches(aceitacaoMatch.Groups[1].Value, @"""([^""]+)"""))
                    F.Add(m.Groups[1].Value);
            }

            // Mapeia a lista de transições
            Match transicoesMatch = Regex.Match(json, @"""transicoes""\s*:\s*\[(.*)\]", RegexOptions.Singleline);
            if (transicoesMatch.Success)
            {
                delta.Clear();
                MatchCollection matches = Regex.Matches(transicoesMatch.Groups[1].Value, @"\{\s*""origem""\s*:\s*""([^""]+)""\s*,\s*""simbolo""\s*:\s*""([^""]+)""\s*,\s*""destino""\s*:\s*""([^""]+)""\s*\}");
                foreach (Match m in matches)
                {
                    string origem = m.Groups[1].Value;
                    char simbolo = m.Groups[2].Value[0];
                    string destino = m.Groups[3].Value;
                    delta[(origem, simbolo)] = destino;
                }
            }
            Console.WriteLine("-> Configuração carregada com sucesso do arquivo 'afd.json'!\n");
        }

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