namespace EM.FDTD
{
    public struct SpaceCharacteristic
    {
        public static SpaceCharacteristic[] Create(int Length, double eps = 1, double mu = 1, double sgm = 0)
        {
            var space = new SpaceCharacteristic[Length];
            for(var i = 0; i < space.Length ; i++)
                space[i] = new SpaceCharacteristic(eps, mu, sgm);
            return space;
        }

        public readonly double eps;
        public readonly double mu;
        public readonly double sgm;

        public SpaceCharacteristic(double eps, double mu = 1, double Sgm = 0)
        {
            this.eps = eps;
            this.mu = mu;
            this.sgm = Sgm;
        }
    }
}