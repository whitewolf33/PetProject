using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetProject.BusinessLayer.Configurations;
using PetProject.BusinessLayer.Interfaces;
using PetProject.BusinessLayer.Services;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PetProject
{
    class Program
    {
        private static ServiceProvider serviceProvider;
        static void Main(string[] args)
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                serviceProvider = serviceCollection.BuildServiceProvider();

                var appLogger = serviceProvider.GetRequiredService<ILoggerFactory>()
                                .CreateLogger<Program>();
                appLogger.LogDebug("Starting application");

                Console.WriteLine("Welcome to Pet Project");
                Console.WriteLine("========================");

                MainMenu();

            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"Oops! An error occured. Details...{Environment.NewLine} {ex.Message}");
#else
                Console.WriteLine($"Oops! An error occured. Please try launching the app again");
#endif
                Console.ReadLine();
            }
        }

        private static void MainMenu()
        {
            bool loop = true;

            while (loop)
            {
                Console.WriteLine("Please choose from the following menu:");
                Console.WriteLine("1. Display Cats under their Owner's Gender");               
                Console.WriteLine("2. Exit program");

                Console.Write($"{Environment.NewLine}Enter the number to proceed: ");
                var key = Console.ReadLine();
                Console.WriteLine();
                switch (key.Trim().ToLower())
                {
                    case "1":
                        {
                            DisplayCatsUnderOwnersGender();
                        }
                        break;                  
                    case "2":
                        {
                            //break the circuit                            
                            Func<Task> displayMessage = () => Task.Run(() =>
                             {
                                 Console.WriteLine("Thank you for using Pet Project. This window will now close.");
                                 loop = false;
                             });
                            Func<Task> userFeedbackDelay = () => Task.Delay(TimeSpan.FromSeconds(3));
                            var actions = new[] { displayMessage, userFeedbackDelay };
                            Task.WaitAll(actions.Select(a => a.Invoke()).ToArray());
                        }
                        break;
                }
            }
        }

        private static void DisplayCatsUnderOwnersGender()
        {
            var petService = serviceProvider.GetService<IPetService>();
            if (petService == null)
            {
                throw new InvalidOperationException("Error initializing pet Service");
            }
          
            var fetchPetsCompleted = false;
            Func<Task> fetchPetsTask = async () =>
            {
                try
                {
                    var listOfPetOwnersAndPets = await petService.GetPetOwnersAsync();
                    if (listOfPetOwnersAndPets != null && listOfPetOwnersAndPets.Success)
                    {
                        var petsList = await petService.GetPetsByOwnerGender(listOfPetOwnersAndPets.Result, BusinessLayer.Models.PetType.Cat);
                        DisplayGroupedResults(petsList);
                    }
                    else
                    {
                        Console.WriteLine(listOfPetOwnersAndPets != null ? listOfPetOwnersAndPets.ErrorMessage :
                            BusinessLayer.Models.StandardResponse<string>.GenericError().ErrorMessage);
                    }
                }
                finally
                {
                    fetchPetsCompleted = true;
                }
            };

            Func<Task> userFeedback = () => Task.Run(() =>
            {
                while (!fetchPetsCompleted)
                {
                    Task.Delay(TimeSpan.FromMilliseconds(500));
                }
                Console.WriteLine();
                Console.WriteLine("Task completed. Returning to Main Menu");
                Console.WriteLine();
            });

            var actions = new[] { fetchPetsTask, userFeedback };
            Task.WaitAll(actions.Select(a => a.Invoke()).ToArray());
        }

        static void ConfigureServices(IServiceCollection serviceCollection)
        {
            //Setup Logging
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.AddLogging(loggingBuilder => loggingBuilder
               .AddConsole()
               .SetMinimumLevel(LogLevel.Error)); //Change to Debug if you want more insights

            serviceCollection.AddSingleton(typeof(ILogger), typeof(Logger<Program>));

            //Setup Configuration to read AppSettings file
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            //Configure Options
            var petRepoSettings = configuration.GetSection(nameof(PetRepositorySetting));
            serviceCollection.AddOptions()
                   .Configure<PetRepositorySetting>(petRepoSettings);

            //HttpClient Setup
            serviceCollection.AddHttpClient<TypedHttpClient>("restService", client =>
            {
                //will setup base later to read from IOptions to cater for runtime changes
            })
            .ConfigureHttpMessageHandlerBuilder(config => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip //setup decompression as GZip
            })            
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2), (result, timeSpan, retryCount, context) =>
             {
                 var errorMsg = (result.Exception != null) ? result.Exception.Message : result.Result?.StatusCode.ToString();
                 Console.WriteLine($"Request failed with {errorMsg}.Waiting { timeSpan} before next retry.Retry attempt { retryCount} ");
             }));
            //Register Service instance types
            serviceCollection.AddTransient(typeof(IPetService), typeof(PetService));
        }

        private static void DisplayGroupedResults(List<IGroupable> resultList)
        {
            //Group By Gender to Print under headers
            var groupedByGender = resultList.GroupBy(x => x.Key);
            if (groupedByGender.Any())
            {
                foreach (var group in groupedByGender)
                {
                    Console.WriteLine();
                    Console.WriteLine(group.Key);
                    Console.WriteLine("*****************");
                    foreach (var entry in group.ToList().OrderBy(x => x.Name))
                    {
                        Console.WriteLine(entry);
                    }
                }
            }
            else
            {
                Console.WriteLine($"No results available to print");
            }
        }
    }
}