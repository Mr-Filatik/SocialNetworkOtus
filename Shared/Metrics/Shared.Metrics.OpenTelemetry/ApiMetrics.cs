using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace SocialNetworkOtus.Shared.Metrics.OpenTelemetry
{
    public class ApiMetrics
    {
        public readonly Meter Meter;

        private readonly Counter<long> _requestCount;
        private readonly Counter<long> _responseSuccessCount;
        private readonly Counter<long> _responseErrorCount;
        private readonly Histogram<double> _requestDurationMilliseconds;

        public ApiMetrics(string serviceName)
        {
            Meter = new Meter(serviceName);
            _requestCount = Meter.CreateCounter<long>(
                name: "http_requests_total",
                description: "AAA");
            _responseSuccessCount = Meter.CreateCounter<long>(
                name: "http_requests_success_total",
                description: "AAA");
            _responseErrorCount = Meter.CreateCounter<long>(
                name: "http_requests_error_total",
                description: "AAA");
            _requestDurationMilliseconds = Meter.CreateHistogram<double>(
                name: "http_request_duration_milliseconds",
                unit: "ms",
                description: "Handler duration histogram");
        }

        #region Implement IQueueBrokerMetrics

        /// <inheritdoc/>
        public void AddRequest(
            string messageType,
            params (string key, string value)[] tags)
        {
            IncrementCounter(_requestCount, messageType, tags);
        }

        /// <inheritdoc/>
        public void AddResponseSuccess(
            string messageType,
            params (string key, string value)[] tags)
        {
            IncrementCounter(_responseSuccessCount, messageType, tags);
        }

        /// <inheritdoc/>
        public void AddResponseError(
            string messageType,
            params (string key, string value)[] tags)
        {
            IncrementCounter(_responseErrorCount, messageType, tags);
        }

        /// <inheritdoc/>
        public void RecordHandlerDuration(
            string messageType,
            double durationInSeconds,
            params (string key, string value)[] tags)
        {
            RecordHistogram(_requestDurationMilliseconds, messageType, durationInSeconds, tags);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper method for creating a list of tags (TagList) with the addition of a message type.
        /// </summary>
        /// <param name="messageType"> The type of message that will be added to the tags. </param>
        /// <param name="tags"> Additional arbitrary tags in the format (key, value). </param>
        /// <returns> List of tags with the message type added. </returns>
        private TagList BuildTagList(string messageType, params (string key, string value)[] tags)
        {
            var tagList = new TagList
            {
                { "message_type", messageType }
            };

            foreach (var tag in tags)
            {
                tagList.Add(tag.key, tag.value);
            }

            return tagList;
        }

        /// <summary>
        /// Method to increment a counter by one.
        /// </summary>
        /// <param name="counter"> Counter to increase. </param>
        /// <param name="messageType"> The type of message being processed. </param>
        /// <param name="tags"> Additional arbitrary tags in the format (key, value). </param>
        private void IncrementCounter(
            Counter<long> counter,
            string messageType,
            params (string key, string value)[] tags)
        {
            if (Meter != null)
            {
                var tagList = BuildTagList(messageType, tags);
                counter.Add(1, tagList);
            }
        }

        /// <summary>
        /// Method for writing a value to a histogram.
        /// </summary>
        /// <param name="histogram"> Histogram for recording the value. </param>
        /// <param name="messageType"> The type of message being processed. </param>
        /// <param name="value"> The value that will be written to the histogram. </param>
        /// <param name="tags"> Additional arbitrary tags in the format (key, value). </param>
        private void RecordHistogram(
            Histogram<double> histogram,
            string messageType,
            double value,
            params (string key, string value)[] tags)
        {
            if (Meter != null)
            {
                var tagList = BuildTagList(messageType, tags);
                histogram.Record(value, tagList);
            }
        }

        #endregion
    }
}
