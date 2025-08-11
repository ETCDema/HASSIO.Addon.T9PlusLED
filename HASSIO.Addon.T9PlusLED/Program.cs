using System;
using System.IO;
using System.Threading.Tasks;

using HASSIO.Addon.T9PlusLED.Services;
using HASSIO.Supervisor.API;
using HASSIO.Supervisor.API.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HASSIO.Addon.T9PlusLED
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var env             = Environment.GetEnvironmentVariable("DOTNET_"+HostDefaults.EnvironmentKey.ToUpper())
								?? Environments.Production;
			var appCfgBuilder   = new ConfigurationBuilder();
			if (env==Environments.Production)
			{
				appCfgBuilder.AddJsonFile("/data/options.json", false, false);
			} else
			{
				appCfgBuilder
					.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "../../../dev-options.json"), false, false)
					.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "../../../dev-options.secret.json"), false, false);
			}

			var appCfg          = appCfgBuilder
									.AddEnvironmentVariables()
									.AddCommandLine(args)
									.Build();

			var logLevel        = appCfg.GetValue("LogLevel", LogLevel.Information);

			var host			= Host.CreateDefaultBuilder(args)
									.ConfigureAppConfiguration(cfg =>
									{
										cfg.AddConfiguration(appCfg);
									})
									.ConfigureLogging(log =>
									{
										log.ClearProviders();
										log.AddConsole();
										log.SetMinimumLevel(logLevel);
									})
									.ConfigureServices((ctx, services) =>
									{
										services
											.AddHASSIOSuperviserAPI(appCfg.GetValue("SUPERVISOR_TOKEN", appCfg.GetValue("API:longLivedAccessToken", default(string)!)),
																	appCfg.GetValue("API:endpoint",     default(string)))
											.AddSingleton<IDeviceService>(sp => new T9PlusLEDService(
																					sp.GetRequiredService<ILogger<T9PlusLEDService>>(),
																					appCfg.GetValue("T9PlusLED:port",				default(string)!), 
																					appCfg.GetValue("T9PlusLED:modeEntityID",		"input_select.server_led_mode"), 
																					appCfg.GetValue("T9PlusLED:brightnessEntityID",	"input_number.server_led_brightness"),
																					appCfg.GetValue("T9PlusLED:speedEntityID",		"input_number.server_led_speed")));
									})
									.Build();

			if (logLevel==LogLevel.Trace)
			{
				var log         = host.Services.GetService<ILogger<Program>>();
				log?.LogInformation(appCfg.GetDebugView());
			}

			await host.RunHASSIOSupervisorCoreAsync();
		}
	}
}
