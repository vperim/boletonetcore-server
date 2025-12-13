using System;

namespace BoletoNetCore
{
    [CarteiraCodigo("06")]
    public class BancoBradescoCarteira06 : BancoBradescoCarteiraBase, ICarteira<BancoBradesco>
    {
        internal static Lazy<ICarteira<BancoBradesco>> Instance { get; } = new Lazy<ICarteira<BancoBradesco>>(() => new BancoBradescoCarteira06());

        private BancoBradescoCarteira06()
        {
        }
    }
}