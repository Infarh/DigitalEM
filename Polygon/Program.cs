using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using EM.FDTD;
using MathService;
// ReSharper disable InconsistentNaming

namespace EM.Polygon
{
    static class Program
    {
        public static void Main(string[] args)
        {
            const int Nx = 200; // Число элементов пространства
            const int Nt = 400; // Число отсчётов по времени
            
            const double eps0 = Consts.ElectroDynamic.Epsilon0; // Ф/м
            const double mu0 = Consts.ElectroDynamic.Mu0; // Гн/м
            const double Z0_2 = mu0 / eps0;
            var imp0 = Consts.ElectroDynamic.Impedance0; // sqrt(mu0/eps0)
            var c = Consts.SpeedOfLightInVacuum; // 1/sqrt(mu0 * eps0)
            var c0 = 299863380.466127D;
            var eps = new double[Nx];
            var mu = new double[Nx];
            var sigma = new double[Nx];

            const double f0 = 1e9;       // 1   ГГц
            const double T0 = 1 / f0;    // 1   нс
            const double dt = T0 / 10;   // 0.1 нс

            for(var x = 0; x < Nx; x++)
            {
                eps[x] = 1;
                mu[x] = 1;
                sigma[x] = 0;
            }

            var Ez = new double[Nx];
            var Hy = new double[Nx];
            var e = new double[Nx];
            //Ez[1] = 1;

            using(var file = File.CreateText("sim.csw"))
                for(var t = 0; t < Nt; t++)
                {
                    if(t % 5 == 0)
                        file.WriteLine(Ez.Select(E => E.ToString(CultureInfo.CurrentCulture)).ToSeparatedStr(";"));
                    e[100] = Math.Exp(-(t - 30) * (t - 30) / 100d);
                    ProcessHy(Hy, Ez, mu);
                    ProcessEz(Ez, Hy, eps, e);
                }
        }

        private const double imp0 = 377;

        private static void ProcessHy(double[] Hy, double[] Ez, double[] mu)
        {
            for(var x = 0; x < Hy.Length - 1; x++)
                Hy[x] += (Ez[x + 1] - Ez[x]) / (imp0 * mu[x]);
        }

        private static void ProcessEz(double[] Ez, double[] Hy, double[] eps, double[] e)
        {
            for(var x = 1; x < Ez.Length; x++)
                Ez[x] += (Hy[x] - Hy[x - 1]) * (imp0 / eps[x]) + e[x];
        }

        private static readonly string sf_Separator = new string('-', Console.BufferWidth - 2);
        private static void PrintFields(int step, double[] E, double[] H, int N = 5)
        {
            Console.WriteLine(sf_Separator);
            Console.WriteLine($@"Step{step}");
            Console.WriteLine("E:" + string.Join(" ", E.Take(N).Select(e => e.ToString("e2")).ToArray()));
            Console.WriteLine("H:" + string.Join(" ", H.Take(N).Select(h => h.ToString("e2")).ToArray()));
        }
    }
}
