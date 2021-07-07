using System;
using System.Collections;
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
                var fileContentAttribute = fileContentProperty.GetCustomAttribute<FileContent>();
                var fileTemplate = fileContentAttribute.FileTemplate;
                var pathsToFiles = FileFinder.FindPaths(directory, fileTemplate, fileContentAttribute.Global);
                
                LoadPropertyValue(content, pathsToFiles, fileContentProperty, fileContentAttribute);
            }
        }

        private void LoadPropertyValue(
            TContent content,
            IEnumerable<string> pathsToFiles,
            PropertyInfo fileContentProperty,
            FileContent fileContentAttribute)
        {
            if (!fileContentProperty.CanWrite)
                throw new Exception($"Property '{fileContentProperty.Name}' of '{content.GetType().Name}' should has setter.");

            if (fileContentAttribute.Multiple)
            {
                LoadMultiplePropertyValue(content, pathsToFiles, fileContentProperty, fileContentAttribute);
                return;
            }

            LoadSinglePropertyValue(content, pathsToFiles, fileContentProperty, fileContentAttribute);
        }

        private void LoadSinglePropertyValue(TContent content, IEnumerable<string> pathsToFiles, PropertyInfo fileContentProperty,
            FileContent fileContentAttribute)
        {
            var path = pathsToFiles.FirstOrDefault();
            if (path == null && !fileContentAttribute.Optional)
                throw new FileNotFoundException($"Not found file marked as required with template '{fileContentAttribute.FileTemplate}'");

            if (path != null)
            {
                var value = LoadValueFromFile(content, path, fileContentProperty.PropertyType, fileContentProperty, fileContentAttribute);
                fileContentProperty.SetValue(content, value);
            }
        }

        private void LoadMultiplePropertyValue(TContent content, IEnumerable<string> pathsToFiles, PropertyInfo fileContentProperty,
            FileContent fileContentAttribute)
        {
            var genericTypeDefinition = fileContentProperty.PropertyType.GetGenericTypeDefinition();
            if (genericTypeDefinition != typeof(IReadOnlyCollection<>))
            {
                throw new InvalidOperationException($"Expected type IReadOnlyCollection<...> for property '{fileContentProperty.Name}' " +
                                                    $"marked as Multiple");
            }

            var genericType = fileContentProperty.PropertyType.GetGenericArguments().Single();
            var valuesList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericType));

            foreach (var path in pathsToFiles)
            {
                var value = LoadValueFromFile(content, path, genericType, fileContentProperty, fileContentAttribute);
                valuesList.Add(value);
            }
            
            if (valuesList.Count == 0 && !fileContentAttribute.Optional)
                throw new FileNotFoundException($"Not found any files marked as required " +
                                                $"with template '{fileContentAttribute.FileTemplate}'");

            var oldValue = fileContentProperty.GetValue(content);
            if (valuesList.Count != 0 || oldValue == null)
            {
                fileContentProperty.SetValue(content, valuesList);
            }
        }

        private object LoadValueFromFile(TContent content,
            string path,
            Type type,
            PropertyInfo fileContentProperty,
            FileContent fileContentAttribute)
        {
            if (fileContentAttribute.InjectPath)
            {
                if (type != typeof(string))
                    throw new InvalidOperationException($"Property '{fileContentProperty.Name}' of '{content.GetType().Name}' " +
                                                        $"should has type '{nameof(String)}' " +
                                                        $"but was '{fileContentProperty.PropertyType.Name}'");
                return path;
            }
            
            var fileStream = File.OpenRead(path);
            content.Resources.Add(fileStream);
            var deserializer = GetDeserializerFor(type);
            var value = deserializer.Invoke(fileStream);

            return value;
        }

        private Func<Stream, object> GetDeserializerFor(Type type)
        {
            if (!fileDeserializers.TryGetValue(type, out var deserializer))
                throw new InvalidOperationException($"Can't deserializer for type {type.FullName}, " +
                                                    $"use method {nameof(WithDeserializer)} to fix it");

            return deserializer;
        }
    }
}