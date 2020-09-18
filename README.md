# EasyTest
A set of tools that helps write tests based on directories

# Statuses
![Build status](https://github.com/qoter/EasyTest/workflows/Main%20build%20and%20tests/badge.svg)
[![Nuget version](https://img.shields.io/nuget/v/EasyTest)](https://www.nuget.org/packages/EasyTest)

# Usage
Define 'TestContext' class
```cs
public class MyTestContext : EasyTest.TestContext
{
    [TestFile("document.xml")]
    public XDocument Xml { get; private set; }
    
    [TestFile("expected.txt")]
    public string Expected { get; private set; }
}
```

Use 'ContextLoader' with necessary deserializers
```cs
using var context = ContextLoader
    .For<MyTestContext>()
    .WithDeserializer(XDocument.Load)
    .WithDeserializer(s => new StreamReader(s).ReadToEnd())
    .LoadFromDirectory("/path/to/directory/with/files");
```
Now you can use context for asserts
```cs
var actual = MakeMagic(context.Xml);
Assert.AreEqual(context.Expected, actual);
```
