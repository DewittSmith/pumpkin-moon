using System;
using System.Collections.Generic;
using System.IO;
using PumpkinMoon.Loading.Files;
using PumpkinMoon.Loading.Loaders;
using PumpkinMoon.Loading.Namespace;

namespace PumpkinMoon.Loading
{
    public class DataLoader
    {
        public delegate void LazyCreatedDelegate(string path, string namespacedId, Type type);

        private readonly Dictionary<string, Lazy> lazies = new Dictionary<string, Lazy>();
        private readonly Dictionary<string, ILoader> loaders = new Dictionary<string, ILoader>();

        private readonly INamespacedIdProvider namespacedIdProvider;
        private readonly IDirectoryEnumerator directoryEnumerator;
        private readonly IFileReader fileReader;

        public event LazyCreatedDelegate LazyCreated;

        public DataLoader()
        {
            namespacedIdProvider = new DefaultNamespaceIdProvider();
            directoryEnumerator = new DefaultDirectoryEnumerator();
            fileReader = new DefaultFileReader();
        }

        public DataLoader(INamespacedIdProvider namespacedIdProvider = null,
            IDirectoryEnumerator directoryEnumerator = null,
            IFileReader fileReader = null)
        {
            this.namespacedIdProvider = namespacedIdProvider ?? new DefaultNamespaceIdProvider();
            this.directoryEnumerator = directoryEnumerator ?? new DefaultDirectoryEnumerator();
            this.fileReader = fileReader ?? new DefaultFileReader();
        }

        public void RegisterLoader<TLoader>(string extension) where TLoader : ILoader, new()
        {
            loaders[extension.ToLower()] = Activator.CreateInstance<TLoader>();
        }

        public T GetData<T>(string namespacedId)
        {
            if (lazies.TryGetValue(namespacedId, out Lazy lazy))
            {
                return lazy.GetValue<T>();
            }

            return default;
        }

        public void LoadEntry(string path)
        {
            FileEntry entry = new FileEntry(null, path);
            LoadEntry(entry);
        }

        public void LoadDirectory(string path, bool recursive = true)
        {
            LoadDirectory(path, path, recursive);
        }

        private void LoadDirectory(string root, string path, bool recursive)
        {
            foreach (string file in directoryEnumerator.EnumerateFiles(path))
            {
                FileEntry entry = new FileEntry(root, file);
                LoadEntry(entry);
            }

            if (!recursive)
            {
                return;
            }

            foreach (string directory in directoryEnumerator.EnumerateDirectories(path))
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                LoadDirectory(root, directory, recursive);
            }
        }

        private void LoadEntry(FileEntry fileEntry)
        {
            string extension = fileEntry.Extension;

            if (loaders.TryGetValue(extension, out ILoader loader))
            {
                AddLazy(fileEntry, loader);
            }
        }

        private void AddLazy(FileEntry fileEntry, ILoader loader)
        {
            Stream stream = fileReader.ReadFile(fileEntry.Path);

            if (stream == null)
            {
                return;
            }

            string namespacedId = namespacedIdProvider.GetNamespacedId(fileEntry.Path, fileEntry.Root, loader.Type);
            lazies[namespacedId] = loader.LazyLoad(namespacedId, stream);

            LazyCreated?.Invoke(fileEntry.Path, namespacedId, loader.Type);
        }
    }
}