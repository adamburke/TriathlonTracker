using System;
using System.Threading.Tasks;
using System.Net.Http;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Xunit;

namespace TriathlonTracker.PerformanceTests;

public class PerformanceTests
{
    [Fact]
    public void HomePage_PerformanceTest()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("home_page_scenario", async context =>
        {
            var request = Http.CreateRequest("GET", "http://localhost:5217/")
                .WithHeader("Accept", "text/html");

            var response = await Http.Send(httpClient, request);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(2))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 10, during: TimeSpan.FromSeconds(10))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}