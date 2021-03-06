namespace Microsoft.HpcAcm.Frontend.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.HpcAcm.Common.Dto;
    using Microsoft.HpcAcm.Common.Utilities;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    [Route("api/[controller]")]
    public class HeatmapController : Controller
    {
        private readonly CloudUtilities utilities;
        private readonly ILogger logger;

        public HeatmapController(CloudUtilities u, ILogger<HeatmapController> logger)
        {
            this.utilities = u;
            this.logger = logger;
        }

        // GET api/heatmap/values/cpu?lastNodeName=abc&count=5
        [HttpGet("values/{category}")]
        public async Task<Heatmap> GetValuesAsync(string category, [FromQuery] string lastNodeName, [FromQuery] int? count, CancellationToken token)
        {
            var partitionQuery = this.utilities.GetPartitionQueryString(this.utilities.MetricsValuesPartitionKey);

            var lastRegistrationKey = lastNodeName;
            var registrationEnd = this.utilities.MaxString;

            var registrationRangeQuery = this.utilities.GetRowKeyRangeString(lastRegistrationKey, registrationEnd);

            var q = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(
                    partitionQuery,
                    TableOperators.And,
                    registrationRangeQuery))
                .Select(new List<string>() { category })
                .Take(count);

            var metricsTable = this.utilities.GetMetricsTable();

            TableContinuationToken conToken = null;

            List<(string, Dictionary<string, double?>)> list = new List<(string, Dictionary<string, double?>)>();

            do
            {
                var result = await metricsTable.ExecuteQuerySegmentedAsync(q, conToken, null, null, token);

                list.AddRange(result.Results.Select(r =>
                {
                    if (r.Timestamp > DateTimeOffset.UtcNow - TimeSpan.FromSeconds(5.0)
                        && r.Properties.TryGetValue(category, out EntityProperty values))
                    {
                        try
                        {
                            return (r.RowKey, JsonConvert.DeserializeObject<Dictionary<string, double?>>(values.StringValue));
                        }
                        catch (JsonReaderException)
                        {
                            return (r.RowKey, new Dictionary<string, double?>());
                        }
                    }
                    else
                    {
                        return (r.RowKey, new Dictionary<string, double?>());
                    }
                }));

                conToken = result.ContinuationToken;
            }
            while (conToken != null);

            return new Heatmap()
            {
                Category = category,
                Values = list.ToDictionary(l => l.Item1, l => l.Item2),
            };
        }

        // GET api/heatmap/categories
        [HttpGet("categories")]
        public async Task<List<string>> GetCategoriesAsync(CancellationToken token)
        {
            var partitionQuery = this.utilities.GetPartitionQueryString(this.utilities.MetricsCategoriesPartitionKey);

            var q = new TableQuery<DynamicTableEntity>()
                .Where(partitionQuery)
                .Select(new List<string>());

            var metricsTable = this.utilities.GetMetricsTable();

            TableContinuationToken conToken = null;

            List<string> list = new List<string>();

            do
            {
                var result = await metricsTable.ExecuteQuerySegmentedAsync(q, conToken, null, null, token);

                list.AddRange(result.Results.Select(r => r.RowKey));

                conToken = result.ContinuationToken;
            }
            while (conToken != null);

            return list;
        }
    }
}
