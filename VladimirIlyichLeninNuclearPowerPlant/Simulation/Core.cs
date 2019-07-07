using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VladimirIlyichLeninNuclearPowerPlant.Simulation
{
    class Core
    {
        public Cell[,] cells;
        List<ControlRod> controlRods;
        IGameConstants constants;

        private double _InletTemp = 200;
        public double InletTemp { set { _InletTemp = value; } }

        private double _InletPressure = 50;//bar
        public double InletPressure { set { _InletPressure = value; } }

        private double _InletFlow = 10;
        public double InletFlow { get { return _InletFlow; } }

        private double _OutletTemp = 10;
        public double OutletTemp { get { return _OutletTemp; } }

        private double _OutletFlow = 10;
        public double OutletFlow { get { return _OutletFlow; } }

        private double _PowerLevel = 0;
        public double PowerLevel { get { return _PowerLevel; } }

        public double PromptCriticality { get { return totalPrompt / totalEmitted; } }
        public double DelayedCriticality { get { return totalPromptAndDelayed / totalEmitted; } }

        private double totalEmitted = 0;
        private double totalPrompt = 0;
        private double totalPromptAndDelayed = 0;
        public double exactPower = 0;
        public double avgTemp = 0;


        public Core(List<ControlRod> controlRods, IGameConstants constants)
        {
            this.controlRods = controlRods;
            this.constants = constants;

            cells = new Cell[5, 5];

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    cells[i, j] = new Cell();
                }
            }


        }

        public void update(double deltaT)
        {
            if (deltaT > 0)
            {
                if(checkDed())
                {
                    return;
                }
                int subdiv = 50;
                deltaT /= subdiv;
                for (int i = 0; i < subdiv; i++)
                {
                    updateControlRodStatus();
                    performNeutronSimulation(deltaT);
                    updateXenon(deltaT);
                    transferTemp(deltaT);
                    waterFlowSim(deltaT);
                    powerEstimate();
                    exactPower = totalEmitted * constants.ReactivityThermalGenerationCoefficient / deltaT;
                    double sumTemp = 0;
                    foreach(var cell in cells)
                    {
                        sumTemp += cell.Temp;
                    }
                    avgTemp = sumTemp / 25;
                }
            }
        }

        public bool checkDed()
        {
            if(PowerLevel > 99999)
            {
                return true;
            }
            foreach(var cell in cells)
            {
                if (cell.Temp > 1500)
                {
                    return true;
                }
                if (cell.WaterTemp > 800)
                {
                    return true;
                }
            }
            return false;
        }

        void updateControlRodStatus()
        {
            //determine how much of each control rod part is in each cell
            for (int i = 0; i < 5; i++)
            {
                //for each column/rod
                var controlRod = controlRods[i];
                for (int j = 0; j < 5; j++)
                {
                    cells[i, j].SteamPercent = 0;
                    //for each cell
                    var adaptedPercent = controlRod.insertedPercentage - 20 * j + 80;
                    if (adaptedPercent > 100)
                    {
                        cells[i, j].RodPercent = 100;
                        cells[i, j].GraphitePercent = 0;
                        cells[i, j].WaterPercent = 0;
                    }
                    else if (adaptedPercent > 80)
                    {
                        cells[i, j].RodPercent = 100 * (adaptedPercent - 80) / 20;
                        cells[i, j].GraphitePercent = 0;
                        cells[i, j].WaterPercent = 0;
                    }
                    else if (adaptedPercent > 60)
                    {
                        cells[i, j].RodPercent = 0;
                        cells[i, j].GraphitePercent = 100 - 100 * (adaptedPercent - 60) / 20;
                        cells[i, j].WaterPercent = 0;
                    }
                    else if (adaptedPercent > 20)
                    {
                        cells[i, j].RodPercent = 0;
                        cells[i, j].GraphitePercent = 100;
                        cells[i, j].WaterPercent = 0;
                    }
                    else if (adaptedPercent > 0)
                    {
                        cells[i, j].RodPercent = 0;
                        cells[i, j].GraphitePercent = (adaptedPercent) / 20 * 100;
                        cells[i, j].WaterPercent = (1 - (adaptedPercent) / 20) * (100 - cells[i, j].SteamPercent);
                    }
                }
            }
        }

        void performNeutronSimulation(double deltaT)
        {
            emitNeutrons(deltaT);
            for(int i = 0; i < 5; i++)
            {
                moderateNeutrons();
                nonReactiveAbsorbNeutrons();
                reactiveAbsorbNeutrons(deltaT);
                transferNeutrons();
            }
            updateCriticality();
        }

        void emitNeutrons(double deltaT)
        {
            totalEmitted = 0;
            totalPrompt = 0;
            totalPromptAndDelayed = 0;
            foreach(var cell in cells)
            {
                cell.CellEmitted = 0;
                cell.CellDelayedAbsorb = 0;
                cell.CellPromptAbsorb = 0;
                var emittedDelayedFlux = cell.DelayedFlux * deltaT / constants.DelayedCriticalityTimeConstant;
                cell.DelayedFlux -= emittedDelayedFlux;
                cell.DelayedRate = emittedDelayedFlux / deltaT;

                var totalFlux = constants.SpontaneousFlux * deltaT + emittedDelayedFlux + cell.PromptRate * deltaT;
                cell.PreXenon += totalFlux * constants.PreXenonProductionRateCoefficient;
                cell.Temp += totalFlux * constants.ReactivityThermalGenerationCoefficient / constants.ReactorThermalCapacity;
                cell.CellEmitted = constants.SpontaneousFlux * deltaT + emittedDelayedFlux + cell.PromptRate * deltaT;
                totalEmitted += cell.CellEmitted;
                cell.PromptRate = 0;
                for (int i = 0; i < 4; i++)
                {
                    cell.FastFlux[i] = totalFlux / 4;
                    cell.SlowFlux[i] = 0;
                }
            }
        }

        void moderateNeutrons()
        {
            foreach (var cell in cells)
            {
                var totalModerationCoefficent = constants.IntrinsicModerationCoefficient
                    + constants.GraphiteModerationCoefficient * cell.GraphitePercent / 100
                    + constants.FeedWaterModerationCoefficient * (100 - cell.SteamPercent) / 100
                    + constants.ControlWaterModerationCoefficient * cell.WaterPercent / 100
                    + constants.BoronModerationCoefficient * cell.RodPercent / 100;
                cell.ModerationPercent = totalModerationCoefficent * 100;
                for (int i = 0; i < 4; i++)
                {
                    var moderatedAmount = cell.FastFlux[i] * totalModerationCoefficent;
                    cell.FastFlux[i] -= moderatedAmount;
                    cell.SlowFlux[i] += moderatedAmount;
                }
            }
        }

        void nonReactiveAbsorbNeutrons()
        {
            foreach (var cell in cells)
            {
                var totalAbsorptionCoefficent = constants.IntrinsicAbsorptionCoefficient
                    + constants.GraphiteAbsorptionCoefficient * cell.GraphitePercent / 100
                    + constants.FeedWaterAbsorptionCoefficient * (100 - cell.SteamPercent) / 100
                    + constants.ControlWaterAbsorptionCoefficient * cell.WaterPercent / 100
                    + constants.BoronAbsorptionCoefficient * cell.RodPercent / 100;

                totalAbsorptionCoefficent *= 1 + (cell.Temp - 200) * constants.TemperatureAbsorptionCoefficient;

                cell.NonReactiveAbsorbtionPercent = totalAbsorptionCoefficent * 100;

                for (int i = 0; i < 4; i++)
                {
                    cell.FastFlux[i] *= 1 - (totalAbsorptionCoefficent * constants.FastAbsorptionMultiplier);
                    cell.SlowFlux[i] *= 1 - totalAbsorptionCoefficent;
                }

                var xenonAbsorption = 1 - Math.Pow(1-constants.XenonAbsorptionCoefficent, cell.Xenon);
                double absorbed = 0;
                for (int i = 0; i < 4; i++)
                {
                    absorbed += cell.FastFlux[i] * (xenonAbsorption * constants.FastAbsorptionMultiplier) + cell.SlowFlux[i] * xenonAbsorption;
                    cell.FastFlux[i] *= 1 - (xenonAbsorption * constants.FastAbsorptionMultiplier);
                    cell.SlowFlux[i] *= 1 - xenonAbsorption;
                }
                cell.Xenon -= absorbed * constants.XenonBuringRateCoefficent;
                cell.NonReactiveAbsorbtionPercent = (1- (1- totalAbsorptionCoefficent) * (1- xenonAbsorption)) * 100;
            }
        }

        void reactiveAbsorbNeutrons(double deltaT)
        {
            foreach (var cell in cells)
            {
                var absorption = constants.FuelAbsorptionCoefficient
                    * (1 + (cell.Temp - 200) * constants.TemperatureAbsorptionCoefficient);

                double absorbed = 0;
                cell.ReactiveAbsorbtionPercent = absorption * 100;

                for (int i = 0; i < 4; i++)
                {
                    absorbed += cell.FastFlux[i] * (absorption * constants.FastAbsorptionMultiplier) + cell.SlowFlux[i] * absorption;
                    cell.FastFlux[i] *= 1 - (absorption * constants.FastAbsorptionMultiplier);
                    cell.SlowFlux[i] *= 1 - absorption;
                }

                var promptAbsorb = absorbed * (1 - constants.DelayedCriticalityProportion) * constants.FluxPerReaction;
                var delayedAbsorb = absorbed * constants.DelayedCriticalityProportion* constants.FluxPerReaction;

                cell.CellPromptAbsorb += promptAbsorb;
                cell.CellDelayedAbsorb += promptAbsorb + delayedAbsorb;

                totalPrompt += promptAbsorb;
                totalPromptAndDelayed += promptAbsorb + delayedAbsorb;

                cell.PromptRate += promptAbsorb / deltaT;
                cell.DelayedFlux += delayedAbsorb;
            }
        }

        void transferNeutrons()
        {
            //up flux
            for (int i = 0; i < 5; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    cells[i, j - 1].FastFlux[0] = cells[i, j].FastFlux[0];
                    cells[i, j - 1].SlowFlux[0] = cells[i, j].SlowFlux[0];
                    if (j == 4)
                    {
                        cells[i, j].FastFlux[0] = 0;
                        cells[i, j].SlowFlux[0] = 0;
                    }
                }
            }
            //right flux
            for (int i = 3; i >= 0; i--)
            {
                for (int j = 0; j < 5; j++)
                {
                    cells[i + 1, j].FastFlux[1] = cells[i, j].FastFlux[1];
                    cells[i + 1, j].SlowFlux[1] = cells[i, j].SlowFlux[1];
                    if (i == 0)
                    {
                        cells[i, j].FastFlux[1] = 0;
                        cells[i, j].SlowFlux[1] = 0;
                    }
                }
            }
            //down flux
            for (int i = 0; i < 5; i++)
            {
                for (int j = 3; j >= 0; j--)
                {
                    cells[i, j + 1].FastFlux[2] = cells[i, j].FastFlux[2];
                    cells[i, j + 1].SlowFlux[2] = cells[i, j].SlowFlux[2];
                    if (j == 0)
                    {
                        cells[i, j].FastFlux[2] = 0;
                        cells[i, j].SlowFlux[2] = 0;
                    }
                }
            }
            //left flux
            for (int i = 1; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    cells[i-1, j].FastFlux[3] = cells[i, j].FastFlux[3];
                    cells[i-1, j].SlowFlux[3] = cells[i, j].SlowFlux[3];
                    if (i == 4)
                    {
                        cells[i, j].FastFlux[3] = 0;
                        cells[i, j].SlowFlux[3] = 0;
                    }
                }
            }
        }
        void updateCriticality()
        {
            foreach (var cell in cells)
            {
                cell.CellDelayedCriticality = cell.CellDelayedAbsorb / cell.CellEmitted;
                cell.CellPromptCriticality = cell.CellPromptAbsorb / cell.CellEmitted;
            }
        }

        void updateXenon(double deltaT)
        {
            foreach (var cell in cells)
            {
                var amountConvertedToXenon = cell.PreXenon * deltaT / constants.PreXenonDecayTimeConstant;
                cell.PreXenon -= amountConvertedToXenon;
                cell.Xenon += amountConvertedToXenon;
                var amountDecayedFromXenon = cell.Xenon * deltaT / constants.XenonDecayTimeConstant;
                cell.Xenon -= amountDecayedFromXenon;
            }
        }

        void transferTemp(double deltaT)
        {
            foreach (var cell in cells)
            {
                var transferEnergy = (cell.Temp - cell.WaterTemp) * constants.FeedwaterThermalTransferCoefficient;
                cell.Temp -= transferEnergy / constants.ReactorThermalCapacity;
                cell.WaterTemp += transferEnergy / (constants.WaterPerCell * constants.WaterThermalCapacity) * 1000000;
            }
        }


        void waterFlowSim(double deltaT)
        {
            foreach (var cell in cells)
            {
                var boilingPoint = 100 * Math.Pow(_InletPressure, 0.2432);
                cell.SteamPercent = Math.Max(Math.Min((cell.WaterTemp - boilingPoint)/2, 100), 0);
                cell.WaterResistance = 0.005 + 0.04*cell.SteamPercent/100;
            }
            
            double sumFlow = 0;
            double sumWeightedTemp = 0;
            for (int i = 0; i<5; i++)
            {
                double sumResistance = 0;
                for (int j = 0; j < 5; j++)
                {
                    sumResistance += cells[i, j].WaterResistance;
                }
                var flow = _InletPressure / sumResistance;
                sumFlow += flow;
                for (int j = 0; j < 5; j++)
                {
                    cells[i,j].WaterFlow = flow;
                    if (j == 0)
                    {
                        sumWeightedTemp += cells[i, j].WaterTemp * flow;
                    }
                    if (j < 4)
                    {
                        cells[i, j].WaterTemp = cells[i, j].WaterTemp * (1 - flow / constants.WaterPerCell * deltaT) + cells[i, j + 1].WaterTemp * (flow / constants.WaterPerCell * deltaT);
                    }
                    if (j == 4)
                    {
                        cells[i, j].WaterTemp = cells[i, j].WaterTemp * (1 - flow / constants.WaterPerCell * deltaT) + _InletTemp * (flow / constants.WaterPerCell * deltaT);
                    }
                }
            }
            _InletFlow = sumFlow;
            _OutletFlow = sumFlow;
            _OutletTemp = sumWeightedTemp / sumFlow;

            
        }
        void powerEstimate()
        {
            var estimateFlux = (cells[1, 1].DelayedRate + cells[1, 1].PromptRate + cells[1, 3].DelayedRate + cells[1, 3].PromptRate + cells[3, 1].DelayedRate + cells[3, 1].PromptRate + cells[3, 3].DelayedRate + cells[3, 3].PromptRate) / 4;
            _PowerLevel = estimateFlux * 25 * constants.ReactivityThermalGenerationCoefficient;
        }

    }
}
