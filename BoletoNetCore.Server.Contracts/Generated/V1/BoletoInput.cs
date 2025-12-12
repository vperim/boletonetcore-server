#nullable enable
using System;
using System.Globalization;
using BoletoNetCore.Server.Contracts.Generated.Types;
using BoletoNetCore.Server.Contracts.Generated.Validation;
using Google.Protobuf.WellKnownTypes;

namespace BoletoNetCore.Server.Contracts.Generated.V1;

public partial class BoletoInput
{
    /// <summary>
    /// Inicia a cadeia de builder escalonado para criar um BoletoInput.
    /// Campos obrigatórios são garantidos em tempo de compilação através da progressão de interfaces.
    /// </summary>
    public static IRequirePagador Builder() => new BoletoInputBuilder();

    #region Stepped Builder Interfaces

    public interface IRequirePagador
    {
        IRequireVencimento ComPagador(Pagador pagador);
        IRequireVencimento ComPagador(string cpfCnpj, string nome);
    }

    public interface IRequireVencimento
    {
        IRequireValor ComVencimento(DateTime dataVencimento);
    }

    public interface IRequireValor
    {
        IRequireNossoNumero ComValor(decimal valorTitulo);
        IRequireNossoNumero ComValor(Money valorTitulo);
    }

    public interface IRequireNossoNumero
    {
        IRequireDocumento ComNossoNumero(string nossoNumero);
    }

    public interface IRequireDocumento
    {
        IBoletoInputBuilder ComDocumento(string numeroDocumento, TipoEspecieDocumento especie);
    }

    /// <summary>
    /// Interface fluent builder para campos opcionais do BoletoInput.
    /// </summary>
    public interface IBoletoInputBuilder
    {
        IBoletoInputBuilder ComAceite(string aceite);
        IBoletoInputBuilder ComDataEmissao(DateTime dataEmissao);
        IBoletoInputBuilder ComDataProcessamento(DateTime dataProcessamento);
        IBoletoInputBuilder ComNumeroControle(string numeroControle);
        IBoletoInputBuilder ComJuros(DateTime dataJuros, decimal valorDia, TipoJuros tipo = TipoJuros.Simples);
        IBoletoInputBuilder ComJurosPercentual(DateTime dataJuros, decimal percentualDia, TipoJuros tipo = TipoJuros.Simples);
        IBoletoInputBuilder ComMulta(DateTime dataMulta, decimal valor, TipoCodigoMulta tipo = TipoCodigoMulta.Valor);
        IBoletoInputBuilder ComMultaPercentual(DateTime dataMulta, decimal percentual, TipoCodigoMulta tipo = TipoCodigoMulta.Percentual);

        /// <summary>
        /// Adiciona um desconto ao boleto. Máximo de 3 descontos permitidos.
        /// </summary>
        IBoletoInputBuilder ComDesconto(DateTime data, decimal valor);
        IBoletoInputBuilder ComInstrucoes(string? codigoInstrucao1 = null, string? codigoInstrucao2 = null);
        IBoletoInputBuilder ComMensagemCaixa(string mensagem);
        IBoletoInputBuilder ImprimirValoresAuxiliares(bool imprimir = true);
        BoletoInput Build();
    }

    #endregion

    #region Builder Implementation

