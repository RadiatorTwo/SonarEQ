using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarEQ.Commandline
{
    public class Options
    {
        [Option('p', "preset", Required = true, HelpText = "The name of the preset, that should be created or updated.")]
        public string Preset { get; set; } = string.Empty;

        [Option('e', "eqfile", Required = true, HelpText = "The path to the config text file, that should be imported. (Format is EqualizerAPO ParametricEq)")]
        public string EQFile { get; set; } = string.Empty;

        [Option('c', "channel", Required = true, HelpText = "The channel the preset is for. Possible values: game, chat, mic, media, aux")]
        public string Channel { get; set; } = string.Empty;

        [Option('u', "update", Required = false, HelpText = "Update existing Preset")]
        public bool Update { get; set; }
    }
}
