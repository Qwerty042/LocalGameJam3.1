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

        private double _InletPressure = 10;//bar
        public double InletPressure { set { _InletPressure = value; } }

        public double InletFlow { get; }

        public double OutletTemp { get; }

        public double OutletFlow { get; }

        public double PowerLevel { get; }


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
                updateControlRodStatus();
                performNeutronSimulation(deltaT);
                updateXenon(deltaT);
                transferTemp(deltaT);
                waterFlowSim(deltaT);
            }
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
        }

        void emitNeutrons(double deltaT)
        {
            foreach(var cell in cells)
            {
                var emittedDelayedFlux = cell.DelayedFlux * deltaT / constants.DelayedCriticalityTimeConstant;
                cell.DelayedFlux -= emittedDelayedFlux;
                cell.DelayedRate = emittedDelayedFlux / deltaT;

                var totalFlux = constants.SpontaneousFlux * deltaT + emittedDelayedFlux + cell.PromptRate * deltaT;
                cell.PreXenon += totalFlux * constants.PreXenonProductionRateCoefficient;
                cell.Temp += totalFlux * constants.ReactivityThermalGenerationCoefficient;
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

                cell.PromptRate += absorbed / deltaT * (1 - constants.DelayedCriticalityProportion) * constants.FluxPerReaction;
                cell.DelayedFlux += absorbed * constants.DelayedCriticalityProportion * constants.FluxPerReaction;
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
                cell.WaterTemp += transferEnergy / constants.FeedwaterThermalCapacity;
            }
        }


        void waterFlowSim(double deltaT)
        {
            foreach (var cell in cells)
            {
                cell.WaterResistance = 0.1;
            }

            for(int i = 0; i<5; i++)
            {
                double sumResistance = 0;
                for (int j = 0; j < 5; j++)
                {
                    sumResistance += cells[i, j].WaterResistance;
                }
                var flow = _InletPressure / sumResistance;
                for (int j = 0; j < 5; j++)
                {
                    cells[i,j].WaterFlow = flow;
                }
            }


            foreach (var cell in cells)
            {
                var transferEnergy = (cell.Temp - cell.WaterTemp) * constants.FeedwaterThermalTransferCoefficient;
                cell.Temp -= transferEnergy / constants.ReactorThermalCapacity;
                cell.WaterTemp += transferEnergy / constants.FeedwaterThermalCapacity;
            }
        }

    }
}
