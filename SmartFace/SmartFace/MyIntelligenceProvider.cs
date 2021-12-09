using MFiles.Extensibility.Framework.IntelligenceServices;
using MFiles.Extensibility.IntelligenceServices;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using FaceRecognitionDotNet;
using System.Text;
using MFiles.VAF.Common;
using Newtonsoft.Json;

namespace SmartFace
{
    /// <summary>
    /// Metadata Provider.
    /// </summary>
    public class MyIntelligenceProvider :
        MarshalByRefObject,
        IIntelligenceProvider
    {
        #region Default implementations (do not typically need to be edited)

        /// <summary>
        /// The metadata structure details for the provider.
        /// </summary>
        private IntelligenceServiceConfiguration<MyConfiguration> Configuration { get; set; }

        /// <summary>
        /// Instance name. 
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Returns the declaration part of the intelligence service configuration to M-Files.
        /// </summary>
        public IntelligenceServiceDeclaration Declaration => this.Configuration.Declaration;

        /// <summary>
        ///  Can this intelligence provider extract metadata?
        /// </summary>
        public bool CanExtractMetadata => true;

        private FaceRecognition FaceRecognition;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the provider instance.</param>
        /// <param name="configuration">The metadata structure details for the provider.</param>
        /// <param name="hasConfigurationErrors">True, if the validation found errors from the configuration.</param>
        public MyIntelligenceProvider(
            string name,
            IntelligenceServiceConfiguration<MyConfiguration> configuration,
            bool hasConfigurationErrors
        )
        {
            // Set instance name.
            this.Name = name;

            // Save configurations.
            this.Configuration = configuration;

            this.FaceRecognition = FaceRecognition.Create(this.Configuration.CustomConfiguration.ModelLocation);
        }

        #endregion

        /// <summary>
        /// Gets the metadata based on the input.
        /// </summary>
        /// <param name="operationContext">Provides contextual information for the request.</param>
        /// <param name="request">The metadata request details.</param>
        /// <returns>The resulting metadata.</returns>
        public MetadataResult ExtractMetadata(
            MFiles.Extensibility.Applications.IOperationContext operationContext,
            MetadataRequest request
        )
        {
            var result = new MetadataResult();

            // TODO: Populate the metadata result.

            //// The file data can be found in request.FileContents.
            //foreach( var file in request.FileContents )
            //{
            //	// The file contents (as a System.IO. Stream) are available in file.GetFileStream().
            //	// The file text (if available) is available in file.GetAsText().
            //}

            //// The existing property values for the object can be found in request.ExistingMetadata.
            //// Note: this is not populated when a document is first added, only if the "Analyse" button is later clicked,
            //// or the intelligence service is implicitly called via VaultAutomaticMetadataOperations.GetAutomaticMetadataForObject.
            //foreach( var propertyValue in request.ExistingMetadata.Cast<PropertyValue>() )
            //{
            //	System.Diagnostics.Debug.WriteLine( $"{propertyValue.PropertyDef} = {propertyValue.GetValueAsUnlocalizedText()}" );
            //}

            ////To suggest "Suggested value" as text for term "My Term", use this syntax:

            //result.Suggestions.Add( new TextValueMetadataSuggestion( MyTerms.MyTerm, "Suggested value", 0.5 ) );

            ////To suggest an item in a vault for the term "My Term", use this syntax:

            //var objId = new MFilesAPI.ObjID();
            //objId.SetIDs( (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument, 123 ); // Suggest document with ID 123
            //result.Suggestions.Add( new VaultReferenceMetadataSuggestion( MyTerms.MyTerm, objId, 0.5 ) );

            try
            {

                using (var image = FaceRecognition.LoadImage(new Bitmap(request.FileContents[0].GetFileStream())))
                {

                    var locations = this.FaceRecognition.FaceLocations(image);
                    List<ObjVerEx> mugShots = FindMugShots(operationContext.TransactionVault);

                    foreach (var location in locations)
                    {
                        var encodings = this.FaceRecognition.FaceEncodings(image, new[] { location });
                        foreach (FaceEncoding e in encodings)
                        {
                            FindMatches(e, mugShots).ForEach(match =>
                            {
                                result.Suggestions.Add(new VaultReferenceMetadataSuggestion(MyTerms.Face, match.person.ObjID, (1 - match.distanceBetWeenFaces) * 100));
                            });

                            e.Dispose();
                        }
                    }


                }
            } catch (Exception e)
            {
                
            }

            return result;

        }

        /// <summary>
        /// Gets all mugshot class objects with encoded face data and person object reference.
        /// </summary>
        /// <param name="vault">Transactional vault</param>
        /// <returns>Non-deleted mugshot objects</returns>
        private List<ObjVerEx> FindMugShots(Vault vault)
        {
            MFSearchBuilder sb = new MFSearchBuilder(vault);
            sb.Class(this.Configuration.CustomConfiguration.MugShotClass)
                .PropertyNot(this.Configuration.CustomConfiguration.EncodedFace, MFDataType.MFDatatypeMultiLineText, null)
                .PropertyNotMissing(this.Configuration.CustomConfiguration.EncodedFace)
                .PropertyNot(this.Configuration.CustomConfiguration.PersonReference, MFDataType.MFDatatypeMultiLineText, null)
                .PropertyNotMissing(this.Configuration.CustomConfiguration.PersonReference)
                .Deleted(false);

            return sb.FindEx();
        }

        /// <summary>
        /// Finds all matching persons by comparing face encoding with mugshots' encoded data.
        /// </summary>
        /// <param name="encoding">Encoding from the new object</param>
        /// <param name="mugShots">List of mugshot objects</param>
        /// <returns>List of Match objects including matching person and difference between the two compared faces.</returns>
        private List<Match> FindMatches(FaceEncoding encoding, List<ObjVerEx> mugShots)
        {
            List<Match> matches = new List<Match>();

            foreach(ObjVerEx mugShot in mugShots)
            {
                string encodeFaceString = mugShot.GetPropertyText(Configuration.CustomConfiguration.EncodedFace);
                double[] encodedFace = JsonConvert.DeserializeObject<double[]>(encodeFaceString);

                FaceEncoding mugshotEncoding = FaceRecognition.LoadFaceEncoding(encodedFace);
                double distance = FaceRecognition.FaceDistance(encoding, mugshotEncoding);

                if (distance < this.Configuration.CustomConfiguration.MatchTolerance && mugShot.HasValue(Configuration.CustomConfiguration.PersonReference))
                {

                    matches.Add(new Match() {
                        person = mugShot.GetDirectReference(Configuration.CustomConfiguration.PersonReference),
                        distanceBetWeenFaces = distance
                    });
                }
            }

            return matches;
        }

        public class Match
        {
            public ObjVerEx person;
            public double distanceBetWeenFaces;
        }

    }

}
