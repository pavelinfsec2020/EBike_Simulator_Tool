using EBike_Simulator.Core.Services;
using EBike_Simulator.Data.Data;
using EBike_Simulator.Data.Repositories;

namespace EBike_Simulator.Data
{
    public class DataInitializer
    {
        #region fields

        private TranslationRepository _translationRepos;
        private readonly AppDbContext _appDbContext;
        #endregion

        #region ctor

        public DataInitializer()
        { 
            _appDbContext = new AppDbContext();
        }

        #endregion

        #region props

        public TranslationRepository TranslationRepos => _translationRepos;

        #endregion

        #region methods

        public async void StartAsync()
        {

            await DbInitializer.InitializeAsync(_appDbContext);

            _translationRepos = new TranslationRepository(_appDbContext);        
        }

        #endregion
    }
}
