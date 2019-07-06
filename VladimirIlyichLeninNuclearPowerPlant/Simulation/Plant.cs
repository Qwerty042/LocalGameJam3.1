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

        public Plant(List<ControlRod> controlRods)
        {
            core = new Core(controlRods);
        }

        public void update(GameTime gameTime)
        {
            var deltaT = gameTime.ElapsedGameTime.TotalSeconds;
            core.update(deltaT);
        }
    }
}
