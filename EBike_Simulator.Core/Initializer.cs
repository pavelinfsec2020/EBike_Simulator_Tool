using EBike_Simulator.Core.Enums;
using EBike_Simulator.Core.Interfaces.Repositories;
using EBike_Simulator.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

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
        }

        #endregion

        #region props

        public Language Language { get; set; }
        public TranslaterService Translater => _translater;
        #endregion
    }
}
