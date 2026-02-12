using System.Diagnostics.CodeAnalysis;

namespace Identity.BR.ValueObjects
{
    /// <summary>
    /// Representacao imutavel e autovalidavel de um CNPJ Alfanumerico.
    /// Implementa validacao Zero-Allocation usando Span e Stackalloc.
    /// </summary>
    public readonly struct CNPJ : IEquatable<CNPJ>, IComparable<CNPJ>, IParsable<CNPJ>
    {
        private const int Length = 14;
        public readonly string _value;

        private CNPJ(string input, bool _)
        {
            _value = input;
        }
        public CNPJ(string input)
        {
            _value = SetValue(input);
        }

        public static implicit operator CNPJ(string input) => new CNPJ(input);
        public static implicit operator string(CNPJ cnpj) => cnpj._value ?? string.Empty;

        public bool IsValid => !string.IsNullOrEmpty(_value);

        /// <summary>
        /// Converte a string em um CNPJ.
        /// </summary>
        /// <param name="input">String a ser convertida</param>
        /// <param name="provider">Nulo</param>
        /// <returns>O resultado da conversao</returns>
        /// <exception cref="ArgumentNullException">Se o input for nulo ou vazio</exception>
        /// <exception cref="ArgumentException">Se o input for um CNPJ invalido</exception>
        public static CNPJ Parse(string input, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input), "Input nao pode ser nulo ou uma string vazia");

            if (TryParse(input, provider, out var result))
                return result;

            throw new ArgumentException("CNPJ invalido");
        }

        /// <summary>
        /// Tenta converter uma string em um CNPJ
        /// </summary>
        /// <param name="input">String a ser convertida</param>
        /// <param name="provider">Nulo</param>
        /// <param name="result">Contem o resultado da conversao em caso de sucesso. Ou nulo em caso de falha</param>
        /// <returns>Verdadeiro se foi feito a conversao; caso contrario falso</returns>
        public static bool TryParse(string? input, IFormatProvider? provider, out CNPJ result)
        {
            Span<char> buffer = stackalloc char[Length];
            int written = Sanitize(input, buffer);

            if (written != Length)
            {
                result = default;
                return false;
            }

            if (IsUniform(buffer))
            {
                result = default;
                return false;
            }

            int dv1 = CalculateCheckDigit(buffer[..12]);
            if (buffer[12] != (char)(dv1 + '0'))
            {
                result = default;
                return false;
            }

            int dv2 = CalculateCheckDigit(buffer[..13]);
            if (buffer[13] != (char)(dv2 + '0'))
            {
                result = default;
                return false;
            }
            result = new CNPJ(new string(buffer), true);
            return true;
        }

        /// <summary>
        /// Verifica se um CNPJ e valido segundo o calculo do DV conforme (Modulo 11, Pesos 2-9, ASCII-48)
        /// </summary>
        /// <param name="input">String contendo o CNPJ com máscara, ou sem máscara</param>
        /// <returns>Verdadeiro se valido; caso contrario falso</returns>
        public static bool IsValidCnpj(string input)
        {
            Span<char> buffer = stackalloc char[Length];
            int written = Sanitize(input, buffer);

            if (written != Length)
                return false;

            if (IsUniform(buffer))
                return false;

            int dv1 = CalculateCheckDigit(buffer[..12]);
            if (buffer[12] != (char)(dv1 + '0'))
                return false;

            int dv2 = CalculateCheckDigit(buffer[..13]);
            if (buffer[13] != (char)(dv2 + '0'))
                return false;

            return true;
        }

        /// <summary>
        /// Retorna o CNPJ formatado no padrao XX.XXX.XXX/XXXX-XX.
        /// </summary>
        public override string ToString()
        {
            if (IsValid)
                return string.Create(18, _value, (span, value) =>
                {
                    var raw = value.AsSpan();
                    raw[..2].CopyTo(span[..2]);
                    span[2] = '.';
                    raw[2..5].CopyTo(span[3..6]);
                    span[6] = '.';
                    raw[5..8].CopyTo(span[7..10]);
                    span[10] = '/';
                    raw[8..12].CopyTo(span[11..15]);
                    span[15] = '-';
                    raw[12..].CopyTo(span[16..]);
                });

            return string.Empty;
        }

        private string SetValue(string input)
        {
            Span<char> buffer = stackalloc char[Length];
            int written = Sanitize(input, buffer);

            if (written != Length)
                throw new ArgumentOutOfRangeException(nameof(input), input, "Tamanho do CNPJ invalido. Tamanho permitido: 14 caracteres sem a mascara e 18 caracteres com mascara");

            if (IsUniform(buffer))
                throw new ArgumentException($"CNPJ nao pode possuir todos os digitos iguais {input}", nameof(input));

            int dv1 = CalculateCheckDigit(buffer[..12]);
            if (buffer[12] != (char)(dv1 + '0'))
                throw new ArgumentException($"Digito verificador invalido", nameof(input));

            int dv2 = CalculateCheckDigit(buffer[..13]);
            if (buffer[13] != (char)(dv2 + '0'))
                throw new ArgumentException($"Digito verificador invalido", nameof(input));

            return new string(buffer);
        }

        /// <summary>
        /// Mantem somente numeros e letras
        /// </summary>
        /// <param name="input">String contendo a mascara</param>
        /// <param name="buffer">Onde o valor sera armazenado apos a limpeza da mascara</param>
        /// <returns>Tamanho do buffer apos a limpeza</returns>
        private static int Sanitize(ReadOnlySpan<char> input, Span<char> buffer)
        {
            int count = 0;
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (count >= Length) return -1;
                    buffer[count++] = char.ToUpperInvariant(c);
                }
            }
            return count;
        }

        /// <summary>
        /// Verifica se todos os caracteres no buffer sao identicos (ex: "00000000000000").
        /// </summary>
        private static bool IsUniform(ReadOnlySpan<char> buffer)
        {
            char first = buffer[0];

            var occurrence = buffer.Count(first);

            return occurrence == Length;
        }

        /// <summary>
        /// Implementa o calculo do DV conforme (Modulo 11, Pesos 2-9, ASCII-48).
        /// </summary>
        private static int CalculateCheckDigit(ReadOnlySpan<char> input)
        {
            int sum = 0;
            int weight = 2;

            // Processa da direita para a esquerda
            for (int i = input.Length - 1; i >= 0; i--)
            {
                // Conforme PDF: "Subtrair 48 do Valor ASCII"
                int val = input[i] - 48;
                sum += val * weight;

                if (++weight > 9) weight = 2;
            }

            int remainder = sum % 11;
            return (remainder < 2) ? 0 : 11 - remainder;
        }

        public bool Equals(CNPJ other) => string.Equals(_value, other._value, StringComparison.Ordinal);

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is CNPJ other && Equals(other);

        public override int GetHashCode() => _value?.GetHashCode(StringComparison.Ordinal) ?? 0;

        public int CompareTo(CNPJ other) => string.Compare(_value, other._value, StringComparison.Ordinal);

        public static bool operator ==(CNPJ left, CNPJ right) => left.Equals(right);

        public static bool operator !=(CNPJ left, CNPJ right) => !left.Equals(right);
    }
}
