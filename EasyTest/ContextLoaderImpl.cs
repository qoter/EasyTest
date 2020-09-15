using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EasyTest
{
    internal class ContextLoaderImpl<TContext> : IContextLoader<TContext> where TContext : TestContext, new()
    {
        private readonly Dictionary<Type, Func<Stream, object>> fileDeserializers = new Dictionary<Type, Func<Stream, object>>();

        public IContextLoader<TContext> WithDeserializer<T>(Func<Stream, T> deserializer)
        {
            fileDeserializers[typeof(T)] = stream => deserializer(stream);

            return this;
        }

        public TContext LoadFromDirectory(string directory)
        {
            var context = new TContext();
            foreach (var testFileProperty in context.GetType().GetProperties().Where(p => p.HasAttribute<TestFile>()))
            {
                if (!testFileProperty.CanWrite)
                    throw new Exception($"Property '{testFileProperty.Name}' of '{context.GetType().Name}' should has setter.");

                var testFileAttribute = testFileProperty.GetCustomAttribute<TestFile>();
                var fileTemplate = testFileAttribute.FileTemplate;
                var pathToFile = testFileAttribute.Global 
                    ? FileFinder.FindPathToFileGlobal(directory, fileTemplate) 
                    : FileFinder.FindPathToFile(directory, fileTemplate);

                if (pathToFile == null && !testFileAttribute.Optional)
                    throw new FileNotFoundException($"Not found file marked as required with template '{fileTemplate}'");

                if (pathToFile != null)
                {
                    LoadPropertyValue(context, pathToFile, testFileProperty, testFileAttribute);
                }
            }

            return context;
        }
        
        private void LoadPropertyValue(TContext context, string pathToFile, PropertyInfo testFileProperty, TestFile testFileAttribute)
        {
            if (testFileAttribute.InjectPath)
            {
                if (testFileProperty.PropertyType != typeof(string))
                    throw new InvalidOperationException($"Property '{testFileProperty.Name}' of '{context.GetType().Name}' " +
                                                        $"should has type '{nameof(String)}' but was '{testFileProperty.PropertyType.Name}'");

                testFileProperty.SetValue(context, pathToFile);
                return;
            }
			
            var fileStream = File.OpenRead(pathToFile);
            context.Resources.Add(fileStream);
            var deserializer = GetDeserializerFor(testFileProperty);
            var propertyValue = deserializer.Invoke(fileStream);
            testFileProperty.SetValue(context, propertyValue);
        }
        
        private Func<Stream, object> GetDeserializerFor(PropertyInfo propertyInfo)
        {
            var type = propertyInfo.PropertyType;

            if (!fileDeserializers.TryGetValue(type, out var deserializer))
                throw new InvalidOperationException($"Can't deserialize property '{propertyInfo.Name}' with type '{propertyInfo.PropertyType.Name}', " +
                                                    $"use method ${nameof(WithDeserializer)} to fix it");

            return deserializer;
        }
    }
}