using System.Text.RegularExpressions;

namespace ImplementacaoAutomatos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            while (true)
            { //menu para ficar repetindo ate o usuario sair (escolha 3)
                Console.WriteLine("------- Implementação de AFD, AP e MT -------\n");
                Console.WriteLine("Selecione o tipo do Autômato");
                Console.WriteLine("1 - Autômato Finito Determinístico (AFD)");
                Console.WriteLine("2 - Autômato de Pilha Determinístico (APD)");
                Console.WriteLine("3 - Máquina de Turing (MT)");
                Console.WriteLine("4 - Sair\n");
                Console.WriteLine("-- Rafael Soares Almeida Fonseca --");
                Console.WriteLine("-- Luiz Felipe de Souza Cassimiro --");
                if (!int.TryParse(Console.ReadLine(), out int escolha))
                    escolha = 0;

                switch (escolha)
                {
                    case 1:
                        Console.WriteLine();
                        AFD(); //executando o AFD
                        break;

                    case 2:
                        Console.WriteLine();
                        APD(); //executando o APD
                        break;

                    case 3:
                        Console.WriteLine();
                        MT(); //executando a MT
                        break;

                    case 4:
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
    }
}