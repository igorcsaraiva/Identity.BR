using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Identity.BR.ValueObjects;

namespace Identity.BR.Benchmark
{
    // Configura o benchmark para rodar com as otimizações do .NET 8/9
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser] // Monitora alocações de memória e GC
    [RankColumn]      // Cria um ranking de performance
    [HideColumns("Error", "StdDev", "Median")]
    public class CnpjBenchmark
    {
        private const string RawValid = "12ABC34501DE35";
        private const string MaskedValid = "12.ABC.345/01DE-35";
        private const string InvalidCnpj = "11.111.111/1111-11"; // Falha no IsUniform e DV

        private CNPJ _validInstance;
        private CNPJ _OtherValidInstance;

        [GlobalSetup]
        public void Setup()
        {
            _validInstance = RawValid;
            _OtherValidInstance = RawValid;
        }

        // --- Benchmarks de Validação (Onde o Zero-Allocation brilha) ---

        [Benchmark(Baseline = true)]
        public bool IsValidCnpj_Raw_Valid()
        {
            // Mede o tempo de processamento de uma string limpa
            return CNPJ.IsValidCnpj(RawValid);
        }

        [Benchmark]
        public bool IsValidCnpj_Masked_Valid()
        {
            // Mede o tempo com o overhead da sanitização (pular pontos e traços)
            return CNPJ.IsValidCnpj(MaskedValid);
        }

        [Benchmark]
        public bool IsValidCnpj_Invalid_Uniform()
        {
            // Mede o impacto do guardião 'IsUniform' que rejeita rapidamente
            return CNPJ.IsValidCnpj(InvalidCnpj);
        }

        // --- Benchmarks de Formatação (Uso de string.Create) ---

        [Benchmark]
        public string ToString_Formatted()
        {
            // Mede a performance do string.Create para gerar a máscara
            return _validInstance.ToString();
        }

        [Benchmark]
        public CNPJ Create_CNPJ_Without_Parse()
        {
            // Mede a performance de criação de um CNPJ
            CNPJ cnpj = RawValid;
            return cnpj;
        }

        [Benchmark]
        public CNPJ Create_CNPJ_With_Parse()
        {
            // Mede a performance de criação de um CNPJ
            var cnpj = CNPJ.Parse(RawValid);
            return cnpj;
        }

        [Benchmark]
        public bool Create_CNPJ_With_TryParse()
        {
            // Mede a performance de criação de um CNPJ
            var r = CNPJ.TryParse(RawValid, null, out var result);
            return r;
        }

        [Benchmark]
        public bool CNPJ_Equals_With_Other()
        {
            // Mede a performance de comparação de um CNPJ com outro
            var r = _validInstance.Equals(_OtherValidInstance);
            return r;
        }
    }
}
