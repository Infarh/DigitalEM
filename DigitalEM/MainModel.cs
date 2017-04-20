using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using EM.FDTD;
using MathService;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using WPFExtentions.Commands;
using WPFExtentions.ViewModels;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleAnonymousFunction

namespace EM
{
    [MarkupExtensionReturnType(typeof(MainModel))]
    public class MainModel : ViewModel
    {
        private string f_WindowTitle = "FDTD";
        private int f_SpaceSize = 200;
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

        public double SpaceLength => f_SpaceStep * f_SpaceSize;

        public double dt
        {
            get { return f_dt; }
            set
            {
                if(!Set(ref f_dt, value, t => t > 0, "dt > 0")) return;
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


        public double[,] Field => new double[100, 100].Initialize((i, j) => 100 * Sinc((i - 50) / 10d) * Sinc((j - 50) / 10d));

        private static double Sinc(double x)
        {
            if(Math.Abs(x) < 0.1) return 1;
            var pix = Math.PI * x;
            return Math.Sin(pix) / pix;
        }

        public double[] Levels => new double[] { -100, -80, -60, -40, -20, 0, 20, 40, 60, 80, 100 };

        public LamdaCommand StartModelingCommand { get; }

        public MainModel()
        {
            StartModelingCommand = new LamdaCommand(StartModelingCommand_ExecutedAsync);
        }

        private async void StartModelingCommand_ExecutedAsync() 
        {
            StartModelingCommand.IsCanExecute = false;
            await ProcessFieldsAsync().ConfigureAwait(true);
            StartModelingCommand.IsCanExecute = true;
        }

        private async Task ProcessFieldsAsync()
        {
            var solver = new Solver1D(
                SpaceCharacteristic.Create(f_SpaceSize), 
                new Solver1D.Boundary(
                    Xmin: BoundaryType.PMC, 
                    Xmax: BoundaryType.PMC), 
                f_SpaceStep, 
                new[]
                {
                    new FieldSource(
                        position: 50, 
                        type: FieldSource.FieldTypes.E, 
                        orientation: FieldSource.SourceOrientation.Z, 
                        f: t => Math.Exp(- (t - 30) * (t - 30) / 100))
                });
            await Task.Run(() =>
            {
                while(solver.Time < f_Tmax)
                    solver.TimeStep(f_dt);
            }).ConfigureAwait(false);
        }
    }
}
