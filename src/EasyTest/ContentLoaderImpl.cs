using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EasyTest
{
    internal class ContentLoaderImpl<TContent> : IContentLoader<TContent> where TContent : TestContent, new()
    {
        private readonly Dictionary<Type, Func<Stream, object>> fileDeserializers = new Dictionary<Type, Func<Stream, object>>();

        public IContentLoader<TContent> WithDeserializer<T>(Func<Stream, T> deserializer)
        {
            fileDeserializers[typeof(T)] = stream => deserializer(stream);

            return this;
        }

        public TContent LoadFromDirectory(string directory)
        {
            var content = new TContent();
            try
            {
                InitializeContent(content, directory);
            }
            catch (Exception)
            {
                content.Dispose();
                throw;
            }


            return content;
        }

        private void InitializeContent(TContent content, string directory)
        {
            foreach (var fileContentProperty in content.GetType().GetProperties().Where(p => p.HasAttribute<FileContent>()))
            {
                if (!fileContentProperty.CanWrite)
                    throw new Exception($"Property '{fileContentProperty.Name}' of '{content.GetType().Name}' should has setter.");

                var fileContentAttribute = fileContentProperty.GetCustomAttribute<FileContent>();
                var fileTemplate = fileContentAttribute.FileTemplate;
                var pathToFile = fileContentAttribute.Global
                    ? FileFinder.FindPathToFileGlobal(directory, fileTemplate)
                    : FileFinder.FindPathToFile(directory, fileTemplate);

                if (pathToFile == null && !fileContentAttribute.Optional)
                    throw new FileNotFoundException($"Not found file marked as required with template '{fileTemplate}'");

                if (pathToFile != null)
                {
                    LoadPropertyValue(content, pathToFile, fileContentProperty, fileContentAttribute);
                }
            }
        }

        private void LoadPropertyValue(TContent content, string pathToFile, PropertyInfo fileContentProperty, FileContent fileContentAttribute)
        {
            if (fileContentAttribute.InjectPath)
            {
                if (fileContentProperty.PropertyType != typeof(string))
                    throw new InvalidOperationException($"Property '{fileContentProperty.Name}' of '{content.GetType().Name}' " +
                                                        $"should has type '{nameof(String)}' but was '{fileContentProperty.PropertyType.Name}'");

                fileContentProperty.SetValue(content, pathToFile);
                return;
            }
			
            var fileStream = File.OpenRead(pathToFile);
            content.Resources.Add(fileStream);
            var deserializer = GetDeserializerFor(fileContentProperty);
            var propertyValue = deserializer.Invoke(fileStream);
            fileContentProperty.SetValue(content, propertyValue);
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