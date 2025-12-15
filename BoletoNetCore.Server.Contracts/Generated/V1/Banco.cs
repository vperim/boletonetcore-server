namespace BoletoNetCore.Server.Contracts.Generated.V1;

public partial class Banco
{
    public static Banco Instancia(int codigo) => new() { Codigo = codigo };
}

public partial class Boleto
{
    public Boleto(Banco banco) : this()
    {
        this.Carteira = banco.Beneficiario.ContaBancaria.CarteiraPadrao;
        this.CarteiraImpressaoBoleto = banco.Beneficiario.ContaBancaria.CarteiraPadrao;
        this.VariacaoCarteira = banco.Beneficiario.ContaBancaria.VariacaoCarteiraPadrao;
        this.TipoCarteira = banco.Beneficiario.ContaBancaria.TipoCarteiraPadrao;
    }
}
