﻿using Microsoft.Azure.Mobile.Server;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoMoments.Common.Models
{
    public class Image : EntityData
    {
        public Image()
        {
            IsVisible = true;
        }

        public string UploadFormat { get; set; }

        [Column("Album_Id")]
        public string AlbumId { get; set; }

        public virtual Album Album { get; set; }

        [Column("User_Id")]
        public string UserId { get; set; }
        public User User { get; set; }

        public bool IsVisible { get; set; }
    }
}
