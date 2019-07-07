using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VladimirIlyichLeninNuclearPowerPlant.Simulation
{
    class StandardConstants : IGameConstants
    {
        public double IntrinsicAbsorptionCoefficient { get; } = 0.03;
        public double FuelAbsorptionCoefficient { get; } = 0.31;
        public double GraphiteAbsorptionCoefficient { get; } = 0.01;//0.02;
        public double BoronAbsorptionCoefficient { get; } = 0.05;//0.1;
        public double FeedWaterAbsorptionCoefficient { get; } = 0.03;
        public double ControlWaterAbsorptionCoefficient { get; } = 0.03;//0.01;
        public double XenonAbsorptionCoefficent { get; } = 0.1;

        public double TemperatureAbsorptionCoefficient { get; } = -0.00002; //change per degree C, centered around 200C

        public double FastAbsorptionMultiplier { get; } = 0.05;

        public double IntrinsicModerationCoefficient { get; } = 0.38;
        public double GraphiteModerationCoefficient { get; } = 0.1;// 0.05;
        public double BoronModerationCoefficient { get; } = 0.025;// 0.01;
        public double FeedWaterModerationCoefficient { get; } = 0.02;
        public double ControlWaterModerationCoefficient { get; } = 0.03;// 0.075;

        public double FluxPerReaction { get; } = 2.5;

        public double PreXenonProductionRateCoefficient { get; } = 0.01;
        public double PreXenonDecayTimeConstant { get; } = 30;
        public double XenonDecayTimeConstant { get; } = 90;
        public double XenonBuringRateCoefficent { get; } = 1;

        public double DelayedCriticalityTimeConstant { get; } = 20;
        public double DelayedCriticalityProportion { get; } = 0.02;

        public double ReactivityThermalGenerationCoefficient { get; } = 0.05; // MW/flux unit

        public double ReactorThermalTransferCoefficient { get; } = 0.2; // MW.K^1 per cell
        public double FeedwaterThermalTransferCoefficient { get; } = 0.1; // MW.K^1 per cell
        public double ReactorThermalCapacity { get; } = 5; // MJ/K per cell
        public double FeedwaterThermalCapacity { get; } = 0.5; // MJ/K  per cell
        public double WaterPerCell { get; } = 0.5; // kg  per cell

        public double SpontaneousFlux { get; } = 0.005;//from natural, random decay


    }
}
