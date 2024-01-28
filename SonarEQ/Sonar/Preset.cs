using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarEQ.Sonar
{
    public enum VAD
    {
        Game = 1,
        Chat = 2,
        Mic = 3,
        Media = 4,
        Aux = 5
    }

    [Table("configs")]
    public class Preset
    {
        [PrimaryKey]
        [Column("id")]
        public string id { get; set; } = string.Empty;
        [Column("name")]
        public string name { get; set; } = string.Empty;
        [Column("vad")]
        public int vad { get; set; }
        [Column("data")]
        public string data { get; set; } = string.Empty;
        [Column("schema_version")]
        public int schema_version { get; set; }
        [Column("created_at")]
        public string created_at { get; set; } = string.Empty;
        [Column("updated_at")]
        public string updated_at { get; set; } = string.Empty;
    }
}
