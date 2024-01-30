using CommandLine;
using Newtonsoft.Json;
using SonarEQ.Commandline;
using SonarEQ.Sonar;
using SQLite;

namespace SonarEQ
{
    internal class Program
    {
        private static string databasePath = string.Empty;
        private const double QFACTOR = 0.7071;
        private const string STANDARD_TYPE = "peakingEQ";

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
        }

        static void RunOptions(Options opts)
        {
            if (!File.Exists(opts.EQFile))
            {
                Console.WriteLine("EQ File not found");
                return;
            }

            databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SteelSeries", "GG", "apps", "sonar", "db", "database.db");

            var newFilters = ParseFile(opts.EQFile);
            var channel = GetVad(opts.Channel);
            if (opts.Update)
            {
                var config = GetPreset(opts.Preset, channel);
                if (config == null)
                {
                    Console.WriteLine("Preset for Channel {0} with name {1} not found.", GetChannelName(channel), opts.Preset);
                    return;
                }

                WriteParametricEQ(ref config, newFilters);

                UpdatePreset(config);

                Console.WriteLine("Successfully updated Preset in Channel {0} with name {1}", GetChannelName(channel), opts.Preset);
                Console.WriteLine("You need to reopen the Sonar Window for the new config to load.");
            }
            else
            {
                if (CheckPresetExists(opts.Preset, channel))
                {
                    Console.WriteLine("Preset for Channel {0} with name {1} already exists.", GetChannelName(channel), opts.Preset);
                    return;
                }

                var emptyJson = CreateEmptyJsonData();
                var newConfig = CreateNewPreset(channel, opts.Preset, emptyJson);

                WriteParametricEQ(ref newConfig, newFilters);

                AddPreset(newConfig);

                Console.WriteLine("Successfully created Preset in Channel {0} with name {1}", GetChannelName(channel), opts.Preset);
                if (!opts.Update)
                {
                    Console.WriteLine("You need to Close and reopen Steelseries GG to see the new config.");
                }
            }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
        }

        private static bool CheckPresetExists(string name, VAD channel)
        {
            return GetPreset(name, channel) != null;
        }

        private static List<EQFilter> ParseFile(string filepath)
        {
            var result = new List<EQFilter>();

            var lines = File.ReadAllLines(filepath);

            foreach (var line in lines)
            {
                if (line.StartsWith("Filter") && line.Contains(':'))
                {
                    var resultLine = line;
                    while (resultLine.Contains("  "))
                    {
                        resultLine = resultLine.Replace("  ", " ");
                    }
                    var parsed = ParseLine(resultLine);
                    if (parsed != null)
                    {
                        result.Add(parsed);
                    }
                }
            }

            return result;
        }

        private static EQFilter ParseLine(string line)
        {
            var splits = line.Replace("Filter", string.Empty).Trim().Split(" ");

            foreach (var split in splits)
            {
                if (split == "None")
                {
                    return new EQFilter();
                }
            }

            var id = Convert.ToInt32(splits[0].Replace(":", string.Empty));
            var frequency = Convert.ToDouble(splits[4]);
            var gain = Convert.ToDouble(splits[7]);
            var qFactor = Convert.ToDouble(splits[10]);
            var type = splits[2];

            return new EQFilter(id, frequency, gain, qFactor, type);
        }

        private static Preset GetPreset(string configName, VAD vad)
        {
            using var db = new SQLiteConnection(databasePath, false);

            var vadInt = Convert.ToInt32(vad);

            return db.Table<Preset>()
                     .FirstOrDefault(config => config.name == configName && config.vad == vadInt);
        }

        private static void WriteParametricEQ(ref Preset config, List<EQFilter> filters)
        {
            var jsonText = config.data;

            var jsonData = JsonConvert.DeserializeObject<SonarPreset>(jsonText) ?? throw new Exception("Error parsing Preset data");

            //It ALWAYS has to be 10 filter entries in the json data.
            for (int i = 1; i <= 10; i++)
            {
                var filter = filters[i - 1];

                if (filter.IsEmpty)
                {
                    continue;
                }

                if (filter.QFactor < 0.5)
                    filter.QFactor = 0.5;
                else if (filter.QFactor > 10.0)
                    filter.QFactor = 10.0;

                var currentFilterProperty = jsonData.parametricEQ.GetType().GetProperty($"filter{i}");

                if (currentFilterProperty != null)
                {
                    var currentFilter = currentFilterProperty.GetValue(jsonData.parametricEQ);

                    if (currentFilter != null)
                    {
                        currentFilter.GetType().GetProperty("enabled")?.SetValue(currentFilter, true);
                        currentFilter.GetType().GetProperty("qFactor")?.SetValue(currentFilter, filter.QFactor);
                        currentFilter.GetType().GetProperty("gain")?.SetValue(currentFilter, filter.Gain);
                        currentFilter.GetType().GetProperty("frequency")?.SetValue(currentFilter, filter.Frequency);
                        currentFilter.GetType().GetProperty("type")?.SetValue(currentFilter, ConvertType(filter.Type));
                    }
                }
            }

            config.data = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
        }

