using System.Collections.Generic;
using System.Text.Json;

namespace GBCLV3.Models.JsonClasses
{
    class JArguments
    {
        // Heinous heterogeneous json
        public List<JsonElement> game { get; set; }

        // Useless for now
        // public List<JsonElement> jvm { get; set; }
    }
}
