#if __TVOS__ || __MACOS__
using System;
using System.IO;
using System.Threading.Tasks;

namespace PCLStorage
{
	internal sealed class FileSystem
	{
		public static class Current
		{
			static Current()
			{
				var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				LocalStorage = new FileSystemFolder(documents);
			}

			public static FileSystemFolder LocalStorage { get; private set; }
		}
	}

	internal class FileSystemFolder
	{
		public FileSystemFolder(string path)
		{
			Path = path;
		}

		public string Path { get; private set; }

		public Task<ExistenceCheckResult> CheckExistsAsync(string name)
		{
			var path = System.IO.Path.Combine(Path, name);
			return Task.FromResult(Directory.Exists(path) ? ExistenceCheckResult.FolderExists : ExistenceCheckResult.NotFound);
		}

		public Task<FileSystemFolder> CreateFolderAsync(string name, CreationCollisionOption creationCollisionOption)
		{
			var newPath = System.IO.Path.Combine(Path, name);
			Directory.CreateDirectory(newPath);

			return Task.FromResult(new FileSystemFolder(newPath));
		}
	}

	internal enum ExistenceCheckResult
	{
		NotFound = 0,
		FolderExists = 2,
	}

	internal enum CreationCollisionOption
	{
		OpenIfExists = 3
	}
}
#endif
