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

            for (int i = 1; i <= filters.Count; i++)
            {
                var filter = filters[i - 1];

                if (filter.QFactor < 0.5)
                    filter.QFactor = 0.5;
                else if (filter.QFactor > 10.0)
                    filter.QFactor = 10.0;

                switch (i)
                {
                    case 1:
                        jsonData.parametricEQ.filter1.enabled = true;
                        jsonData.parametricEQ.filter1.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter1.gain = filter.Gain;
                        jsonData.parametricEQ.filter1.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter1.type = ConvertType(filter.Type);
                        break;
                    case 2:
                        jsonData.parametricEQ.filter2.enabled = true;
                        jsonData.parametricEQ.filter2.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter2.gain = filter.Gain;
                        jsonData.parametricEQ.filter2.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter2.type = ConvertType(filter.Type);
                        break;
                    case 3:
                        jsonData.parametricEQ.filter3.enabled = true;
                        jsonData.parametricEQ.filter3.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter3.gain = filter.Gain;
                        jsonData.parametricEQ.filter3.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter3.type = ConvertType(filter.Type);
                        break;
                    case 4:
                        jsonData.parametricEQ.filter4.enabled = true;
                        jsonData.parametricEQ.filter4.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter4.gain = filter.Gain;
                        jsonData.parametricEQ.filter4.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter4.type = ConvertType(filter.Type);
                        break;
                    case 5:
                        jsonData.parametricEQ.filter5.enabled = true;
                        jsonData.parametricEQ.filter5.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter5.gain = filter.Gain;
                        jsonData.parametricEQ.filter5.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter5.type = ConvertType(filter.Type);
                        break;
                    case 6:
                        jsonData.parametricEQ.filter6.enabled = true;
                        jsonData.parametricEQ.filter6.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter6.gain = filter.Gain;
                        jsonData.parametricEQ.filter6.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter6.type = ConvertType(filter.Type);
                        break;
                    case 7:
                        jsonData.parametricEQ.filter7.enabled = true;
                        jsonData.parametricEQ.filter7.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter7.gain = filter.Gain;
                        jsonData.parametricEQ.filter7.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter7.type = ConvertType(filter.Type);
                        break;
                    case 8:
                        jsonData.parametricEQ.filter8.enabled = true;
                        jsonData.parametricEQ.filter8.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter8.gain = filter.Gain;
                        jsonData.parametricEQ.filter8.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter8.type = ConvertType(filter.Type);
                        break;
                    case 9:
                        jsonData.parametricEQ.filter9.enabled = true;
                        jsonData.parametricEQ.filter9.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter9.gain = filter.Gain;
                        jsonData.parametricEQ.filter9.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter9.type = ConvertType(filter.Type);
                        break;
                    case 10:
                        jsonData.parametricEQ.filter10.enabled = true;
                        jsonData.parametricEQ.filter10.qFactor = filter.QFactor;
                        jsonData.parametricEQ.filter10.gain = filter.Gain;
                        jsonData.parametricEQ.filter10.frequency = filter.Frequency;
                        jsonData.parametricEQ.filter10.type = ConvertType(filter.Type);
                        break;
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

            config.parametricEQ.filter1.enabled = true;
            config.parametricEQ.filter1.qFactor = QFACTOR;
            config.parametricEQ.filter1.frequency = 35;
            config.parametricEQ.filter1.gain = 0;
            config.parametricEQ.filter1.type = "peakingEQ";

            config.parametricEQ.filter2.enabled = true;
            config.parametricEQ.filter2.qFactor = QFACTOR;
            config.parametricEQ.filter2.frequency = 120;
            config.parametricEQ.filter2.gain = 0;
            config.parametricEQ.filter2.type = "peakingEQ";

            config.parametricEQ.filter3.enabled = true;
            config.parametricEQ.filter3.qFactor = QFACTOR;
            config.parametricEQ.filter3.frequency = 1000;
            config.parametricEQ.filter3.gain = 0;
            config.parametricEQ.filter3.type = "peakingEQ";

            config.parametricEQ.filter4.enabled = true;
            config.parametricEQ.filter4.qFactor = QFACTOR;
            config.parametricEQ.filter4.frequency = 6000;
            config.parametricEQ.filter4.gain = 0;
            config.parametricEQ.filter4.type = "peakingEQ";

            config.parametricEQ.filter5.enabled = true;
            config.parametricEQ.filter5.qFactor = QFACTOR;
            config.parametricEQ.filter5.frequency = 18000;
            config.parametricEQ.filter5.gain = 0;
            config.parametricEQ.filter5.type = "peakingEQ";

            config.parametricEQ.filter6.enabled = false;
            config.parametricEQ.filter6.qFactor = QFACTOR;
            config.parametricEQ.filter6.frequency = 1000;
            config.parametricEQ.filter6.gain = 0;
            config.parametricEQ.filter6.type = "peakingEQ";

            config.parametricEQ.filter7.enabled = false;
            config.parametricEQ.filter7.qFactor = QFACTOR;
            config.parametricEQ.filter7.frequency = 2000;
            config.parametricEQ.filter7.gain = 0;
            config.parametricEQ.filter7.type = "peakingEQ";

            config.parametricEQ.filter8.enabled = false;
            config.parametricEQ.filter8.qFactor = QFACTOR;
            config.parametricEQ.filter8.frequency = 4000;
            config.parametricEQ.filter8.gain = 0;
            config.parametricEQ.filter8.type = "peakingEQ";

            config.parametricEQ.filter9.enabled = false;
            config.parametricEQ.filter9.qFactor = QFACTOR;
            config.parametricEQ.filter9.frequency = 8000;
            config.parametricEQ.filter9.gain = 0;
            config.parametricEQ.filter9.type = "peakingEQ";

            config.parametricEQ.filter10.enabled = false;
            config.parametricEQ.filter10.qFactor = QFACTOR;
            config.parametricEQ.filter10.frequency = 16000;
            config.parametricEQ.filter10.gain = 0;
            config.parametricEQ.filter10.type = "peakingEQ";

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
    }
}
