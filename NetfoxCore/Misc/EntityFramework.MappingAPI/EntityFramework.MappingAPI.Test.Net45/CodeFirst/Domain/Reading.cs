﻿using System;

namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public class Reading : Entity
    {
        public int MeteringPointId { get; set; }
        public virtual MeteringPoint MeteringPoint { get; set; }
        public DateTime Date { get; set; }
        public decimal? Consumed { get; set; }
        public decimal? Produced { get; set; }
    }
}