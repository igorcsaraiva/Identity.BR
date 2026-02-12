using System.Diagnostics.CodeAnalysis;

namespace Identity.BR.ValueObjects
{
    /// <summary>
    /// Representacao imutavel e de alta performance de um CPF.
    /// Implementa validacao Zero-Allocation usando Span e Stackalloc.
    /// </summary>
    public readonly struct CPF : IEquatable<CPF>, IComparable<CPF>, IParsable<CPF>
    {
        private const int Length = 11;
        private readonly string _value;

        private CPF(string value, bool _)
        {
            _value = value;
        }

        public CPF(string input)
        {
            _value = SetValue(input);
        }

        public static implicit operator CPF(string input) => new CPF(input);
        public static implicit operator string(CPF cpf) => cpf._value ?? string.Empty;

        public bool IsValid => !string.IsNullOrEmpty(_value);

        /// <summary>
        /// Converte a string em um CPF.
        /// </summary>
        /// <param name="input">String a ser convertida</param>
        /// <param name="provider">Nulo</param>
        /// <returns>O resultado da conversao</returns>
        /// <exception cref="ArgumentNullException">Se o input for nulo ou vazio</exception>
        /// <exception cref="ArgumentException">Se o input for um CPF invalido</exception>
        public static CPF Parse(string input, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input), "Input nao pode ser nulo ou uma string vazia");

            if (TryParse(input, provider, out var result))
                return result;

            throw new ArgumentException("CPF invalido", nameof(input));
        }

        /// <summary>
        /// Tenta converter uma string em um CPF
        /// </summary>
        /// <param name="input">String a ser convertida</param>
        /// <param name="provider">Nulo</param>
        /// <param name="result">Contem o resultado da conversao em caso de sucesso. Ou nulo em caso de falha</param>
        /// <returns>Verdadeiro se foi feito a conversao; caso contrario falso</returns>
        public static bool TryParse(string input, IFormatProvider? provider, out CPF result)
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

            int dv1 = CalculateCheckDigit(buffer[..9], 10);
            if (buffer[9] != (char)(dv1 + '0'))
            {
                result = default;
                return false;
            }

            int dv2 = CalculateCheckDigit(buffer[..10], 11);
            if (buffer[10] != (char)(dv2 + '0'))
            {
                result = default;
                return false;
            }

            result = new CPF(new string(buffer), true);
            return true;
        }

        /// <summary>
        /// Verifica se um CPF e valido segundo o calculo do DV conforme (Modulo 11, peso inicial 10 para o 1º DV, 11 para o 2º DV)
        /// </summary>
        /// <param name="input">String contendo o CPF com máscara, ou sem máscara</param>
        /// <returns>Verdadeiro se valido; caso contrario falso</returns>
        public static bool IsValidCpf(string input)
        {
            Span<char> buffer = stackalloc char[Length];
            int written = Sanitize(input, buffer);

            if (written != Length)
                return false;

            if (IsUniform(buffer))
                return false;

            int dv1 = CalculateCheckDigit(buffer[..9], 10);
            if (buffer[9] != (char)(dv1 + '0'))
                return false;

            int dv2 = CalculateCheckDigit(buffer[..10], 11);
            if (buffer[10] != (char)(dv2 + '0'))
                return false;

            return true;
        }

        /// <summary>
        /// Formata o CPF no padrao 000.000.000-00.
        /// </summary>
        public override string ToString()
        {
            if (!IsValid) return string.Empty;

            // string.Create é o método mais eficiente para formatar strings conhecidas em .NET Core+
            return string.Create(14, _value, (span, value) =>
            {
                var raw = value.AsSpan();
                // Formato: ABC.DEF.GHI-JK
                raw[..3].CopyTo(span[..3]);      // ABC
                span[3] = '.';
                raw[3..6].CopyTo(span[4..7]);    // DEF
                span[7] = '.';
                raw[6..9].CopyTo(span[8..11]);   // GHI
                span[11] = '-';
                raw[9..].CopyTo(span[12..]);     // JK
            });
        }

        private string SetValue(string input)
        {
            Span<char> buffer = stackalloc char[Length];
            int written = Sanitize(input, buffer);

            if (written != Length)
                throw new ArgumentOutOfRangeException(nameof(input), input, "Tamanho do CPF invalido. Tamanho permitido: 11 caracteres sem a mascara e 14 caracteres com mascara");

            if (IsUniform(buffer))
                throw new ArgumentException($"CPF nao pode possuir todos os digitos iguais {input}", nameof(input));

            int dv1 = CalculateCheckDigit(buffer[..9], 10);
            if (buffer[9] != (char)(dv1 + '0'))
                throw new ArgumentException($"Digito verificador invalido", nameof(input));

            int dv2 = CalculateCheckDigit(buffer[..10], 11);
            if (buffer[10] != (char)(dv2 + '0'))
                throw new ArgumentException($"Digito verificador invalido", nameof(input));

            return new string(buffer);
        }

        /// <summary>
        /// Extrai apenas dígitos da entrada. Retorna a quantidade de dígitos escritos.
        /// </summary>
        private static int Sanitize(ReadOnlySpan<char> input, Span<char> buffer)
        {
            int count = 0;
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    if (count >= Length) return -1; // Excedeu tamanho
                    buffer[count++] = c;
                }
            }
            return count;
        }

        /// <summary>
        /// Verifica se todos os digitos sao iguais. Otimizado para Span.
        /// </summary>
        private static bool IsUniform(ReadOnlySpan<char> buffer)
        {
            char first = buffer[0];

            var occurrence = buffer.Count(first);

            return occurrence == Length;
        }

        /// <summary>
        /// Calculo do DV do CPF (Modulo 11).
        /// </summary>
        /// <param name="slice">Os digitos a serem calculados (sem o DV)</param>
        /// <param name="weightStart">O peso inicial (10 para o 1º DV, 11 para o 2º DV)</param>
        private static int CalculateCheckDigit(ReadOnlySpan<char> slice, int weightStart)
        {
            int sum = 0;
            int weight = weightStart;

            for (int i = 0; i < slice.Length; i++)
            {
                sum += (slice[i] - '0') * weight--;
            }

            int remainder = sum % 11;
            return remainder < 2 ? 0 : 11 - remainder;
        }

        public bool Equals(CPF other) => string.Equals(_value, other._value, StringComparison.Ordinal);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is CPF other && Equals(other);
        public override int GetHashCode() => _value?.GetHashCode(StringComparison.Ordinal) ?? 0;
        public int CompareTo(CPF other) => string.Compare(_value, other._value, StringComparison.Ordinal);
        public static bool operator ==(CPF left, CPF right) => left.Equals(right);
        public static bool operator !=(CPF left, CPF right) => !left.Equals(right);
    }
}
