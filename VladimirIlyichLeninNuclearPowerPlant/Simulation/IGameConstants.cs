using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VladimirIlyichLeninNuclearPowerPlant.Simulation
{
    public interface IGameConstants
    {
        double IntrinsicAbsorptionCoefficient { get; }
        double FuelAbsorptionCoefficient { get; }
        double GraphiteAbsorptionCoefficient { get; }
        double BoronAbsorptionCoefficient { get; }
        double FeedWaterAbsorptionCoefficient { get; }
        double ControlWaterAbsorptionCoefficient { get; }
        double XenonAbsorptionCoefficent { get; }

        double TemperatureAbsorptionCoefficient { get; } //change per degree C, centered around 200C

        double FastAbsorptionMultiplier { get; }

        double IntrinsicModerationCoefficient { get; }
        double GraphiteModerationCoefficient { get; }
        double BoronModerationCoefficient { get; }
        double FeedWaterModerationCoefficient { get; }
        double ControlWaterModerationCoefficient { get; }

        double FluxPerReaction { get; }

        double PreXenonProductionRateCoefficient { get; }
        double PreXenonDecayTimeConstant { get; }
        double XenonDecayTimeConstant { get; }
        double XenonBuringRateCoefficent { get; }

        double DelayedCriticalityTimeConstant { get; }
        double DelayedCriticalityProportion { get; }

        double ReactivityThermalGenerationCoefficient { get; }

        double ReactorThermalTransferCoefficient { get; } // W.K^1 per cell
        double FeedwaterThermalTransferCoefficient { get; } // W.K^1 per cell

        double SpontaneousFlux { get; }//from natural, random decay
    }
}
