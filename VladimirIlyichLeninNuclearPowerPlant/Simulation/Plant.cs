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
        IGameConstants constants;

        public Plant(List<ControlRod> controlRods, IGameConstants constants)
        {
            this.constants = constants;
            core = new Core(controlRods, constants);
        }

        public void update(GameTime gameTime)
        {
            var deltaT = gameTime.ElapsedGameTime.TotalSeconds;
            core.update(deltaT);
        }
    }
}
