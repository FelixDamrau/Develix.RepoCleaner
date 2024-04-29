using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace Develix.RepoCleaner.ConsoleComponents.Cli;
internal class ConfigSettings : CommandSettings
{
    [CommandOption("--config")]
    public bool Config { get; set; }
}
