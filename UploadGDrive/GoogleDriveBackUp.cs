using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace UploadGDrive
{
    internal class GoogleDriveBackUp
    {
        public void BackupFile(string filePath, ProgressBar progressBar)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, @".credentials\drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.Drive },
                    "user",
                    CancellationToken.None,
                    new FileDataStore("Books.ListMyLibrary")).Result;
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "BackupGDrive",
            });

            // Upload file
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath)
            };
            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(filePath,
                System.IO.FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
                request.ProgressChanged += (IUploadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case UploadStatus.Starting:
                            Console.WriteLine("Upload starting");
                            break;
                        case UploadStatus.Uploading:
                            Console.WriteLine("{0} bytes sent", progress.BytesSent);
                            progressBar.Invoke(new Action(() =>
                            {
                                progressBar.Value = (int)(progress.BytesSent * 100 / stream.Length);
                            }));
                            break;
                        case UploadStatus.Completed:
                            Console.WriteLine("Upload completed");
                            break;
                        case UploadStatus.Failed:
                            Console.WriteLine("Upload failed");
                            break;
                    }
                };
                request.Upload();
            }
        }
    }
}
