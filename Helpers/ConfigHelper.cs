using CupMarker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CupMarker.Helpers
{
    public static class ConfigHelper
    {
        private static List<CupConfig>? _configs;
        public static List<CupConfig> All
        {
            get
            {
                if (_configs == null)
                {
                    var json = File.ReadAllText("CupConfig.json");
                    _configs = JsonSerializer.Deserialize<List<CupConfig>>(json)!;
                }
                return _configs;
            }
        }

        public static CupConfig? GetBySize(string? size) =>
            All.FirstOrDefault(c => c.Size == size);
    }
}
