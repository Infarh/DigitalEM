using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using MathService;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using WPFExtentions.ViewModels;

namespace EM
{
    [MarkupExtensionReturnType(typeof(MainModel))]
    public class MainModel : ViewModel
    {
        private string f_WindowTitle = "FDTD";
        private int f_SpaceSize = 200;
        private Complex[] f_Impedance = new Complex[200];
        private double f_SpaceStep = 1;
        private double f_dt = 1;
        private double f_Tmax = 250;

        public string WindowTitle { get { return f_WindowTitle; } set { Set(ref f_WindowTitle, value); } }

        public int SpaceSize
        {
            get { return f_SpaceSize; }
            set
            {
                if(!Set(ref f_SpaceSize, value, size => size > 2, "size > 2")) return;
                f_Impedance = new Complex[value].Initialize(i => 377);
                OnPropertyChanged(nameof(SpaceLength));
            }
        }

        public double SpaceStep
        {
            get { return f_SpaceStep; }
            set
            {
                if(!Set(ref f_SpaceStep, value, dx => dx > 0, "dx > 0")) return;
                OnPropertyChanged(nameof(SpaceLength));
            }
        }

        public double SpaceLength => f_SpaceStep * f_Impedance.Length;

        public double dt
        {
            get { return f_dt; }
            set
            {
                if(!Set(ref f_dt, value, dt => dt > 0, "dt > 0")) return;
                OnPropertyChanged(nameof(TimeIterationsCount));
            }
        }

        public double Tmax
        {
            get { return f_Tmax; }
            set
            {
                if(!Set(ref f_Tmax, value, T => T > 0, "Tmax > 0")) return;
                OnPropertyChanged(nameof(TimeIterationsCount));
            }
        }

        public int TimeIterationsCount => (int)(f_Tmax / f_dt);

        public PlotModel FieldMap
        {
            get
            {
                var model = new PlotModel();

                model.Axes.Add(new LinearColorAxis
                {
                    Position = AxisPosition.Right,
                    Palette = OxyPalettes.Jet(500),
                    HighColor = OxyColors.Red,
                    LowColor = OxyColors.Blue
                });

                var data = Field;
                model.Series.Add(new HeatMapSeries { X0 = -5, X1 = 5, Y0 = -5, Y1 = 5, Data = data });
                model.Series.Add(new ScatterSeries { });

                return model;
            }
        }

        public double[,] Field => new double[100, 100].Initialize((i, j) => 100 * Sinc((i - 50) / 10d) * Sinc((j - 50) / 10d));

        private static double Sinc(double x)
        {
            if(Math.Abs(x) < 0.1) return 1;
            var pix = Math.PI * x;
            return Math.Sin(pix) / pix;
        }

        public double[] Levels => new double[] { -100, -80, -60, -40, -20, 0, 20, 40, 60, 80, 100 };
    }
}
