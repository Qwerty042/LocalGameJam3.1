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

        private double _InletTemp;
        public double InletTemp { set { _InletTemp = value; } }

        private double _InletPressure;
        public double InletPressure { set { _InletPressure = value; } }

        public double InletFlow { get; }

        public double OutletTemp { get; }

        public double OutletFlow { get; }

        public double PowerLevel { get; }




        public Core(List<ControlRod> controlRods)
        {
            this.controlRods = controlRods;

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

            //determine how much of each control rod part is in each cell
            for (int i = 0; i < 1; i++)
            {
                //for each column/rod
                var controlRod = controlRods[i];
                for (int j = 0; j < 5; j++)
                {
                    cells[i, j].SteamPercent = 0;
                    //for each cell
                    var adaptedPercent = controlRod.insertedPercentage - 20*j + 80;
                    if(adaptedPercent > 100)
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
                        cells[i, j].WaterPercent = (1 - (adaptedPercent) / 20) * (100 - cells[i,j].SteamPercent);
                    }
                }
            }

        }


    }
}
