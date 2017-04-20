namespace EM.FDTD
{
    /// <summary>��� ��������� �������</summary>
    public enum BoundaryType : byte
    {
        /// <summary>������������� ������ (Et = 0)</summary>
        PMC = 0,
        /// <summary>��������� ����������� ������ ��� ���� E</summary>
        ABC_E = 1,
        /// <summary>��������� ����������� ������ ��� ���� H</summary>
        ABC_H = 2,
        /// <summary>��������-������������� ����</summary>
        PML = 3,
        Periodic = 4
    }
}