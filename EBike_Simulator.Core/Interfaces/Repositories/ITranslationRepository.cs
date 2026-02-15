using EBike_Simulator.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Interfaces.Repositories
{
    public interface ITranslationRepository
    {
        Task<string> GetTranslationAsync(string key, Language language);
    }
}
