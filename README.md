# EasyTest
A set of tools that helps write tests based on directories

# Statuses
[![Build status](https://img.shields.io/github/workflow/status/qoter/EasyTest/Main%20build%20and%20tests?label=build%20and%20tests)](https://github.com/qoter/EasyTest/actions)
[![Nuget version](https://img.shields.io/nuget/v/EasyTest)](https://www.nuget.org/packages/EasyTest)

# Usage
Define 'TestContext' class
```cs
public class MyTestContent : TestContent
{
    [FileContent("document.xml")]
    public XDocument Xml { get; private set; }
    
    [FileContent("expected.txt")]
    public string Expected { get; private set; }
}
```

Use 'ContextLoader' with necessary deserializers
```cs
using var content = ContentLoader
    .For<MyTestContext>()
    .WithDeserializer(XDocument.Load)
    .WithDeserializer(s => new StreamReader(s).ReadToEnd())
    .LoadFromDirectory("/path/to/directory/with/files");
```
Now you can use context for asserts
```cs
var actual = MakeMagic(context.Xml);
Assert.AreEqual(content.Expected, actual);
```
