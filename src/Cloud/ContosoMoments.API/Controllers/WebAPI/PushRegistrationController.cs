﻿using ContosoMoments.Common;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace ContosoMoments.Api
{
    [MobileAppController]
    public class PushRegistrationController : ApiController
    {
        // GET api/PushRegistration
        public async Task Post([FromBody] DeviceInstallationInfo deviceInstallInfo)
        {
            bool isChanged = false;
            var ctx = new MobileServiceContext();
            var registration = ctx.DeviceRegistrations.Where(x => x.InstallationId == deviceInstallInfo.InstallationId);

            if (registration.Count() == 0)
            {
                NotificationPlatform? plat = await GetNotificationPlatform(deviceInstallInfo.InstallationId);

                if (null != plat)
                {
                    var newRegistration = new Common.Models.DeviceRegistration() {
                        Id = Guid.NewGuid().ToString(),
                                                                                         InstallationId = deviceInstallInfo.InstallationId,
                                                                                         UserId = deviceInstallInfo.UserId,
                        Platform = plat.Value
                    };

                    ctx.DeviceRegistrations.Add(newRegistration);
                    isChanged = true;
                }
            }
            else
            {
                var reg = registration.First();

                if (reg.UserId != deviceInstallInfo.UserId)
                {
                    reg.UserId = deviceInstallInfo.UserId;
                    isChanged = true;
                }
            }

            try
            {
                if (isChanged)
                    ctx.SaveChanges();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public async Task Delete([FromBody] DeviceInstallationInfo deviceInstallInfo)
        {
            var ctx = new MobileServiceContext();
            var registration = ctx.DeviceRegistrations.Where(x => x.InstallationId == deviceInstallInfo.InstallationId);

            if (registration.Count() > 0)
            {
                var reg = registration.First();

                await Notifier.Instance.RemoveRegistration(deviceInstallInfo.InstallationId);

                ctx.DeviceRegistrations.Remove(reg);

                try
                {
                    ctx.SaveChanges();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        private async Task<NotificationPlatform?> GetNotificationPlatform(string installationId)
        {
            var res = await Notifier.Instance.GetRegistration(installationId);

            if (null != res)
                return res.Platform;
            else
                return null;
        }
    }
}
