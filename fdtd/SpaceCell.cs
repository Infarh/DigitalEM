using MathService.Vectors;

namespace EM.FDTD
{
    public struct SpaceCell
    {
        public readonly double eps;
        public readonly double mu;
        public readonly double sgm;
        public readonly Vector3D d;

        public SpaceCell(Vector3D d, double eps, double mu = 1, double sgm = 0)
        {
            this.d = d;
            this.eps = eps;
            this.mu = mu;
            this.sgm = sgm;
        }

        public SpaceCell_time GetTimeCell(double dt) => new SpaceCell_time(d.X, d.Y, d.Z, dt, eps, mu, sgm);
    }

    public struct SpaceCell_time
    {
        public double Ex, Ey, Ez;
        public double Hx, Hy, Hz;
        public readonly double kE;
        public readonly double kE_dx;
        public readonly double kE_dy;
        public readonly double kE_dz;
        public readonly double kH;
        public readonly double kH_dx;
        public readonly double kH_dy;
        public readonly double kH_dz;

        public SpaceCell_time(double dx, double dy, double dz, double dt, double eps, double mu = 1, double sgm = 0)
        {
            Ex = Ey = Ez = 0;
            Hx = Hy = Hz = 0;

            var dt_eps = dt / eps;
            var dt_mu = dt / mu;
            double k_E, k_H;
            if(sgm.Equals(0))
                kE = kH = k_E = k_H = 1;
            else
            {
                kE = (1 - sgm / 2 * dt_eps) / (k_E = (1 + sgm / 2 * dt_eps));
                kH = (1 - sgm / 2 * dt_mu) / (k_H = (1 + sgm / 2 * dt_mu));
            }

            SetValue(out kE_dx, out kH_dx, dx, k_E, k_H, dt_eps, dt_mu);
            SetValue(out kE_dy, out kH_dy, dy, k_E, k_H, dt_eps, dt_mu);
            SetValue(out kE_dz, out kH_dz, dz, k_E, k_H, dt_eps, dt_mu);
        }

        private static void SetValue(out double E, out double H, double d, double ke, double kh, double e, double m)
        {
            if(double.IsInfinity(d) || d.Equals(0) || double.IsNaN(d))
            {
                E = H = 0;
                return;
            }
            E = e / d / ke;
            H = m / d / kh;
        }

        public double GetEx(double Exdt, double dHy_dz, double dHz_dy) => kE * Exdt + kE_dy * dHz_dy - kE_dz * dHy_dz;
        public double GetEy(double Eydt, double dHx_dz, double dHz_dx) => kE * Eydt + kE_dz * dHx_dz - kE_dx * dHz_dx;
        public double GetEx(double Exdt, double dHz_dy) => kE * Exdt + kE_dy * dHz_dy;
        public double GetEy(double Eydt, double dHz_dx) => kE * Eydt - kE_dx * dHz_dx;
        public double GetEz(double Ezdt, double dHy_dx, double dHx_dy) => kE * Ezdt + kE_dx * dHy_dx - kE_dy * dHx_dy;
        public double GetEz(double Ezdt, double dHy) => kE * Ezdt + kE_dx * dHy;

        public double GetHx(double Hxdt, double dEy_dz, double dEz_dy) => kH * Hxdt + kH_dz * dEy_dz - kH_dy * dEz_dy;
        public double GetHy(double Hydt, double dEz_dx, double dEx_dz) => kH * Hydt + kH_dx * dEz_dx - kH_dz * dEx_dz;
        public double GetHx(double Hxdt, double dEz_dy) => kH * Hxdt - kH_dy * dEz_dy;
        public double GetHy(double Hydt, double dEz_dx) => kH * Hydt + kH_dx * dEz_dx;
        public double GetHz(double Hzdt, double dEy_dx, double dEx_dy) => kH * Hzdt + kH_dy * dEx_dy - kH_dx * dEy_dx;
        public double GetHz(double Hzdt, double dEy_dx) => kH * Hzdt - kH_dx * dEy_dx;
    }
}