using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MathService;
using MathService.ViewModels;

namespace EM.FDTD
{
    /// <summary>Одномерный вычислитель FDTD</summary>
    public class Solver1D
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
        private readonly FieldSource[] f_SrcE;
        private readonly FieldSource[] f_SrcH;

        public double Time => f_Time;

        public double[] Ey => f_Ey;
        public double[] Ez => f_Ez;
        public double[] Hy => f_Ey;
        public double[] Hz => f_Ez;

        public SpaceCharacteristic[] Space => f_Space;

        public Boundary BoundaryConditions => f_Boundary;

        public Solver1D(SpaceCharacteristic[] space, Boundary boundary, double dx, FieldSource[] sources = null, double StartTime = 0)
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

            if(sources == null || sources.Length == 0) return;
            var src_e = new List<FieldSource>(sources.Length);
            var src_h = new List<FieldSource>(sources.Length);

            for(var i = 0; i < sources.Length; i++)
            {
                var src = sources[i];
                if(src.Orientation != FieldSource.SourceOrientation.Y && src.Orientation != FieldSource.SourceOrientation.Z) continue;
                if(src.FieldType == FieldSource.FieldTypes.E)
                    src_e.Add(src);
                else
                    src_h.Add(src);
            }

            f_SrcE = src_e.ToArray();
            f_SrcH = src_h.ToArray();

        }

        public void ResetFields()
        {
            for (var i = 0; i < f_SpaceLength; i++)
            {
                f_Ey[i] = 0;
                f_Ez[i] = 0;
                f_Hy[i] = 0;
                f_Hz[i] = 0;
            }
            f_Time = 0;
        }

        public void TimeStep() => TimeStep(f_StableTimeStep);
        public void TimeStep(double dt)
        {
            f_Time += dt;
            Process_H_Boundary();
            Process_H_Field(dt);
            if (f_SrcH != null) ProcessSources(f_Time, f_SrcH, f_Hy, f_Hz); //Process_H_Sources(f_Time);
            Process_E_Boundary();
            Process_E_Field(dt);
            if(f_SrcE != null) ProcessSources(f_Time, f_SrcE, f_Ey, f_Ez); //Process_E_Sources(f_Time);
        }

        private void ProcessSources(double t, FieldSource[] sources, double[] FieldY, double[] FieldZ)
        {
            for(var i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                if(source.Orientation == FieldSource.SourceOrientation.Y)
                    FieldY[source.Position] = source.Function(t);
                else
                    FieldZ[source.Position] = source.Function(t);
            }
        }

        private void Process_H_Sources(double t)
        {
            for(var i=  0; i < f_SrcH.Length; i++)
            {
                var h_source = f_SrcH[i];
                if (h_source.Orientation == FieldSource.SourceOrientation.Y)
                    f_Hy[h_source.Position] = h_source.Function(t);
                else
                    f_Hz[h_source.Position] = h_source.Function(t);
            }
        }

        private void Process_E_Sources(double t)    
        {
            for(var i = 0; i < f_SrcE.Length; i++)
            {
                var e_source = f_SrcE[i];
                if(e_source.Orientation == FieldSource.SourceOrientation.Y)
                    f_Ey[e_source.Position] = e_source.Function(t);
                else
                    f_Ez[e_source.Position] = e_source.Function(t);
            }
        }

        private void Process_E_Boundary()
        {
            switch(f_Boundary.Xmin)
            {
                case BoundaryType.ABC_E:
                    f_Ey[0] = f_Ey[1];
                    f_Ez[0] = f_Ez[1];
                    break;
                case BoundaryType.ABC_H:
                    f_Hy[0] = f_Hy[1];
                    f_Hz[0] = f_Hz[1];
                    break;
            }
        }

        private void Process_E_Field(double dt)
        {
            var dt05 = dt;
            for(var x = 1; x < f_SpaceLength; x++)
            {
                var eps = f_Space[x].eps;
                var sgm_dt_05 = f_Space[x].sgm * dt05;
                var s05_eps = sgm_dt_05 / eps;
                var kE = 1 - s05_eps;
                var K = 1 + s05_eps;
                var kH = dt / eps / f_dx;

                f_Ey[x] = (kE * f_Ey[x] + kH * (f_Hz[x - 1] - f_Hz[x])) / K;
                f_Ez[x] = (kE * f_Ez[x] + kH * (f_Hy[x] - f_Hy[x - 1])) / K;
            }
        }

        private void Process_H_Boundary()
        {
            switch(f_Boundary.Xmax)
            {
                case BoundaryType.ABC_E:
                    f_Ey[f_SpaceLength - 1] = f_Ey[f_SpaceLength - 2];
                    f_Ez[f_SpaceLength - 1] = f_Ez[f_SpaceLength - 2];
                    break;
                case BoundaryType.ABC_H:
                    f_Hy[f_SpaceLength - 1] = f_Hy[f_SpaceLength - 2];
                    f_Hz[f_SpaceLength - 1] = f_Hz[f_SpaceLength - 2];
                    break;
            }
        }

        private void Process_H_Field(double dt)
        {
            var dt05 = dt;
            for(var x = 0; x < f_SpaceLength - 1; x++)
            {
                var mu = f_Space[x].mu;
                var sgm_dt_05 = f_Space[x].sgm * dt05;
                var s05_mu = sgm_dt_05 / mu;
                var kH = 1 - s05_mu;
                var K = 1 + s05_mu;
                var kE = dt / mu / f_dx;

                f_Hy[x] = (kH * f_Hy[x] + kE * (f_Ez[x + 1] - f_Ez[x])) / K;
                f_Hz[x] = (kH * f_Hz[x] + kE * (f_Ey[x] - f_Ey[x + 1])) / K;
            }
        }
    }
}
