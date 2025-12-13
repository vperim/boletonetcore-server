using BoletoNetCore.BTGPactual;

namespace BoletoNetCore.Server.Services.Boletos;

/// <summary>
/// Creates fresh bank instances per request to avoid singleton concurrency issues.
/// Uses InternalsVisibleTo to access internal bank constructors.
/// </summary>
public sealed class BancoFactory : IBancoFactory
{
    private static readonly Dictionary<int, Func<IBanco>> Factories = new()
    {
        [001] = () => new BancoBrasil(),
        [004] = () => new BancoNordeste(),
        [033] = () => new BancoSantander(),
        [041] = () => new BancoBanrisul(),
        [077] = () => new BancoInter(),
        [084] = () => new BancoUniprimeNortePR(),
        [085] = () => new BancoCecred(),
        [097] = () => new BancoCrediSIS(),
        [104] = () => new BancoCaixa(),
        [208] = () => new BancoBTGPactual(),
        [237] = () => new BancoBradesco(),
        [341] = () => new BancoItau(),
        [422] = () => new BancoSafra(),
        [707] = () => new BancoDaycoval(),
        [748] = () => new BancoSicredi(),
        [756] = () => new BancoSicoob(),
    };

    public IBanco Create(int codigoBanco)
    {
        if (!Factories.TryGetValue(codigoBanco, out var factory))
        {
            throw new ArgumentException($"Banco {codigoBanco} não suportado");
        }

        var banco = factory();
        banco.Beneficiario ??= new Beneficiario();
        return banco;
    }
}
