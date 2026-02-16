using EBike_Simulator.Core.Enums;
using EBike_Simulator.Core.Helpers;
using EBike_Simulator.Core.Interfaces.Repositories;
using EBike_Simulator.Core.Services;

namespace EBike_Simulator.Core
{
    public class Initializer
    {
        #region fields

        private readonly TranslaterService _translater;

        #endregion

        #region ctor

        public Initializer(ITranslationRepository translationRepository)
        {
            _translater = new TranslaterService(translationRepository);
            Translater.TranslateService = _translater;
            Translater.Language = Language.Ru;
        }

        #endregion

    }
}
