﻿namespace Cicm.Database.Models
{
    public class MemoryByMachine
    {
        public int     Machine { get; set; }
        public int     Type    { get; set; }
        public int     Usage   { get; set; }
        public long?   Size    { get; set; }
        public double? Speed   { get; set; }
        public long    Id      { get; set; }

        public Machines MachineNavigation { get; set; }
    }
}