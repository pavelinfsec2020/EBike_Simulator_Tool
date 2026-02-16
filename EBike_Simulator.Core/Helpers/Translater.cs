using EBike_Simulator.Core.Enums;
using EBike_Simulator.Core.Services;

namespace EBike_Simulator.Core.Helpers
{
    /// <summary>
    /// Вызывается в любом классе Core для получения перевода строки по ключу (записи хранятся в DbInitializer)
    /// </summary>
    public static class Translater
    {
        public static TranslaterService TranslateService { get; set; }
        public static Language Language { get; set; }
        public static string TranslateByKey(string key)
        {
            return TranslateService.Translate(key, Language).Result;
        }

    }
}
