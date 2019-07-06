using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VladimirIlyichLeninNuclearPowerPlant.Simulation
{
    class Cell
    {
        public double PromptRate { get; set; }
        public double DelayedRate { get; set; }
        public double FastFlux { get; set; }
        public double SlowFlux { get; set; }
        public double PreXenon { get; set; }
        public double Xenon { get; set; }
        public double Temp { get; set; } //C
        public double WaterTemp { get; set; } //C
        public double WaterResistance { get; set; } 
        public double WaterFlow { get; set; } //kg/s
        public double SteamPercent { get; set; }

        public double RodPercent { get; set; }
        public double GraphitePercent { get; set; }
        public double WaterPercent { get; set; }

        public double ModerationPercent { get; set; }
        public double NonReactiveAbsorbtionPercent { get; set; }
        public double ReactiveAbsorbtionPercent { get; set; }
    }
}
