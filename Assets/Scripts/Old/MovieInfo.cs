using System;
using System.Collections.Generic;
[Serializable]
public class MovieInfo
{
    public int id;
    public string title;
    public string poster_path;
    // Other fields like overview, release_date, etc.
}

[Serializable]
public class MovieSearchResults
{
    public List<MovieInfo> results;
}
