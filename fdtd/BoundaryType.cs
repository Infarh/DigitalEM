namespace EM.FDTD
{
    /// <summary>“ип граничных условий</summary>
    public enum BoundaryType : byte
    {
        /// <summary>Ёлектрическа€ стенка (Et = 0)</summary>
        PMC = 0,
        /// <summary>јбсолютно поглащающа€ стенка дл€ пол€ E</summary>
        ABC_E = 1,
        /// <summary>јбсолютно поглащающа€ стенка дл€ пол€ H</summary>
        ABC_H = 2,
        /// <summary>»деально-согласованные слои</summary>
        PML = 3,
        Periodic = 4
    }
}