using System;
using System.Collections.Generic;
using System.Text;
using ANT.Core;
using Microsoft.Extensions.DependencyInjection;
using Shiny;
using Shiny.Notifications;

namespace ANT.ShinyStart
{
    public class StartUp : Shiny.ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.UseNotifications(
                requestPermissionImmediately: true,
                notificationCategories: new NotificationCategory("Today Animes")
                );
        }
    }
}
