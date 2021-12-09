using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.JsonAdaptor;
using MFiles.VAF.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MFilesAPI;

namespace SmartFaceVaultApp
{
    [DataContract]
    public class Configuration
    {

        [MFPropertyDef(Required = true, Datatypes = new MFDataType[] { MFDataType.MFDatatypeMultiLineText })]
        [DataMember]
        public MFIdentifier EncodedFace { get; set; }


        [MFClass(Required = true)]
        [DataMember]
        public MFIdentifier MugShotClass { get; set; }

        [JsonConfEditor(IsRequired = true, HelpText = "Location for face recognition models on local drive.")]
        [DataMember]
        public string ModelLocation { get; set; }
    }
}