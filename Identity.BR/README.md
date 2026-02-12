
![Nuget](https://img.shields.io/nuget/v/Identity.BR)
![NetVersion](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-purple)

**Identity.BR** é uma biblioteca de **Value Objects** ultra-performática para manipulação e validação de documentos brasileiros (**CPF** e **CNPJ Alfanumérico**).

Desenvolvida utilizando as últimas inovações do C# 13 e .NET 8+, ela foca em reduzir a pressão no Garbage Collector (GC) utilizando `readonly struct`, `Span<T>` e `stackalloc`.

## 🚀 Principais Vantagens

- **Zero-Allocation Validation**: A validação (`IsValidCpf`/`IsValidCnpj`) não aloca memória na Heap. Tudo é feito na Stack.
- **Imutabilidade**: Implementado como `readonly struct`, garantindo thread-safety e previsibilidade.
- **Performance Extrema**: Uso de `string.Create` para formatação e aritmética de ponteiros/spans para cálculos de dígitos verificadores.
- **Alfanumérico**: Já implementa a nova regulamentação de CNPJ's alfanuméricos.
- **Integração Nativa**: Implementa interfaces modernas como `IParsable<T>`, `IEquatable<T>` e `IComparable<T>`.
- **Developer Experience**: Conversores implícitos permitem usar strings diretamente, mantendo a segurança de tipos.

---

## 📦 Instalação

Via .NET CLI:

```bash
dotnet add package Identity.BR
```

---

## 💻 Como Utilizar

### 1. Instanciação e Conversão Implícita

Você pode criar um CPF ou CNPJ diretamente de uma string. A biblioteca cuida da limpeza de caracteres especiais (pontos, traços) automaticamente.

```csharp
using ValueObjects;

// Conversão implícita (Maneira mais limpa)
// Lança exceção se inválido
CPF cpf = "123.456.789-00"; 
CNPJ cnpj = "R5.I0S.OGT/0001-30";

// Via Construtor
// Lança exceção se inválido
var cpf2 = new CPF("12345678900");
var cnpj2 = new CNPJ("12345678000190");

// O valor interno é armazenado sem máscara (limpo)
Console.WriteLine((string)cpf); // Saída: "12345678900"
Console.WriteLine((string)cnpj); // Saída: "R5I0SOGT000130"
```

### 2. Validação de Alta Performance (Zero-Allocation)

Se você precisa apenas verificar se uma string é válida sem criar o objeto (para não alocar memória desnecessária em loops ou validações de API), use os métodos estáticos.

```csharp
string input = "123.456.789-00";

// Extremamente rápido e sem alocação de memória
if (CPF.IsValidCpf(input))
{
    Console.WriteLine("CPF Válido!");
}

if (CNPJ.IsValidCnpj("R5.I0S.OGT/0001-30"))
{
    Console.WriteLine("CNPJ Válido!");
}
```

### 3. Parse e TryParse (Padrão .NET)

Ideal para validação de entrada de dados onde você precisa do objeto caso seja válido.

```csharp
// Parse (Lança exceção se inválido)
try 
{
    var documento = CPF.Parse("123.456.789-00");
}
catch (ArgumentException ex)
{
    Console.WriteLine("Documento inválido: " + ex.Message);
}

// Parse (Lança exceção se inválido)
try
{
    var documento = CNPJ.Parse("12.345.678/0001-90");
}
catch (ArgumentException ex)
{
    Console.WriteLine("Documento inválido: " + ex.Message);
}

// TryParse (Seguro e performático)
if (CPF.TryParse("123.456.789-00", null, out CPF resultado))
{
    // 'resultado' é um CPF válido
    Console.WriteLine($"CPF: {resultado}");
}
else
{
    Console.WriteLine("CPF inválido fornecido.");
}

// TryParse (Seguro e performático)
if (CNPJ.TryParse("12.345.678/0001-90", null, out CNPJ resultado))
{
    // 'resultado' é uma struct CNPJ válida
    Console.WriteLine($"Empresa: {resultado}");
}
else
{
    Console.WriteLine("CNPJ inválido fornecido.");
}
```

### 4. Formatação

O método `ToString()` retorna o documento formatado com a máscara padrão. A implementação usa `string.Create` para máxima eficiência.

```csharp
CPF cpf = "11122233344";
Console.WriteLine(cpf.ToString()); 
// Saída: 111.222.333-44

CNPJ cnpj = "11222333000144";
Console.WriteLine(cnpj.ToString()); 
// Saída: 11.222.333/0001-44
```

### 5. Comparação e Igualdade

Como implementam `IEquatable`, as comparações são diretas e otimizadas.

```csharp
CPF c1 = "123.456.789-00";
CPF c2 = "12345678900"; // Sem máscara

if (c1 == c2) // True
{
    Console.WriteLine("São o mesmo documento.");
}

CNPJ d1 = "12.345.678/0001-90";
CNPJ d2 = "12345678000190"; // Sem máscara

if (d1 == d2) // True
{
    Console.WriteLine("São a mesma empresa.");
}

// Útil para Dictionaries ou HashSets
var listaNegra = new HashSet<CPF>();
listaNegra.Add(c1);
```

---

## ⚡ Benchmarks

Os testes foram realizados utilizando o **BenchmarkDotNet v0.15.8** no seguinte ambiente:

- **OS:** Windows 11 (10.0.26100.7840/24H2)
- **CPU:** AMD Ryzen 5 7535HS with Radeon Graphics, 3.30GHz (6 núcleos físicos, 12 lógicos)
- **Runtime:** .NET 8.0.3 (X64 RyuJIT x86-64-v3)

### CNPJ

| Method | Mean | Ratio | RatioSD | Rank | Gen0 | Allocated |
|:--- |---:|---:|---:|---:|---:|---:|
| CNPJ_Equals_With_Other | 3.186 ns | 0.09 | 0.00 | 1 | - | - |
| ToString_Formatted | 20.937 ns | 0.60 | 0.01 | 2 | 0.0076 | 64 B |
| IsValidCnpj_Invalid_Uniform | 21.648 ns | 0.62 | 0.01 | 2 | - | - |
| IsValidCnpj_Raw_Valid | 34.760 ns | 1.00 | 0.01 | 3 | - | - |
| IsValidCnpj_Masked_Valid | 37.576 ns | 1.08 | 0.01 | 4 | - | - |
| Create_CNPJ_With_TryParse | 46.222 ns | 1.33 | 0.03 | 5 | 0.0067 | 56 B |
| Create_CNPJ_With_Parse | 47.094 ns | 1.35 | 0.04 | 5 | 0.0067 | 56 B |
| Create_CNPJ_Without_Parse | 49.875 ns | 1.43 | 0.02 | 5 | 0.0067 | 56 B |

### CPF

| Method | Mean | Ratio | RatioSD | Rank | Gen0 | Allocated |
|:--- |---:|---:|---:|---:|---:|---:|
| CPF_Equals_With_Other | 2.952 ns | 0.13 | 0.00 | 1 | - | - |
| ToString_Formatted | 12.663 ns | 0.54 | 0.02 | 2 | 0.0067 | 56 B |
| IsValidCpf_Invalid_Uniform | 14.478 ns | 0.62 | 0.02 | 3 | - | - |
| IsValidCpf_Raw_Valid | 23.421 ns | 1.00 | 0.03 | 4 | - | - |
| IsValidCpf_Masked_Valid | 24.613 ns | 1.05 | 0.03 | 4 | - | - |
| Create_CPF_With_TryParse | 31.719 ns | 1.35 | 0.04 | 5 | 0.0057 | 48 B |
| Create_CPF_With_Parse | 32.372 ns | 1.38 | 0.04 | 5 | 0.0057 | 48 B |
| Create_CPF_Without_Parse | 33.588 ns | 1.43 | 0.03 | 5 | 0.0057 | 48 B |

---

## 🛠 Detalhes Técnicos

- **Stackalloc**: Utilizamos alocação na stack para buffers de caracteres durante a validação e cálculo de dígitos verificadores, evitando pressão no GC.
- **Sanitização**: A limpeza de caracteres não numéricos é feita "on-the-fly" durante a iteração do Span, sem criar strings intermediárias.
- **CNPJ Alfanumérico**: A estrutura `CNPJ` suporta letras, convertendo-as para maiúsculas automaticamente, preparando a lib para futuros padrões ou usos específicos.

## 📄 Licença

Este projeto está licenciado sob a licença MIT.