    private sealed class BoletoInputBuilder :
        IRequirePagador,
        IRequireVencimento,
        IRequireValor,
        IRequireNossoNumero,
        IRequireDocumento,
        IBoletoInputBuilder
    {
        private readonly BoletoInput boleto = new();

        // Etapa 1: Pagador (obrigatório)
        public IRequireVencimento ComPagador(Pagador pagador)
        {
            Validator.Create()
                .Required(pagador, nameof(pagador), "Pagador não informado.")
                .ThrowIfInvalid();

            this.boleto.Pagador = pagador;
            return this;
        }

        public IRequireVencimento ComPagador(string cpfCnpj, string nome)
        {
            this.boleto.Pagador = Pagador.CriarValido(cpfCnpj, nome);
            return this;
        }

        // Etapa 2: Vencimento (obrigatório)
        public IRequireValor ComVencimento(DateTime dataVencimento)
        {
            this.boleto.DataVencimento = ToTimestamp(dataVencimento);
            return this;
        }

        // Etapa 3: Valor (obrigatório)
        public IRequireNossoNumero ComValor(decimal valorTitulo)
        {
            Validator.Create()
                .Value(valorTitulo > 0, nameof(valorTitulo), "Valor do título deve ser maior que zero.")
                .ThrowIfInvalid();

            this.boleto.ValorTitulo = Money.BrlPositivo(valorTitulo);
            return this;
        }

        public IRequireNossoNumero ComValor(Money valorTitulo)
        {
            Validator.Create()
                .Required(valorTitulo, nameof(valorTitulo), "Valor do título não informado.")
                .Value(valorTitulo?.ToDecimal() > 0, nameof(valorTitulo), "Valor do título deve ser maior que zero.")
                .ThrowIfInvalid();

            this.boleto.ValorTitulo = valorTitulo!;
            return this;
        }

        // Etapa 4: NossoNumero (obrigatório)
        public IRequireDocumento ComNossoNumero(string nossoNumero)
        {
            Validator.Create()
                .Required(nossoNumero, nameof(nossoNumero), "Nosso número não informado.")
                .ThrowIfInvalid();

            this.boleto.NossoNumero = nossoNumero.Trim();
            return this;
        }

        // Etapa 5: Documento (obrigatório) - transiciona para fluent builder
        public IBoletoInputBuilder ComDocumento(string numeroDocumento, TipoEspecieDocumento especie)
        {
            Validator.Create()
                .Required(numeroDocumento, nameof(numeroDocumento), "Número do documento não informado.")
                .Value(especie != TipoEspecieDocumento.Unspecified, nameof(especie), "Espécie do documento não informada.")
                .ThrowIfInvalid();

            this.boleto.NumeroDocumento = numeroDocumento.Trim();
            this.boleto.EspecieDocumento = especie;
            return this;
        }

        // Métodos fluent (opcionais)
        public IBoletoInputBuilder ComAceite(string aceite)
        {
            var aceiteUpper = aceite?.Trim().ToUpperInvariant();

            Validator.Create()
                .Required(aceite, nameof(aceite), "Aceite não informado.")
                .Format(aceiteUpper is "A" or "S" or "N", nameof(aceite), "Aceite deve ser 'A', 'S' ou 'N'.")
                .ThrowIfInvalid();

            this.boleto.Aceite = aceiteUpper!;
            return this;
        }

        public IBoletoInputBuilder ComDataEmissao(DateTime dataEmissao)
        {
            this.boleto.DataEmissao = ToTimestamp(dataEmissao);
            return this;
        }

        public IBoletoInputBuilder ComDataProcessamento(DateTime dataProcessamento)
        {
            this.boleto.DataProcessamento = ToTimestamp(dataProcessamento);
            return this;
        }

        public IBoletoInputBuilder ComNumeroControle(string numeroControle)
        {
            if (!string.IsNullOrWhiteSpace(numeroControle))
                this.boleto.NumeroControleParticipante = numeroControle.Trim();
            return this;
        }

        public IBoletoInputBuilder ComJuros(DateTime dataJuros, decimal valorDia, TipoJuros tipo = TipoJuros.Simples)
        {
            Validator.Create()
                .Value(valorDia > 0, nameof(valorDia), "Valor de juros deve ser maior que zero.")
                .ThrowIfInvalid();

            this.boleto.DataJuros = ToTimestamp(dataJuros);
            this.boleto.ValorJurosDia = Money.BrlPositivo(valorDia);
            this.boleto.TipoJuros = tipo == TipoJuros.Unspecified ? TipoJuros.Simples : tipo;
            return this;
        }

        public IBoletoInputBuilder ComJurosPercentual(DateTime dataJuros, decimal percentualDia, TipoJuros tipo = TipoJuros.Simples)
        {
            Validator.Create()
                .Value(percentualDia > 0, nameof(percentualDia), "Percentual de juros deve ser maior que zero.")
                .ThrowIfInvalid();

            this.boleto.DataJuros = ToTimestamp(dataJuros);
            this.boleto.PercentualJurosDia = percentualDia.ToString("F4", CultureInfo.InvariantCulture);
            this.boleto.TipoJuros = tipo == TipoJuros.Unspecified ? TipoJuros.Simples : tipo;
            return this;
        }

        public IBoletoInputBuilder ComMulta(DateTime dataMulta, decimal valor, TipoCodigoMulta tipo = TipoCodigoMulta.Valor)
        {
            Validator.Create()
                .Value(valor > 0, nameof(valor), "Valor da multa deve ser maior que zero.")
                .ThrowIfInvalid();

            this.boleto.DataMulta = ToTimestamp(dataMulta);
            this.boleto.ValorMulta = Money.BrlPositivo(valor);
            this.boleto.TipoCodigoMulta = tipo == TipoCodigoMulta.Unspecified ? TipoCodigoMulta.Valor : tipo;
            return this;
        }

        public IBoletoInputBuilder ComMultaPercentual(DateTime dataMulta, decimal percentual, TipoCodigoMulta tipo = TipoCodigoMulta.Percentual)
        {
            Validator.Create()
                .Value(percentual > 0, nameof(percentual), "Percentual da multa deve ser maior que zero.")
                .ThrowIfInvalid();

            this.boleto.DataMulta = ToTimestamp(dataMulta);
            this.boleto.PercentualMulta = percentual.ToString("F4", CultureInfo.InvariantCulture);
            this.boleto.TipoCodigoMulta = tipo == TipoCodigoMulta.Unspecified ? TipoCodigoMulta.Percentual : tipo;
            return this;
        }

        public IBoletoInputBuilder ComDesconto(DateTime data, decimal valor)
        {
            Validator.Create()
                .Value(valor > 0, nameof(valor), "Valor do desconto deve ser maior que zero.")
                .Limit(this.boleto.Descontos.Count < 3, "descontos", "Máximo de 3 descontos permitidos.")
                .ThrowIfInvalid();

            this.boleto.Descontos.Add(Desconto.Criar(data, valor));
            return this;
        }

        public IBoletoInputBuilder ComInstrucoes(string? codigoInstrucao1 = null, string? codigoInstrucao2 = null)
        {
            if (!string.IsNullOrWhiteSpace(codigoInstrucao1))
                this.boleto.CodigoInstrucao1 = codigoInstrucao1!.Trim();

            if (!string.IsNullOrWhiteSpace(codigoInstrucao2))
                this.boleto.CodigoInstrucao2 = codigoInstrucao2!.Trim();

            return this;
        }

        public IBoletoInputBuilder ComMensagemCaixa(string mensagem)
        {
            if (!string.IsNullOrWhiteSpace(mensagem))
                this.boleto.MensagemInstrucoesCaixa = mensagem.Trim();
            return this;
        }

        public IBoletoInputBuilder ImprimirValoresAuxiliares(bool imprimir = true)
        {
            this.boleto.ImprimirValoresAuxiliares = imprimir;
            return this;
        }

        public BoletoInput Build()
        {
            if (string.IsNullOrEmpty(this.boleto.Aceite))
                this.boleto.Aceite = "N";

            return this.boleto;
        }

        private static Timestamp ToTimestamp(DateTime date)
            => Timestamp.FromDateTime(DateTime.SpecifyKind(date.Date, DateTimeKind.Utc));
    }

    #endregion
}
