# EasyTest
Lightweight tools that helps write tests based on directories

# Statuses
[![Build status](https://img.shields.io/github/workflow/status/qoter/EasyTest/Main%20build%20and%20tests?label=build%20and%20tests)](https://github.com/qoter/EasyTest/actions)
[![Nuget version](https://img.shields.io/nuget/v/EasyTest)](https://www.nuget.org/packages/EasyTest)

# Usage

### Content loading
Define `TestContent` class
```cs
public class MyTestContent : TestContent
{
    [FileContent("document.xml")]
    public XDocument Xml { get; private set; }
    
    [FileContent("expected.txt")]
    public string Expected { get; private set; }
}
```

Use `ContentLoader` with necessary deserializers
```cs
using var content = ContentLoader
    .For<MyTestContent>()
    .WithDeserializer(XDocument.Load)
    .WithDeserializer(s => s.ReadString())
    .LoadFromDirectory("/path/to/directory/with/files");
```
Now you can use content for asserts
```cs
var actual = MakeMagic(content.Xml);
Assert.AreEqual(content.Expected, actual);
```

### Content verifying
You can save `expected` and `actual` in files with `ContentVerifier` (like Approval Tests)
```cs
ContentVerifier
    .UseDirectory(testDirectory)
    .SaveActualAs("actual.txt", s => s.WriteString(actual))
    .ReadExpectedAs("expected.txt", s => s.ReadString())
    .Verify(expected => Assert.Equal(actual, expected));
```

### Snapshot testing
Just create directory `__snapshots__` in root of your project, then call `MatchSnapshot` inside your test method:
```cs
Snapshoter.MatchSnapshot(actualString);
```
Snapshoter save snapshots as `__snapshots__/<caller method file name>/<caller method name>.snap`.  
We use [\[CallerMemberName\]](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute) and [\[CallerFilePath\]](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerfilepathattribute) attributes to resove path to snapshots directory and it's nessery to call `MatchSnapshot` strictly inside your test method.

### Code examples
[See example ContentLoader and ContentVerifier in code](https://github.com/qoter/EasyTest/blob/master/src/EasyTest.Tests/UsageExample.cs#L19) 

[See example Snapshoter in code](https://github.com/qoter/EasyTest/blob/master/src/EasyTest.Tests/SnapshoterUsageExample.cs#L21)

