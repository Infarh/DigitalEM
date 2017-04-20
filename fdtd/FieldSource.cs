using System;

namespace EM.FDTD
{
    public class FieldSource
    {
        public enum FieldTypes : byte { E, H }

        public enum SourceOrientation : byte { X, Y, Z }

        public readonly int Position;
        public readonly FieldTypes FieldType;
        public readonly SourceOrientation Orientation;
        public readonly Func<double, double> Function;

        public FieldSource(int position, FieldTypes type, SourceOrientation orientation, Func<double, double> f)
        {
            Function = f;
            Position = position;
            FieldType = type;
            Orientation = orientation;
        }

    }
}