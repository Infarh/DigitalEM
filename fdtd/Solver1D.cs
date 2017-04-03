using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MathService;
using MathService.ViewModels;

namespace EM.FDTD
{
    /// <summary>Одномерный вычислитель FDTD</summary>
    public class Solver1D : ViewModel
    {
        /// <summary>Граничные условия пространства</summary>
        public struct Boundary
        {
            /// <summary>Граничные условия в направлении максимума оси OX (+OX)</summary>
            public BoundaryType Xmin;
            /// <summary>Граничные условия в направлении минимума оси OX (-OX)</summary>
            public BoundaryType Xmax;

            /// <summary>Инициализация новых граничных условий вдоль оси OX</summary>
            /// <param name="Xmin">Граничные условия в направлении максимума оси OX (+OX)</param>
            /// <param name="Xmax">Граничные условия в направлении минимума оси OX (-OX)</param>
            public Boundary(BoundaryType Xmin, BoundaryType Xmax)
            {
                this.Xmin = Xmin;
                this.Xmax = Xmax;
            }
        }

        private readonly int f_SpaceLength;
        private readonly SpaceCharacteristic[] f_Space;
        private readonly double[] f_Ey;
        private readonly double[] f_Ez;
        private readonly double[] f_Hy;
        private readonly double[] f_Hz;
        private readonly Boundary f_Boundary;
        private readonly double f_dx;
        private double f_Time;
        private readonly double f_StableTimeStep;

        //private List<>

        public Solver1D(SpaceCharacteristic[] space, Boundary boundary, double dx, double StartTime = 0)
        {
            f_SpaceLength = (f_Space = space).Length;
            f_Ey = new double[f_SpaceLength];
            f_Ez = new double[f_SpaceLength];
            f_Hy = new double[f_SpaceLength];
            f_Hz = new double[f_SpaceLength];

            f_Boundary = boundary;
            f_dx = dx;
            f_StableTimeStep = dx / Consts.SpeedOfLightInVacuum;
            f_Time = StartTime;
        }

        public void TimeStep() => TimeStep(f_StableTimeStep);
        public void TimeStep(double dt)
        {
            double s;
            if(dt <= 0 || dt.Equals(f_StableTimeStep))
            {
                f_Time += f_StableTimeStep;
                s = 1;
            }
            else
            {
                f_Time += dt;
                s = dt / f_StableTimeStep;
            }
            Process_H_Boundary();
            Process_H_Field(s);
            Process_E_Boundary();
            Process_E_Field(s);
        }

        private void Process_E_Boundary()
        {
            switch (f_Boundary.Xmin)
            {
                case BoundaryType.ABC:
                    f_Ey[0] = f_Ey[1];
                    f_Ez[0] = f_Ez[1];
                    break;
            }
        }

        private void Process_E_Field(double S)
        {
            for(var x = 1; x < f_SpaceLength; x++)
            {
                var e = Consts.ElectroDynamic.Impedance0 / f_Space[x].eps;
                f_Ey[x] = f_Ey[x] + (f_Hz[x - 1] - f_Hz[x]) * e * S;
                f_Ez[x] = f_Ez[x] + (f_Hy[x] - f_Hy[x - 1]) * e * S;
            }
        }

        private void Process_H_Boundary()
        {
            switch(f_Boundary.Xmax)
            {
                case BoundaryType.ABC:
                    f_Hy[f_SpaceLength - 1] = f_Hy[f_SpaceLength - 2];
                    f_Hz[f_SpaceLength - 1] = f_Hz[f_SpaceLength - 2];
                    break;
            }
        }

        private void Process_H_Field(double S)
        {
            for(var x = 0; x < f_SpaceLength - 1; x++)
            {
                var m = Consts.ElectroDynamic.Impedance0 * f_Space[x].mu;
                f_Hy[x] = f_Hy[x] + (f_Ez[x + 1] - f_Ez[x]) * S / m;
                f_Hz[x] = f_Hz[x] + (f_Ey[x] - f_Ey[x + 1]) * S / m;
            }
        }
    }
}
