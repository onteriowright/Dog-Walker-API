﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkerAPI.Models
{
    public class Walks
    {

        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int WalkerId { get; set; }

        public int DogId { get; set; }

        public int Duration { get; set; }
    }
}
