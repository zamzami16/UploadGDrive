using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UploadGDrive
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void backupButton_Click(object sender, EventArgs e)
        {
            var filePath = @"C:\Users\yusuf\Downloads\11_Axata.axt.gz";
            BackupFile(filePath);
        }

        private delegate void UpdateProgressDelegate(int progress);

        private void UpdateProgress(int progress)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new UpdateProgressDelegate(this.UpdateProgress), progress);
            }
            else
            {
                progressBar1.Value = progress;
            }
        }

        // TODO: 1. Ambil folder di drive dengan nama 'AxataBackup'
        // TODO: 2. Jika belum ada, buat dulu
        // TODO: 3. Upload file di folder ini
        public void BackupFile(string filePath)
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
            var uploadStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
            var request = service.Files.Create(fileMetadata, uploadStream, "application/octet-stream");
            request.Fields = "id";
            request.ChunkSize = ResumableUpload.MinimumChunkSize;
            request.ProgressChanged += (IUploadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case UploadStatus.Starting:
                        Console.WriteLine("Upload starting");
                        break;
                    case UploadStatus.Uploading:
                        Console.WriteLine("{0} bytes sent", progress.BytesSent);
                        UpdateProgress((int)(progress.BytesSent * 100 / uploadStream.Length));
                        break;
                    case UploadStatus.Completed:
                        Console.WriteLine("Upload completed");
                        UpdateProgress(100);
                        break;
                    case UploadStatus.Failed:
                        Console.WriteLine("Upload failed");
                        break;
                }
            };
            request.UploadAsync();
        }
    }
}
