using System.IO;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Transfer;

namespace AwsTranscoder.Services
{
    /// <summary>
    /// Service that wraps the Amazon S3 Client library and any other "S3" related tasks.
    /// </summary>
    public class StorageService
    {
        private readonly TransferUtility _transferUtility;
        private readonly AmazonS3Client _s3Client;

        public StorageService()
        {
            _s3Client = new AmazonS3Client();
            _transferUtility = new TransferUtility(_s3Client);
        }

        /// <summary>
        /// Check whether or not the file exists in the S3 bucket
        /// </summary>
        /// <param name="fileName">The full filename</param>
        /// <param name="bucketName">The bucket name</param>
        /// <returns>Whether or not the file exists</returns>
        public bool FileExist(string fileName, string bucketName)
        {
            return new S3FileInfo(_s3Client, bucketName, fileName).Exists;
        }

        /// <summary>
        /// Uploads to file to an S3 bucket
        /// </summary>
        /// <param name="file">The file as a stream</param>
        /// <param name="fileName">The full filename</param>
        /// <param name="bucketName">The bucket name</param>
        public void UploadFile(Stream file, string fileName, string bucketName)
        {
            _transferUtility.Upload(file, bucketName , fileName);
        }

        /// <summary>
        /// Gets all the files in the S3 bucket
        /// </summary>
        /// <param name="bucketName">The bucket name</param>
        /// <returns>File array</returns>
        public S3FileInfo[] AllFiles(string bucketName)
        {
            return new S3DirectoryInfo(_s3Client, bucketName).GetFiles();
        }

        /// <summary>
        /// Gets all the files that start with a specific name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <param name="bucketName">The bucket name</param>
        /// <returns>File array</returns>
        public S3FileInfo[] FilesStartWith(string name, string bucketName)
        {
            return new S3DirectoryInfo(_s3Client, bucketName).GetFiles(name + "*", SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Removes all the files that start with a specific name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <param name="bucketName">The bucket name</param>
        public void RemoveFilesStartWith(string name, string bucketName)
        {
            var files = FilesStartWith(name, bucketName);
            foreach (var file in files)
                file.Delete();
        }
    }
}