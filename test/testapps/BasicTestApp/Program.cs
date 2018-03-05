// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.AspNetCore.Blazor.Browser.Services.Temporary;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BasicTestApp.Groups;

namespace BasicTestApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Signal to tests that we're ready
            RegisteredFunction.Invoke<object>("testReady");
        }

        public static void MountTestComponent(string componentTypeName)
        {
            var componentType = Type.GetType(componentTypeName);
            
            var browserServiceProvider = new BrowserServiceProvider(
                services =>
                {
                    services.AddSingleton<IQueryGateway<Group>, Groups.GroupsQuery>();
                    services.AddSingleton(new GatewaysConfiguration());
                });

            new BrowserRenderer(browserServiceProvider).AddComponent(componentType, "app");
        }
    }

     public class GatewaysConfiguration
    {
        public string GroupsBaseUrl { get { return "api url"; } }
    }
}
