# Goal

Get metrics from the application and go through ways to analyze them

## References

https://azure.microsoft.com/en-us/services/application-insights/

https://docs.microsoft.com/en-us/azure/application-insights/app-insights-analytics

# Let's code!

## Add App Insights to the web application

Right click on the web project : add -> Application Insights Telemetry...

![img2][img2]

Click on Start Free button

![img1][img1]

Notice that the file ApplicationInsights folder has been added to the project.

![img3][img3]

Note in the appsettings.json file, you should find the a new section:

  ```json

  "ApplicationInsights": {
    "InstrumentationKey": "509d0f55-xxxx-4d24-xxxx-3d0847696552"
  }

  ```

## Track javascript calls

Note that in file `/Views/Shared/_Layout.cshtml` has been updated with two lines of code `@inject Microsoft.ApplicationInsights.AspNetCore.JavaScriptSnippet JavaScriptSnippet`  and `@Html.Raw(JavaScriptSnippet.FullScript)` which inlcudes javascript tracking code. 

```cshtml

@inject Microsoft.ApplicationInsights.AspNetCore.JavaScriptSnippet JavaScriptSnippet
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - ClasseAffaires Azure Bootcamp</title>

    <environment include="Development">
        <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.css" />
        <link rel="stylesheet" href="~/css/site.css" />
    </environment>
    <environment exclude="Development">
        <link rel="stylesheet" href="https://ajax.aspnetcdn.com/ajax/bootstrap/3.3.7/css/bootstrap.min.css"
              asp-fallback-href="~/lib/bootstrap/dist/css/bootstrap.min.css"
              asp-fallback-test-class="sr-only" asp-fallback-test-property="position" asp-fallback-test-value="absolute" />
        <link rel="stylesheet" href="~/css/site.min.css" asp-append-version="true" />
    </environment>
    @Html.Raw(JavaScriptSnippet.FullScript)
</head>
```
Take a look at `Program.cs` and it is already including `UseApplicationInsights()`. That means you can comment out the two lines in the `/Views/Shared/_Layout.cshtml`. 


Note that this code is available in the portal (App Insight -> Getting Started -> Monitor and diagnose client side application)

![img4][img4]

Now you can view the application in your favorite web browser. Open the developper console (F12) and you will notice calls to App Insights in the network tab.  You should see requests to `https://dc.services.visualstudio.com/v2/track` which is App Insights end point to collect data.

At this point, you should be able to see some data in Application Insights if you login in the Portal and look at the overview section.

## Tracking Unhandled Exceptions (request failures)

Now we will demonstrate how to track exceptions and how to log them ourselves (custom exception logging)

Add a new folder in the project `Services`
Now add a new cs file and call it `SomeService.cs`
In this file, copy and paste the definition of a custom excection `ServiceException` and the `SomeService` class:

```cs

    public class ServiceException : Exception
    {
        public ServiceException() : base("Service Exception") { }
        public ServiceException(Exception inner) : base("Service Exception", inner) { }
    }

    public class SomeService
    {
        public static void ThrowAnExceptionPlease()
        {
            throw new ServiceException();
        }
    }

```

Now add an empty controller to the web applicaiton and call it `ServiceController.cs`
In the `Index` method, add the following line of code:

```cs

        Services.SomeService.ThrowAnExceptionPlease();

```

Create the related view file  `views\service\index.cshtml` and add

```cshtml

@{
    ViewBag.Title = "ViewService";
}

<h2>Hello!!!!</h2>

```

now in the layout file `views\Shared\_Layout.cshtml`, after line 37, add;

```cshtml

<li>@Html.ActionLink("Service", "Index", "Service")</li>

```

Now run the application and click on the 'Service' link on the top. You should see and exception stack.

Back to visual studio, under the Solutuin Explorer -> Connected Services -> Application Insights folder, right click on `Application Insights folder' and choose 'Search Live Telemtry' you should see the Application Insight Search opens up. Choose 'Search debug session telemtry'. You should see failed request along with the details.

## Tracking Handled Exceptions

Back to the Index method of the `ServiceController`, replace the code of the method by;

```cs

            try
            {
                Services.SomeService.ThrowAnExceptionPlease();
            }
            catch(Exception ex)
            {
                var client = new TelemetryClient();
                client.TrackException(ex);
            }
            return View();

```

Add the using line to make it build:

```cs

using Microsoft.ApplicationInsights;

```

Run the application again and click on the `Service` menu item. You should not see the exception stack at this point.

On the Azure Portal, click on Metric Explorer and then add metrics to add the exceptions. We should be able to see the details of the exceptions.

![ExceptionMetric][ExceptionMetric]

## Explore Application Map to track dependencies

Application Insights will track dependencies calls by default, though you can explicitly track dependencies using the SDK.

First add the following line at the top of the SomeService.cs file:

```cs

    using Microsoft.ApplicationInsights;

```

add the following method to the SomeService class:

```cs

        public static void SomeWorkWithDependency()
        {
            var success = false;
            var telemetry = new TelemetryClient();            
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var dependency = new DependencyService();
                success = dependency.DoSomeWork();
            }
            finally
            {
                timer.Stop();
                telemetry.TrackDependency("DependencyService", "DoSomeWork", startTime, timer.Elapsed, success);
            }
        }

```

In the same file, add the following class:

```cs

    public class DependencyService
    {
        public bool DoSomeWork()
        {
            System.Threading.Thread.Sleep(5000);
            return true;
        }
    }

```

Now, change the code in the ServiceController file, Index method by replacing the call to `Services.SomeService.ThrowAnExceptionPlease();` by `Services.SomeService.SomeWorkWithDependency();`

Build and run the application and click on the Service menu item. You can also change the delay in the `DoSomeWork` method (make it very low for instance), rebuild and run again (do not forget to click on the Service menu item).

Go in the Portal and click on the App Insights instance then Application Map. You should be able to see the calls the newly created method.


## Track Event and Metrics

You can track many different things other then exceptions. Let's add some metric when we create new runners entry.

First, go in the SomeService class and add the following method:

```cs

    public static void TrackCustomStuff(int fivekmTime) {
        var telemetry = new TelemetryClient();
        var properties = new Dictionary<string, string> { { "Run", "RunName" }, { "Jog", "Race" } };
        var measurements = new Dictionary<string, double> { { "RunTime", fivekmTime }, { "Opponents", 1 } };
        telemetry.TrackEvent("RunCompleted", properties, measurements);
        telemetry.TrackMetric("RunTime", fivekmTime, properties);
    }

```

This method will help us to track a custom Event and Metric. Now let's add the call to that method in the RunnerPerformancesController in the Create Action. After adding the call the code should look like:

```cs

    public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,FivekmTime")] RunnerPerformance runnerPerformance)
        {
            if (ModelState.IsValid)
            {
                Services.SomeService.TrackCustomStuff(runnerPerformance.FivekmTime);
                _context.Add(runnerPerformance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(runnerPerformance);
        }

```

It's now time to test it. Deploy your application, and create a few new result.  Once your done, go back in the Azure portal, in the Application Insight blade, open a Metrics Explorer blade, add a new chart, and select Events

![ExceptionMetric][ExceptionMetric]

After a few minutes (around two), you should see your new metric populated.

> *Note*
> Your custom metric might take several minutes to appear in the list of available metrics.

![Results][Results]


[img1]: Media/img1.png
[img2]: Media/img2.png
[img3]: Media/img3.png
[img4]: Media/img4.png
[ExceptionMetric]: Media/ExceptionMetric.png "Add an exception Metric"
[Results]: Media/Results.png "Results"
