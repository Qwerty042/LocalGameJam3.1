using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VladimirIlyichLeninNuclearPowerPlant.Simulation
{
    class Cell
    {
        public double PromptRate { get; set; } = 0;
        public double DelayedFlux { get; set; } = 0;//amount of 'saved up' flux
        public double DelayedRate { get; set; } = 0;//for display purposes
        public double[] FastFlux { get; set; } = new double[4];//0 = up, 1 = right, 2 = down, 3 = left
        public double[] SlowFlux { get; set; } = new double[4];//0 = up, 1 = right, 2 = down, 3 = left
        public double PreXenon { get; set; } = 0;
        public double Xenon { get; set; } = 0;
        public double Temp { get; set; } = 100; //C
        public double WaterTemp { get; set; } = 100; //C
        public double WaterResistance { get; set; } = 0;
        public double WaterFlow { get; set; } = 0; //kg/s
        public double SteamPercent { get; set; } = 0;

        public double RodPercent { get; set; } = 0;
        public double GraphitePercent { get; set; } = 0;
        public double WaterPercent { get; set; } = 0;

        public double ModerationPercent { get; set; } = 0;
        public double NonReactiveAbsorbtionPercent { get; set; } = 0;
        public double ReactiveAbsorbtionPercent { get; set; } = 0;
    }
}
