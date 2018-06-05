
#r "Microsoft.ML.Api"
#r "Microsoft.ML.Core"
#r "Microsoft.ML.CpuMath"
#r "Microsoft.ML.Data"
#r "Microsoft.ML"
#r "Microsoft.ML.FastTree"
#r "Microsoft.ML.InternalStreams"
#r "Microsoft.ML.KMeansClustering"
#r "Microsoft.ML.Maml"
#r "Microsoft.ML.PCA"
#r "Microsoft.ML.PipelineInference"
#r "Microsoft.ML.ResultProcessor"
#r "Microsoft.ML.StandardLearners"
#r "Microsoft.ML.Sweeper"
#r "Microsoft.ML.Transforms"
#r "Microsoft.ML.UniversalModelFormat"

using Microsoft.ML.Runtime.Api;

public class SentimentData
{
    [Column("0")]
    public string SentimentText;

    [Column("1", name: "Label")]
    public float Sentiment;
}

public class SentimentPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Sentiment;
}

internal class TestSentimentData
{
    internal static readonly IEnumerable<SentimentData> Sentiments = new[]
    {
        new SentimentData
        {
            SentimentText = "Contoso's 11 is a wonderful experience",
            Sentiment = 0
        },
        new SentimentData
        {
            SentimentText = "The acting in this movie is very bad",
            Sentiment = 0
        },
        new SentimentData
        {
            SentimentText = "Joe versus the Volcano Coffee Company is a great film.",
            Sentiment = 0
        }
    };
}