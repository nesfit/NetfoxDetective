﻿using System;

namespace EntityFramework.BulkInsert.Test.CodeFirst.Domain
{
    public class MeteringPoint : Entity, ICreatedAt, IModifiedAt
    {
        public string EIC { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}