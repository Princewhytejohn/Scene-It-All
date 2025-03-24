using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class UIManagerTests
{
    [Test]
    public void AddToRecentlyViewed_AddsUniqueMovie()
    {
        var movie = new MovieResult { id = 1, title = "Movie A" };
        UIManager.Instance.recentlyViewedMovies.Clear();

        UIManager.Instance.AddToRecentlyViewed(movie);
        UIManager.Instance.AddToRecentlyViewed(movie); // Duplicate

        Assert.AreEqual(1, UIManager.Instance.recentlyViewedMovies.Count);
    }

    [Test]
    public void GetApiKey_ReturnsDecodedValue()
    {
        string originalKey = "test_api_key_123";
        string encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(originalKey));
        PlayerPrefs.SetString("TMDB_API_KEY", encoded);

        string key = UIManager.Instance.GetApiKey();

        Assert.AreEqual(originalKey, key);
    }
}