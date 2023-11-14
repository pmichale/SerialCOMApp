using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace SerialComApp.Services
{
    internal class FileService : IFileService
    {
        private readonly Window _target;

        public FileService(Window target)
        {
            if (target == null) throw new ArgumentNullException("Target is null.");
            _target = target;
        }

        public async Task<IStorageFile?> OpenFileAsync()
        {
            var file = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open File",
                AllowMultiple = false
            });

            return file.Count >= 1 ? file[0] : null;
        }

        public async Task<IStorageFile?> SaveFileAsync()
        {
            return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "Save File"
            });
        }
    }
}
