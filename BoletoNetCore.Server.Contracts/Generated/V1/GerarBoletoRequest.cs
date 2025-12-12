#nullable enable
using BoletoNetCore.Server.Contracts.Generated.Validation;

namespace BoletoNetCore.Server.Contracts.Generated.V1;

public partial class GerarBoletoRequest
{
    /// <summary>
    /// Inicia a cadeia de builder escalonado para criar um GerarBoletoRequest.
    /// Campos obrigatórios são garantidos em tempo de compilação através da progressão de interfaces.
    /// </summary>
    /// <param name="bancoCodigo">Código do banco (001, 033, 237, 341, 748, etc.)</param>
    /// <exception cref="BoletoValidationException">Lançada quando o código do banco é inválido.</exception>
    public static IRequireCarteira Builder(int bancoCodigo)
    {
        Validator.Create()
            .Value(bancoCodigo > 0, nameof(bancoCodigo), "Código do banco deve ser maior que zero.")
            .ThrowIfInvalid();

        return new GerarBoletoRequestBuilder(bancoCodigo);
    }

    #region Stepped Builder Interfaces

    public interface IRequireCarteira
    {
        IRequireBeneficiario ComCarteira(string carteira, TipoCarteira tipoCarteira, string? variacao = null);
    }

    public interface IRequireBeneficiario
    {
        IRequireBoleto ComBeneficiario(Beneficiario beneficiario);
    }

    public interface IRequireBoleto
    {
        IGerarBoletoRequestBuilder ComBoleto(params BoletoInput[] boletos);
    }

    /// <summary>
    /// Interface fluent builder para campos opcionais do GerarBoletoRequest.
    /// </summary>
    public interface IGerarBoletoRequestBuilder
    {
        IGerarBoletoRequestBuilder ComBoleto(params BoletoInput[] boletos);
        IGerarBoletoRequestBuilder ComOutputFormat(OutputFormat formato);
        GerarBoletoRequest Build();
    }

    #endregion

    #region Builder Implementation

    private sealed class GerarBoletoRequestBuilder :
        IRequireCarteira,
        IRequireBeneficiario,
        IRequireBoleto,
        IGerarBoletoRequestBuilder
    {
        private readonly GerarBoletoRequest request;

        public GerarBoletoRequestBuilder(int bancoCodigo)
        {
            this.request = new GerarBoletoRequest
            {
                BancoCodigo = bancoCodigo,
                OutputFormat = OutputFormat.Pdf
            };
        }

        // Etapa 1: Carteira (obrigatório)
        public IRequireBeneficiario ComCarteira(string carteira, TipoCarteira tipoCarteira, string? variacao = null)
        {
            Validator.Create()
                .Required(carteira, nameof(carteira), "Carteira não informada.")
                .Value(tipoCarteira != TipoCarteira.Unspecified, nameof(tipoCarteira), "Tipo de carteira não informado.")
                .ThrowIfInvalid();

            this.request.Carteira = carteira.Trim();
            this.request.TipoCarteira = tipoCarteira;

            if (!string.IsNullOrWhiteSpace(variacao))
                this.request.VariacaoCarteira = variacao!.Trim();

            return this;
        }

        // Etapa 2: Beneficiario (obrigatório)
        public IRequireBoleto ComBeneficiario(Beneficiario beneficiario)
        {
            Validator.Create()
                .Required(beneficiario, nameof(beneficiario), "Beneficiário não informado.")
                .ThrowIfInvalid();

            this.request.Beneficiario = beneficiario;
            return this;
        }

        // Etapa 3: Boleto (obrigatório, pelo menos um)
        public IGerarBoletoRequestBuilder ComBoleto(params BoletoInput[] boletos)
        {
            var validator = Validator.Create()
                .Value(boletos is { Length: > 0 }, nameof(boletos), "Pelo menos um boleto deve ser informado.");

            validator.ThrowIfInvalid();

            foreach (var boleto in boletos!)
                this.request.Boletos.Add(boleto);

            return this;
        }

        // Fluent: OutputFormat (opcional, padrão PDF)
        public IGerarBoletoRequestBuilder ComOutputFormat(OutputFormat formato)
        {
            Validator.Create()
                .Value(formato != OutputFormat.Unspecified, nameof(formato), "Formato de saída não informado.")
                .ThrowIfInvalid();

            this.request.OutputFormat = formato;
            return this;
        }

        public GerarBoletoRequest Build() => this.request;
    }

    #endregion
}
