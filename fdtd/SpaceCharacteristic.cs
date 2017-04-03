namespace EM.FDTD
{
    public struct SpaceCharacteristic
    {
        public readonly double eps;
        public readonly double mu;
        public readonly double sigma;

        public SpaceCharacteristic(double eps, double mu = 1, double sigma = 0)
        {
            this.eps = eps;
            this.mu = mu;
            this.sigma = sigma;
        }
    }
}