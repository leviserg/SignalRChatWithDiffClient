using MessagePack;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


var data = Test.GetTestData();

var results = new List<BenchmarkResult>();

for (int i = 0; i < 20; i++)
{
    results.Add(JsonVsMessagePackTest(data));
}

AnalyzeAndPrintResults(results);

Console.ReadLine();


static BenchmarkResult JsonVsMessagePackTest(IEnumerable<Test> data)
{
    var result = new BenchmarkResult();

    #region JSON serialization
    var sw = Stopwatch.StartNew();
    var json = JsonSerializer.Serialize(data);
    sw.Stop();

    var jsonBytes = Encoding.UTF8.GetBytes(json);
    result.JsonSize = jsonBytes.LongLength;
    result.JsonSerializeTime = sw.ElapsedMilliseconds;
    using var jsonStream = new MemoryStream(jsonBytes);
    using var compressedJson = new MemoryStream();
    using var brotliStream = new BrotliStream(compressedJson, CompressionLevel.Fastest);
    jsonStream.CopyTo(brotliStream);
    result.CompressedJsonSize = compressedJson.Length;
    #endregion

    #region MessagePack serialization
    sw.Restart();
    var messagePack = MessagePackSerializer.Serialize(data);
    sw.Stop();
    result.MessagePackSerializeTime = sw.ElapsedMilliseconds;
    result.MessagePackSize = messagePack.LongLength;
    using var messagePackStream = new MemoryStream(messagePack);
    using var compressedMessagePack = new MemoryStream();
    using var brotliMessagePack = new BrotliStream(compressedMessagePack, CompressionLevel.Fastest);
    messagePackStream.CopyTo(brotliMessagePack);
    result.CompressedMessagePackSize = compressedMessagePack.Length;
    #endregion

    #region JSON deserialization
    sw.Restart();
    var desJson = JsonSerializer.Deserialize<IEnumerable<Test>>(json);
    sw.Stop();
    result.JsonDeserializeTime = sw.ElapsedMilliseconds;
    #endregion

    #region MessagePack deserialization
    sw.Restart();
    var desMsgPack = MessagePackSerializer.Deserialize<IEnumerable<Test>>(messagePack);
    sw.Stop();
    result.MessagePackDeserializeTime = sw.ElapsedMilliseconds;
    #endregion

    return result;
}

static void AnalyzeAndPrintResults(IEnumerable<BenchmarkResult> results)
{
    var averageJsonSerializeSpeed = results.Select(x => x.JsonSerializeTime).Average();
    Console.WriteLine("Average serialize json: {0}ms", averageJsonSerializeSpeed);

    var averageJsonDeserializeSpeed = results.Select(x => x.JsonDeserializeTime).Average();
    Console.WriteLine("Average deserialize json: {0}ms", averageJsonDeserializeSpeed);
    Console.WriteLine("Max serialize json: {0}ms", results.Select(x => x.JsonSerializeTime).Max());
    Console.WriteLine("Max deserialize json: {0}ms", results.Select(x => x.JsonDeserializeTime).Max());
    Console.WriteLine("Min serialize json: {0}ms", results.Select(x => x.JsonSerializeTime).Min());
    Console.WriteLine("Min deserialize json: {0}ms", results.Select(x => x.JsonDeserializeTime).Min());

    var jsonSize = results.Select(x => x.JsonSize).Average();
    var compressedJsonSize = results.Select(x => x.CompressedJsonSize).Average();
    Console.WriteLine("Average Json bytes: {0}", jsonSize);
    Console.WriteLine("Average Json compressed bytes: {0}", compressedJsonSize);
    Console.WriteLine("-------------------------------------------------------------------------");

    var averageMessagePackSerializeSpeed = results.Select(x => x.MessagePackSerializeTime).Average();
    Console.WriteLine("Average serialize MessagePack: {0}ms", averageMessagePackSerializeSpeed);

    var averageMessagePackDeserializeSpeed = results.Select(x => x.MessagePackDeserializeTime).Average();
    Console.WriteLine("Average deserialize MessagePack: {0}ms", averageMessagePackDeserializeSpeed);
    Console.WriteLine("Max serialize MessagePack: {0}ms", results.Select(x => x.MessagePackSerializeTime).Max());
    Console.WriteLine("Max deserialize MessagePack: {0}ms", results.Select(x => x.MessagePackDeserializeTime).Max());
    Console.WriteLine("Min serialize MessagePack: {0}ms", results.Select(x => x.MessagePackSerializeTime).Min());
    Console.WriteLine("Min deserialize MessagePack: {0}ms", results.Select(x => x.MessagePackDeserializeTime).Min());

    var messagePackSize = results.Select(x => x.MessagePackSize).Average();
    var messagePackCompressedSize = results.Select(x => x.CompressedMessagePackSize).Average();
    Console.WriteLine("Average MessagePack bytes: {0}", messagePackSize);
    Console.WriteLine("Average MessagePack compressed bytes: {0}", messagePackCompressedSize);
    Console.WriteLine("-------------------------------------------------------------------------");


    Console.WriteLine("Difference size: MessagePack less JSON by {0} %", ((jsonSize - messagePackSize) / Math.Abs(messagePackSize)) * 100);
    Console.WriteLine("Difference compressed size: MessagePack less JSON by {0} %", ((compressedJsonSize - messagePackCompressedSize) / Math.Abs(messagePackCompressedSize)) * 100);
    Console.WriteLine("Difference serialize speed: MessagePack less JSON by {0} %", ((averageJsonSerializeSpeed - averageMessagePackSerializeSpeed) / Math.Abs(averageMessagePackSerializeSpeed)) * 100);
    Console.WriteLine("Difference deserialize speed: MessagePack less JSON by {0} %", ((averageJsonDeserializeSpeed - averageMessagePackDeserializeSpeed) / Math.Abs(averageMessagePackDeserializeSpeed)) * 100);
}

public class BenchmarkResult
{
    public long JsonSize { get; set; }
    public long MessagePackSize { get; set; }
    public long CompressedJsonSize { get; set; }
    public long CompressedMessagePackSize { get; set; }
    public long JsonSerializeTime { get; set; }
    public long MessagePackSerializeTime { get; set; }
    public long JsonDeserializeTime { get; set; }
    public long MessagePackDeserializeTime { get; set; }
}

[MessagePackObject]
public class Test
{
    [Key("n")]
    [JsonPropertyName("n")]
    public string Name { get; set; }
    [Key("k")]
    [JsonPropertyName("k")]
    public int Count { get; set; }
    [Key("o")]
    [JsonPropertyName("o")]
    public double Open { get; set; }
    [Key("h")]
    [JsonPropertyName("h")]
    public double High { get; set; }
    [Key("l")]
    [JsonPropertyName("l")]
    public double Low { get; set; }
    [Key("c")]
    [JsonPropertyName("c")]
    public double Close { get; set; }

    public static IEnumerable<Test> GetTestData()
    {
        Random r = new Random();

        return Enumerable.Range(0, 100_000)
            .Select(x => new Test
            {
                Name = Guid.NewGuid().ToString(),
                Count = r.Next(),
                Close = r.NextDouble(),
                High = r.NextDouble(),
                Low = r.NextDouble(),
                Open = r.NextDouble()
            });
    }
}