using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenTelemetry.TraceListeners.Tests
{
    [TestFixture]
    public class OpenTelemetryTraceListenerTests
    {
        private List<Activity> _capturedActivities = new List<Activity>();

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Trace.Listeners.Clear();
            var listener = new OpenTelemetryTraceListener();
            Trace.Listeners.Add(listener);
            var activitySource = new ActivitySource("TestTelemetry.Logs");
            var activityListener = new ActivityListener
            {
                ActivityStopped = activity =>
                {
                    _capturedActivities.Add(activity);
                },
                ShouldListenTo = s => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) =>
                    ActivitySamplingResult.AllData
            };
            ActivitySource.AddActivityListener(activityListener);
        }

        [SetUp]
        public void Setup()
        {
            _capturedActivities.RemoveAll(activity => true);
        }

        [Test]
        public void TraceEvent_WithFormatAndArgs_CreatesActivityWithSpecificDisplayName()
        {
            // Arrange

            // Act
            Trace.TraceInformation("This is a test message with {0} arguments", "two");

            // Assert
            Assert.That(_capturedActivities.Count, Is.EqualTo(1));
            var capturedActivity = _capturedActivities[0];
            Assert.That(capturedActivity.DisplayName, Is.EqualTo("Log: testhost.exe"));
        }

        [Test]
        public void TraceEvent_WithFormatAndArgs_CreatesActivityWithFormattedMessage()
        {
            // Arrange

            // Act
            Trace.TraceInformation("This is a test message with {0} arguments", "two");

            // Assert
            Assert.That(_capturedActivities.Count, Is.EqualTo(1));
            var capturedActivity = _capturedActivities[0];
            Assert.That(capturedActivity.DisplayName, Is.EqualTo("Log: testhost.exe"));
            Assert.That(capturedActivity.Tags.First(t => t.Key == "message").Value, Is.EqualTo("This is a test message with two arguments"));
            Assert.That(capturedActivity.Tags.First(t => t.Key == "format").Value, Is.EqualTo("This is a test message with {0} arguments"));
            Assert.That(_capturedActivities[0].TagObjects.First(t => t.Key == "args").Value, Is.EqualTo(new object[] { "two" }));
        }

        [Test]
        public void Write_WithoutActivity_CreatesActivityWithMessage()
        {
            // Arrange
            var message = "This is a test message";

            // Act
            Trace.Write(message);

            // Assert
            var capturedActivity = _capturedActivities[0];
            Assert.That(_capturedActivities.Count, Is.EqualTo(1));
            Assert.That(capturedActivity.Tags.First(t => t.Key == "message").Value, Is.EqualTo("This is a test message"));
        }

        [Test]
        public void WriteLine_WithoutActivity_CreatesActivityWithMessage()
        {
            // Arrange
            var message = "This is a test message";

            // Act
            Trace.WriteLine(message);

            // Assert
            Assert.That(_capturedActivities.Count, Is.EqualTo(1));
            var capturedActivity = _capturedActivities[0];
            Assert.That(_capturedActivities.Count, Is.EqualTo(1));
            Assert.That(capturedActivity.Tags.First(t => t.Key == "message").Value, Is.EqualTo("This is a test message"));
        }
    }
}