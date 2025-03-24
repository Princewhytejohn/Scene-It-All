using NUnit.Framework;

public class MetadataUtilsTests
{
    [Test]
    public void FormatMetadata_WithAllFields_ReturnsCorrectFormat()
    {
        var movie = new MovieResult
        {
            release_date = "2021-08-12",
            vote_average = 7.9f,
            vote_count = 1234
        };

        string year = movie.release_date.Substring(0, 4);
        string rating = movie.vote_average > 0 ? movie.vote_average.ToString("0.0") : "no rating";
        string voteCount = movie.vote_count > 0 ? $" ({movie.vote_count})" : "";

        string metadata = $"{year} | {rating}{voteCount}";
        Assert.AreEqual("2021 | 7.9 (1234)", metadata);
    }

    [Test]
    public void FormatMetadata_NoRatingOrVotes_ReturnsNoRating()
    {
        var movie = new MovieResult
        {
            release_date = "2020-01-01",
            vote_average = 0,
            vote_count = 0
        };

        string year = movie.release_date.Substring(0, 4);
        string rating = "no rating";

        string metadata = $"{year} | {rating}";
        Assert.AreEqual("2020 | no rating", metadata);
    }
}