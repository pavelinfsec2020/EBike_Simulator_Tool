using EBike_Simulator.Core.Enums;
using EBike_Simulator.Core.Interfaces.Repositories;

namespace EBike_Simulator.Core.Services
{
    /// <summary>
    /// Данный сервис необходим для получения строк на выбранном пользователем языке
    /// </summary>
    public class TranslaterService
    {
        #region fields
       
        private readonly ITranslationRepository _translaterRepository;

        #endregion


        #region ctor

        public TranslaterService(ITranslationRepository translationRepository)
        { 
           _translaterRepository = translationRepository;
        }

        #endregion

        #region Public methods

        public async Task<string> Translate(string key, Language language)
        {
            return await _translaterRepository.GetTranslationAsync(key, language);
        }

        #endregion;
    }
}