        private static string ConvertType(string type)
        {
            return type switch
            {
                "LSC" => "lowShelving",
                "PK" => "peakingEQ",
                "HSC" => "highShelving",
                _ => string.Empty,
            };
        }

        private static void AddPreset(Preset config)
        {
            var db = new SQLiteConnection(databasePath, false);

            config.updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            config.created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            db.Insert(config);

            db.Close();
        }

        private static void UpdatePreset(Preset config)
        {
            var db = new SQLiteConnection(databasePath, false);

            config.updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            config.created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            db.Update(config);

            db.Close();
        }

        private static string CreateEmptyJsonData()
        {
            var config = new SonarPreset();
            config.bassBoostState.enabled = false;
            config.bassBoostState.value = 0;

            config.trebleBoostState.enabled = false;
            config.trebleBoostState.value = 0;

            config.voiceClarityState.enabled = false;
            config.voiceClarityState.value = 0;

            config.smartVolume.enabled = false;
            config.smartVolume.volumeLevel = 0;
            config.smartVolume.loudness = "balanced";

            config.generalGain = 0;

            config.virtualSurroundState = false;

            config.parametricEQ.enabled = true;

            for (int i = 1; i <= 10; i++)
            {
                var currentFilterProperty = config.parametricEQ.GetType().GetProperty($"filter{i}");

                if (currentFilterProperty != null)
                {
                    var currentFilter = currentFilterProperty.GetValue(config.parametricEQ);

                    if (currentFilter != null)
                    {
                        currentFilter.GetType().GetProperty("enabled")?.SetValue(currentFilter, false);
                        currentFilter.GetType().GetProperty("qFactor")?.SetValue(currentFilter, QFACTOR);
                        currentFilter.GetType().GetProperty("gain")?.SetValue(currentFilter, 0);
                        currentFilter.GetType().GetProperty("frequency")?.SetValue(currentFilter, GetStandardFrequency(i));
                        currentFilter.GetType().GetProperty("type")?.SetValue(currentFilter, STANDARD_TYPE);
                    }
                }
            }

            config.virtualSurroundChannels.frontLeft.position = 30;
            config.virtualSurroundChannels.frontLeft.gain = 0;

            config.virtualSurroundChannels.frontRight.position = -30;
            config.virtualSurroundChannels.frontRight.gain = 0;

            config.virtualSurroundChannels.center.position = 0;
            config.virtualSurroundChannels.center.gain = 0;

            config.virtualSurroundChannels.subWoofer.position = 0;
            config.virtualSurroundChannels.subWoofer.gain = 0;

            config.virtualSurroundChannels.rearLeft.position = 150;
            config.virtualSurroundChannels.rearLeft.gain = 0;

            config.virtualSurroundChannels.rearRight.position = -150;
            config.virtualSurroundChannels.rearRight.gain = 0;

            config.virtualSurroundChannels.sideLeft.position = 90;
            config.virtualSurroundChannels.sideLeft.gain = 0;

            config.virtualSurroundChannels.sideRight.position = -90;
            config.virtualSurroundChannels.sideRight.gain = 0;

            config.reverbGainDB = -6;

            config.formFactor = "headphones";

            config.globalEnableState = true;

            return JsonConvert.SerializeObject(config);
        }

        private static Preset CreateNewPreset(VAD vad, string name, string json)
        {
            var newEntry = new Preset
            {
                id = Guid.NewGuid().ToString().ToLower(),
                name = name,
                data = json,
                vad = Convert.ToInt32(vad),
                schema_version = 4,
                updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return newEntry;
        }

        private static string GetChannelName(VAD vad)
        {
            return vad switch
            {
                VAD.Game => "Game",
                VAD.Chat => "Chat",
                VAD.Mic => "Mic",
                VAD.Media => "Media",
                VAD.Aux => "AUX",
                _ => string.Empty,
            };
        }

        private static VAD GetVad(string channel)
        {
            return channel.ToLower() switch
            {
                "game" => VAD.Game,
                "chat" => VAD.Chat,
                "mic" => VAD.Mic,
                "media" => VAD.Media,
                "aux" => VAD.Aux,
                _ => VAD.Game,
            };
        }

        private static string GetStandardFrequency(Int32 filterIndex)
        {
            switch (filterIndex)
            {
                case 1:
                    return "35";
                case 2:
                    return "120";
                case 3:
                    return "1000";
                case 4:
                    return "6000";
                case 5:
                    return "18000";
                case 6:
                    return "1000";
                case 7:
                    return "2000";
                case 8:
                    return "4000";
                case 9:
                    return "8000";
                case 10:
                    return "16000";
                default:
                    return "NaN";
            }
        }
    }
}
