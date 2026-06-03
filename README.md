# Trabalho Final - Implementação de Máquinas Abstratas (FTC)

Este repositório foi desenvolvido como parte do Trabalho Final da disciplina de Fundamentos Teóricos da Computação da Faculdade Cotemig. O objetivo do projeto é modelar e implementar de forma estrita três máquinas abstratas fundamentais utilizando a linguagem C# sob o ecossistema .NET 6+.

## 👥 Integrantes da Equipe
Conforme as exigências de identificação do projeto:<br>
**Rafael Soares Almeida Fonseca** — Matrícula: 72400749<br>
**Luiz Felipe de Souza Cassimiro** — Matrícula: 72301139 

## 📝 Breve Descrição de Cada Parte Implementada
O projeto cumpre todas as metas de desenvolvimento divididas nos seguintes diretórios estruturais:

**Parte 1 — Autômato Finito Determinístico (AFD) (`/Parte1/`):** Simulador construído com fidelidade matemática à 5-tupla formal. Ele valida cadeias para a linguagem alvo $L_{1}=\{w\in\{a,b\}^{*} \mid w \text{ termina com ab}\}$ e possui suporte a carregamento e validação dinâmica de outros autômatos via arquivo de configuração `afd.json`.

**Parte 2 — Autômato de Pilha (AP) (`/Parte2/`):** Simulador cujo critério de aceitação é exclusivamente por pilha vazia. É responsável por validar a linguagem livre de contexto $L_{2}=\{a^{n}b^{n} \mid n\ge1\}$ e resolve o desafio de reconhecimento de palíndromos para a linguagem não-determinística $L_{3}=\{w\in\{a,b\}^{*} \mid w=w^{R}, |w|\ge1\}$.

**Parte 3 — Máquina de Turing (MT) (`/Parte3/`):** Simulador de Máquina de Turing configurado com fita dinâmica baseada em dicionário. Ele reconhece a linguagem sensível ao contexto $L_{4}=\{a^{n}b^{n}c^{n} \mid n\ge1\}$ e atua como uma máquina computadora de funções para computar $f(n)=n+1$ em representação unária.

## 🚀 Instruções de Compilação e Execução
Para executar os simuladores, é necessário possuir o SDK do .NET 6 ou superior instalado no ambiente. Navegue até a pasta da máquina abstrata desejada via terminal e utilize o comando padrão do ecossistema .NET:

## Como Executar
 **Executando o AFD**
/ cd Parte1 / 
dotnet run

**Executando o AP**
/ cd Parte2 /
dotnet run

**Executando a MT**
/ cd Parte3 /
dotnet run
