using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VladimirIlyichLeninNuclearPowerPlant.Simulation
{
    class Plant
    {
        public Core core { get; }
        public double pumpPressure { get; set; } = 50;
        public double seperatorTemp { get; set; } = 200;
        public double steamFlux { get; set; } = 0;//MW
        public double turbineEnergy { get; set; } = 0;//MJ
        public double turbineSpeed { get; set; } = 0;//percent
        public double turbineWastePower { get; set; } = 100;//MW
        public double turbineEfficiency { get; set; } = 0.35;//W/W
        public double turbinePower { get; set; } = 0;//MW

        IGameConstants constants;

        public Plant(List<ControlRod> controlRods, IGameConstants constants)
        {
            this.constants = constants;
            core = new Core(controlRods, constants);
        }

        public void update(GameTime gameTime)
        {
            var deltaT = gameTime.ElapsedGameTime.TotalSeconds;
            core.InletPressure = pumpPressure;
            core.InletTemp = seperatorTemp;
            if (deltaT != 0)
            {
                core.update(deltaT);
                updateSeperator(deltaT);
                steamProduced(deltaT);
                simulateTurbine(deltaT);
            }
        }

        void updateSeperator(double deltaT)
        {
            var addedMass = core.OutletFlow * deltaT;
            var maintainedMass = constants.WaterInSeperator - core.InletFlow * deltaT;
            seperatorTemp = (seperatorTemp * maintainedMass + core.OutletTemp * addedMass)/ constants.WaterInSeperator;
        }

        void steamProduced(double deltaT)
        {
            double boilingPoint = 200;
            steamFlux = Math.Max((seperatorTemp - boilingPoint) * constants.BoilRatePerDegree, 0);//MW
            double boilEnergyAmount = steamFlux * deltaT * 1000000;//J
            seperatorTemp -= boilEnergyAmount / constants.WaterThermalCapacity / constants.WaterInSeperator;

        }

        void simulateTurbine(double deltaT)
        {
            turbineEnergy += steamFlux * turbineEfficiency * deltaT;
            if(turbineEnergy <= turbineWastePower * deltaT)
            {
                turbineEnergy = 0;
            }
            turbineEnergy -= turbineWastePower * deltaT;

            if (turbineEnergy > constants.TurbineEnergy)
            {
                var over = turbineEnergy - constants.TurbineEnergy;
                turbineEnergy = constants.TurbineEnergy;
                turbinePower = over / deltaT;
            }
            turbineSpeed = turbineEnergy / constants.TurbineEnergy;
        }
    }
}
