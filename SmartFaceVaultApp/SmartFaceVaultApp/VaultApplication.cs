using FaceRecognitionDotNet;
using MFiles.VAF;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFiles.VAF.Extensions.Dashboards;
using MFilesAPI;
using MFilesAPI.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace SmartFaceVaultApp
{
    /// <summary>
    /// The entry point for this Vault Application Framework application.
    /// </summary>
    /// <remarks>Examples and further information available on the developer portal: http://developer.m-files.com/. </remarks>
    public class VaultApplication
        : MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
    {

        [TaskQueue]
        public const string QueueId = "SmartFace.EncodeMugShots";
        public const string TaskTypeEncodePhotos = "EncodeMugShots";

        private FaceRecognition FaceRecognition;

        protected override void StartApplication()
        {
            try
            {
                if (Configuration.ModelLocation != null)
                {
                    this.FaceRecognition = FaceRecognition.Create(Configuration.ModelLocation);
                }
            } catch (Exception e)
            {
                SysUtils.ReportErrorToEventLog(e.Message);
            }
        }

        /// <summary>
        /// Reinitializes FaceRecognition instance when model location is changed.
        protected override void OnConfigurationUpdated(Configuration oldConfiguration, bool updateExternals)
        {
            try
            {
                if (Configuration.ModelLocation != null && Configuration.ModelLocation != oldConfiguration.ModelLocation)
                {
                    this.FaceRecognition = FaceRecognition.Create(Configuration.ModelLocation);
                }
            }
            catch (Exception e)
            {
                SysUtils.ReportErrorToEventLog(e.Message);
            }
        }

        [TaskProcessor(QueueId, TaskTypeEncodePhotos, TransactionMode = TransactionMode.Full, MaxRequeues = 1, MaxRetries = 1)]
        [ShowOnDashboard("Encode mugshot")]
        public void EncodeMugShotProcessor(ITaskProcessingJob<ObjVerExTaskDirective> job)
        {


            if (false == job.Directive.TryGetObjVerEx(job.Vault, out ObjVerEx obj))
                return;

            obj.Refresh();
            
            // Only encodes the first file
            obj.SaveProperty(Configuration.EncodedFace, MFDataType.MFDatatypeMultiLineText, EncodeMugShot(job.Vault, obj.Info.Files[1]));


        }

        /// <summary>
        /// Gets the metadata based on the input.
        /// </summary>
        /// <param name="vault">Vault.</param>
        /// <param name="file">ObjectFile containing the encodable photo.</param>
        /// <returns>Serialized encoding of the first face in the photo.</returns>
        private string EncodeMugShot(Vault vault, ObjectFile file)
        {
            using (var stream = file.OpenRead(vault))
            {
                using (Bitmap bitmap = new Bitmap(stream))
                {
                    using (var image = FaceRecognition.LoadImage(bitmap))
                    {
                        IEnumerable<Location> locations = this.FaceRecognition.FaceLocations(image);

                        var encodings = this.FaceRecognition.FaceEncodings(image, new[] { locations.ToList().First() });
                        double[] raw = encodings.First().GetRawEncoding();

                        encodings.ToList().ForEach(x => x.Dispose());

                        return JsonConvert.SerializeObject(raw);

                    }

                }
            }
        }


        [EventHandler(MFEventHandlerType.MFEventHandlerAfterCheckInChangesFinalize)]
        public void AfterCheckInChangesFinalizeMugShot(EventHandlerEnvironment env)
        {
            ObjVerEx obj = env.ObjVerEx;

            if (obj.Info.FilesCount == 0 || obj.Class != Configuration.MugShotClass) return; // only process object with files and correct class

            this.TaskManager.AddTask
            (
                env.Vault,
                QueueId,
                TaskTypeEncodePhotos,
                // Directives allow you to pass serializable data to and from the task.
                directive: new ObjVerExTaskDirective(env.ObjVerEx)
            );

        }
    }
}