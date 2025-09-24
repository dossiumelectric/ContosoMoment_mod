﻿using ContosoMoments.Common;

namespace ContosoMoments.Api
{
    public class ConfigModel
    {
        public string DefaultAlbumId
        {
            get { return AppSettings.DefaultAlbumId; }
        }
        public string DefaultUserId
        {
            get { return AppSettings.DefaultUserId; }
        }

        public string UploadContainerName
        {
            get { return AppSettings.UploadContainerName; }
        }

        public string DefaultServiceUrl
        {
            get { return AppSettings.DefaultServiceUrl.ToLower(); }
        }
    }
}