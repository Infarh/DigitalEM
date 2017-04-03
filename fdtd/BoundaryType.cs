namespace EM.FDTD
{
    /// <summary>“ип граничных условий</summary>
    public enum BoundaryType : byte
    {
        /// <summary>Ёлектрическа€ стенка (Et = 0)</summary>
        PMC = 0,
        /// <summary>јбсолютно поглащающа€ стенка</summary>
        ABC = 1,

    }
}