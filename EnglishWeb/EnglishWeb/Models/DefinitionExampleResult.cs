using Newtonsoft.Json;

namespace EnglishWeb.Models
{
    public class DefinitionExampleResult
    {
        [JsonProperty("definition")]
        public string Definition { get; set; }

        [JsonProperty("example")]
        public string Example { get; set; }
    }

}
