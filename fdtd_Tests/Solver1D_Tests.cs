using System;
using MathService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EM.FDTD.Tests
{
    [TestClass]
    public class Solver1D_Tests
    {
        [TestMethod]
        public void Ez_Calculation_Test()
        {
            const int Nx = 200;
            const int Nt = 250;
            var f0 = 0.1;       // 0.1 Гц
            var T0 = 1 / f0;    //  10 с
            var dt = T0 / 10;   //   1 с
            var Tmax = Nt * dt; // 250 с  
        }
    }
}
