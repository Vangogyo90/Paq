﻿using System;
using System.Collections.Generic;

namespace Paq.Models
{
    public partial class PhotoNews
    {
        public int? IdPhotoNews { get; set; }
        public int? NewsId { get; set; }
        public byte[]? Photo { get; set; }
        public News? Newses { get; set; }
    }
}
