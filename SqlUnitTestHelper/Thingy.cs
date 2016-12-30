using System;

namespace SqlUnitTestHelper
{
    public enum ThingyStatus
    {
        Cow,
        Fern,
        Pillow,
        Planet,
        Sherbert,
        Tank,
        Unknown
    }

    public class Thingy
    {
        public int PrimaryKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public ThingyStatus Status { get; set; }
    }
}