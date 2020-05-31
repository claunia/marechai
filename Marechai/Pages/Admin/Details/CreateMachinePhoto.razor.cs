using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Blazor.FileReader;
using Blazorise;
using Marechai.Helpers;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Admin.Details
{
    public partial class CreateMachinePhoto
    {
        const int                     _maxUploadSize = 25 * 1048576;
        bool?                         _addToDatabase;
        bool                          _allFinished;
        AuthenticationState           _authState;
        bool?                         _convertAvif1440;
        bool?                         _convertAvif1440Th;
        bool?                         _convertAvif4k;
        bool?                         _convertAvif4kTh;
        bool?                         _convertAvifHd;
        bool?                         _convertAvifHdTh;
        bool?                         _convertHeif1440;
        bool?                         _convertHeif1440Th;
        bool?                         _convertHeif4k;
        bool?                         _convertHeif4kTh;
        bool?                         _convertHeifHd;
        bool?                         _convertHeifHdTh;
        bool?                         _convertJp2k1440;
        bool?                         _convertJp2k1440Th;
        bool?                         _convertJp2k4k;
        bool?                         _convertJp2k4kTh;
        bool?                         _convertJp2kHd;
        bool?                         _convertJp2kHdTh;
        bool?                         _convertJpeg1440;
        bool?                         _convertJpeg1440Th;
        bool?                         _convertJpeg4k;
        bool?                         _convertJpeg4kTh;
        bool?                         _convertJpegHd;
        bool?                         _convertJpegHdTh;
        bool?                         _convertWebp1440;
        bool?                         _convertWebp1440Th;
        bool?                         _convertWebp4k;
        bool?                         _convertWebp4kTh;
        bool?                         _convertWebpHd;
        bool?                         _convertWebpHdTh;
        bool?                         _extractExif;
        string                        _imageFormat;
        ElementReference              _inputUpload;
        int                           _licenseId;
        List<Database.Models.License> _licenses;
        bool                          _loaded;
        MachineViewModel              _machine;
        MachinePhotoViewModel         _model;
        bool?                         _moveFile;
        double                        _progressValue;
        string                        _sourceUrl;
        bool                          _unknownSource;
        bool                          _uploaded;
        bool                          _uploadError;
        string                        _uploadErrorMessage;
        bool                          _uploading;
        [Parameter]
        public Guid Id { get; set; }
        [Parameter]
        public int MachineId { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _licenses  = await LicensesService.GetAsync();
            _machine   = await MachinesService.GetAsync(MachineId);
            _authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            if(_machine is null)
                NavigationManager.ToBaseRelativePath("admin/machines");

            _loaded = true;

            StateHasChanged();
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        async Task UploadFile()
        {
            if(!_unknownSource &&
               string.IsNullOrWhiteSpace(_sourceUrl))
                return;

            if(_licenseId == 0)
            {
                _uploadError        = true;
                _uploadErrorMessage = L["Please choose a valid license."];

                return;
            }

            var processExiftool = new Process
            {
                StartInfo =
                {
                    FileName               = "exiftool", CreateNoWindow = true, RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            var processIdentify = new Process
            {
                StartInfo =
                {
                    FileName               = "identify", CreateNoWindow = true, RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            var processConvert = new Process
            {
                StartInfo =
                {
                    FileName               = "convert", CreateNoWindow = true, RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            string identifyOutput;
            string convertOutput;
            string exiftoolOutput;

            try
            {
                processIdentify.Start();
                identifyOutput = await processIdentify.StandardOutput.ReadToEndAsync();
                processIdentify.WaitForExit();
                processConvert.Start();
                convertOutput = await processConvert.StandardOutput.ReadToEndAsync();
                processConvert.WaitForExit();
                processExiftool.Start();
                exiftoolOutput = await processExiftool.StandardOutput.ReadToEndAsync();
                processExiftool.WaitForExit();
            }
            catch(Exception)
            {
                _uploadError        = true;
                _uploadErrorMessage = L["Cannot run ImageMagick please contact the administrator."];

                return;
            }

            IFileReference file = (await FileReaderService.CreateReference(_inputUpload).EnumerateFilesAsync()).
                FirstOrDefault();

            if(file is null)
                return;

            IFileInfo fileInfo = await file.ReadFileInfoAsync();

            if(fileInfo.Size > _maxUploadSize)
            {
                _uploadError        = true;
                _uploadErrorMessage = L["The selected file is too big."];

                return;
            }

            string tmpPath = Path.GetTempFileName();

            FileStream outFs;

            try
            {
                outFs = new FileStream(tmpPath, FileMode.Open, FileAccess.ReadWrite);
            }
            catch(Exception)
            {
                _uploadError        = true;
                _uploadErrorMessage = L["There was an error uploading the file."];

                return;
            }

            _uploading = true;

            await using AsyncDisposableStream fs     = await file.OpenReadAsync();
            byte[]                            buffer = new byte[20480];

            try
            {
                double lastProgress = 0;
                int    count;

                while((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await outFs.WriteAsync(buffer, 0, count);

                    double progress = ((double)fs.Position * 100) / fs.Length;

                    if(!(progress > lastProgress + 0.01))
                        continue;

                    _progressValue = progress;
                    await InvokeAsync(StateHasChanged);
                    await Task.Yield();
                    lastProgress = progress;
                }
            }
            catch(Exception)
            {
                _uploading          = false;
                _uploadError        = true;
                _uploadErrorMessage = L["There was an error uploading the file."];

                return;
            }

            outFs.Close();
            _uploading = false;
            await InvokeAsync(StateHasChanged);
            await Task.Yield();

            processIdentify = new Process
            {
                StartInfo =
                {
                    FileName               = "identify", CreateNoWindow = true, RedirectStandardError = true,
                    RedirectStandardOutput = true, ArgumentList =
                    {
                        tmpPath
                    }
                }
            };

            processIdentify.Start();
            identifyOutput = await processIdentify.StandardOutput.ReadToEndAsync();
            processIdentify.WaitForExit();

            if(processIdentify.ExitCode != 0 ||
               string.IsNullOrWhiteSpace(identifyOutput))
            {
                _uploading          = false;
                _uploadError        = true;
                _uploadErrorMessage = L["The uploaded file was not recognized as an image."];
                File.Delete(tmpPath);

                return;
            }

            string[] pieces = identifyOutput.Substring(tmpPath.Length).
                                             Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if(pieces.Length < 2)
            {
                _uploading          = false;
                _uploadError        = true;
                _uploadErrorMessage = L["The uploaded file was not recognized as an image."];
                File.Delete(tmpPath);

                return;
            }

            // TODO: Move this to Helpers, keep progress

            string extension = ImageMagick.GetExtension(pieces[0]);

            if(string.IsNullOrWhiteSpace(extension))
            {
                _uploading          = false;
                _uploadError        = true;
                _uploadErrorMessage = L["The uploaded file was not recognized as an image."];
                File.Delete(tmpPath);

                return;
            }

            _imageFormat = pieces[0];
            _uploaded    = true;

            _model = new MachinePhotoViewModel
            {
                UserId = (await UserManager.GetUserAsync(_authState.User)).Id, MachineId = MachineId,
                Id     = Guid.NewGuid(), OriginalExtension = extension, UploadDate = DateTime.UtcNow,
                Source = _unknownSource ? null : _sourceUrl, LicenseId = _licenseId
            };

            try
            {
                processExiftool = new Process
                {
                    StartInfo =
                    {
                        FileName               = "exiftool", CreateNoWindow = true, RedirectStandardError = true,
                        RedirectStandardOutput = true, ArgumentList =
                        {
                            "-n", "-json", tmpPath
                        }
                    }
                };

                processExiftool.Start();
                exiftoolOutput = await processExiftool.StandardOutput.ReadToEndAsync();
                processExiftool.WaitForExit();

                Exif[] exif = JsonSerializer.Deserialize<Exif[]>(exiftoolOutput);

                if(exif?.Length >= 1)
                    exif[0].ToViewModel(_model);

                _extractExif = true;
            }
            catch(Exception)
            {
                _extractExif = false;
            }

            string originalFilePath = Path.Combine(Host.WebRootPath, "assets", "photos", "machines", "originals",
                                                   $"{_model.Id}{_model.OriginalExtension}");

            try
            {
                File.Move(tmpPath, originalFilePath);
                _moveFile = true;
            }
            catch(Exception)
            {
                _moveFile = false;
                File.Delete(tmpPath);

                return;
            }

            await Task.Yield();
            await InvokeAsync(StateHasChanged);

            var photos = new Photos();
            photos.FinishedAll                        += OnFinishedAll;
            photos.FinishedRenderingJpeg4k            += OnFinishedRenderingJpeg4k;
            photos.FinishedRenderingJpeg1440          += OnFinishedRenderingJpeg1440;
            photos.FinishedRenderingJpegHd            += OnFinishedRenderingJpegHd;
            photos.FinishedRenderingJpeg4kThumbnail   += OnFinishedRenderingJpeg4kThumbnail;
            photos.FinishedRenderingJpeg1440Thumbnail += OnFinishedRenderingJpeg1440Thumbnail;
            photos.FinishedRenderingJpegHdThumbnail   += OnFinishedRenderingJpegHdThumbnail;
            photos.FinishedRenderingJp2k4k            += OnFinishedRenderingJp2k4k;
            photos.FinishedRenderingJp2k1440          += OnFinishedRenderingJp2k1440;
            photos.FinishedRenderingJp2kHd            += OnFinishedRenderingJp2kHd;
            photos.FinishedRenderingJp2k4kThumbnail   += OnFinishedRenderingJp2k4kThumbnail;
            photos.FinishedRenderingJp2k1440Thumbnail += OnFinishedRenderingJp2k1440Thumbnail;
            photos.FinishedRenderingJp2kHdThumbnail   += OnFinishedRenderingJp2kHdThumbnail;
            photos.FinishedRenderingWebp4k            += OnFinishedRenderingWebp4k;
            photos.FinishedRenderingWebp1440          += OnFinishedRenderingWebp1440;
            photos.FinishedRenderingWebpHd            += OnFinishedRenderingWebpHd;
            photos.FinishedRenderingWebp4kThumbnail   += OnFinishedRenderingWebp4kThumbnail;
            photos.FinishedRenderingWebp1440Thumbnail += OnFinishedRenderingWebp1440Thumbnail;
            photos.FinishedRenderingWebpHdThumbnail   += OnFinishedRenderingWebpHdThumbnail;
            photos.FinishedRenderingHeif4k            += OnFinishedRenderingHeif4k;
            photos.FinishedRenderingHeif1440          += OnFinishedRenderingHeif1440;
            photos.FinishedRenderingHeifHd            += OnFinishedRenderingHeifHd;
            photos.FinishedRenderingHeif4kThumbnail   += OnFinishedRenderingHeif4kThumbnail;
            photos.FinishedRenderingHeif1440Thumbnail += OnFinishedRenderingHeif1440Thumbnail;
            photos.FinishedRenderingHeifHdThumbnail   += OnFinishedRenderingHeifHdThumbnail;
            photos.FinishedRenderingAvif4k            += OnFinishedRenderingAvif4k;
            photos.FinishedRenderingAvif1440          += OnFinishedRenderingAvif1440;
            photos.FinishedRenderingAvifHd            += OnFinishedRenderingAvifHd;
            photos.FinishedRenderingAvif4kThumbnail   += OnFinishedRenderingAvif4kThumbnail;
            photos.FinishedRenderingAvif1440Thumbnail += OnFinishedRenderingAvif1440Thumbnail;
            photos.FinishedRenderingAvifHdThumbnail   += OnFinishedRenderingAvifHdThumbnail;

            #pragma warning disable 4014
            Task.Run(() => photos.ConversionWorker(Host.WebRootPath, _model.Id, originalFilePath, _imageFormat));
            #pragma warning restore 4014
        }

        async Task OnFinishedRenderingJpeg4k(bool result)
        {
            _convertJpeg4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpeg1440(bool result)
        {
            _convertJpeg1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpegHd(bool result)
        {
            _convertJpegHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpeg4kThumbnail(bool result)
        {
            _convertJpeg4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpeg1440Thumbnail(bool result)
        {
            _convertJpeg1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpegHdThumbnail(bool result)
        {
            _convertJpegHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2k4k(bool result)
        {
            _convertJp2k4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2k1440(bool result)
        {
            _convertJp2k1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2kHd(bool result)
        {
            _convertJp2kHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2k4kThumbnail(bool result)
        {
            _convertJp2k4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2k1440Thumbnail(bool result)
        {
            _convertJp2k1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2kHdThumbnail(bool result)
        {
            _convertJp2kHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebp4k(bool result)
        {
            _convertWebp4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebp1440(bool result)
        {
            _convertWebp1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebpHd(bool result)
        {
            _convertWebpHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebp4kThumbnail(bool result)
        {
            _convertWebp4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebp1440Thumbnail(bool result)
        {
            _convertWebp1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebpHdThumbnail(bool result)
        {
            _convertWebpHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeif4k(bool result)
        {
            _convertHeif4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeif1440(bool result)
        {
            _convertHeif1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeifHd(bool result)
        {
            _convertHeifHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeif4kThumbnail(bool result)
        {
            _convertHeif4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeif1440Thumbnail(bool result)
        {
            _convertHeif1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeifHdThumbnail(bool result)
        {
            _convertHeifHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvif4k(bool result)
        {
            _convertAvif4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvif1440(bool result)
        {
            _convertAvif1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvifHd(bool result)
        {
            _convertAvifHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvif4kThumbnail(bool result)
        {
            _convertAvif4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvif1440Thumbnail(bool result)
        {
            _convertAvif1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvifHdThumbnail(bool result)
        {
            _convertAvifHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedAll(bool result)
        {
            try
            {
                await Service.CreateAsync(_model);
                _addToDatabase = true;
            }
            catch(Exception e)
            {
                _addToDatabase = false;
                Console.WriteLine(e);

                throw;
            }

            _allFinished = true;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        void ValidateSource(ValidatorEventArgs e) =>
            Validators.ValidateUrl(e, L["Source URL must be smaller than 255 characters."], 255);
    }
}