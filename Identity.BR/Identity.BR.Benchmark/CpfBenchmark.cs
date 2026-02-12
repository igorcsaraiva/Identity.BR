using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Identity.BR.ValueObjects;

namespace Identity.BR.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser] // Monitora alocações de memória e GC
    [RankColumn]      // Cria um ranking de performance
    [HideColumns("Error", "StdDev", "Median")]
    public class CpfBenchmark
    {
        private const string RawCpf = "76633770014";
        private const string MaskedCpf = "766.337.700-14";
        private const string InvalidCpf = "111.111.111-11";

        private CPF _validInstance;
        private CPF _OtherValidInstance;

        [GlobalSetup]
        public void Setup()
        {
            _validInstance = RawCpf;
            _OtherValidInstance = RawCpf;
        }

        // --- Benchmarks de Validação (Onde o Zero-Allocation brilha) ---

        [Benchmark(Baseline = true)]
        public bool IsValidCpf_Raw_Valid()
        {
            // Mede o tempo de processamento de uma string limpa
            return CPF.IsValidCpf(RawCpf);
        }

        [Benchmark]
        public bool IsValidCpf_Masked_Valid()
        {
            // Mede o tempo com o overhead da sanitização (pular pontos e traços)
            return CPF.IsValidCpf(MaskedCpf);
        }

        [Benchmark]
        public bool IsValidCpf_Invalid_Uniform()
        {
            // Mede o impacto do guardião 'IsUniform' que rejeita rapidamente
            return CPF.IsValidCpf(InvalidCpf);
        }

        // --- Benchmarks de Formatação (Uso de string.Create) ---

        [Benchmark]
        public string ToString_Formatted()
        {
            // Mede a performance do string.Create para gerar a máscara
            return _validInstance.ToString();
        }

        [Benchmark]
        public CPF Create_CPF_Without_Parse()
        {
            // Mede a performance de criação de um CPF
            CPF cpf = RawCpf;
            return cpf;
        }

        [Benchmark]
        public CPF Create_CPF_With_Parse()
        {
            // Mede a performance de criação de um CPF
            var cpf = CPF.Parse(RawCpf);
            return cpf;
        }

        [Benchmark]
        public bool Create_CPF_With_TryParse()
        {
            // Mede a performance de criação de um CPF
            var r = CPF.TryParse(RawCpf, null, out var result);
            return r;
        }

        [Benchmark]
        public bool CPF_Equals_With_Other()
        {
            // Mede a performance de comparação de um CPF com outro
            var r = _validInstance.Equals(_OtherValidInstance);
            return r;
        }
    }
}
