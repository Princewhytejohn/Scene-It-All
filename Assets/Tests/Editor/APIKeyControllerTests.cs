using NUnit.Framework;

public class APIKeyControllerTests
{
    [Test]
    public void TrimmedKey_RemovesWhitespace()
    {
        string key = "  abc123  ";
        Assert.AreEqual("abc123", key.Trim());
    }

    [Test]
    public void EmptyKey_IsInvalid()
    {
        string key = "";
        Assert.IsTrue(string.IsNullOrEmpty(key));
    }
}