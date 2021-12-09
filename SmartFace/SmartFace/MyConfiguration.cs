using MFiles.VAF.Configuration;
using System.Runtime.Serialization;

namespace SmartFace
{
    /// <summary>
    /// A blank configuration class.
    /// </summary>
    /// <remarks>
    /// Members added here will be shown in the "Configurations" area of the vault when using the M-Files Admin tool.
    /// </remarks>
    [DataContract]
    public class MyConfiguration
    {
        //[DataMember]
        //[TextEditor]
        //public string MyConfigurationValue { get; set; }

        //[DataMember]
        //[MFObjType]
        //public MFIdentifier MyObjectType { get; set; }

        [MFClass(Required = true)]
        [DataMember]
        public MFIdentifier MugShotClass { get; set; }


        [MFPropertyDef(Required = true)]
        [DataMember]
        public MFIdentifier EncodedFace { get; set; }

        [MFPropertyDef(Required = true)]
        [DataMember]
        public MFIdentifier PersonReference { get; set; }


        [JsonConfEditor(IsRequired = true, HelpText = "Location for face recognition models on local drive.")]
        [DataMember]
        public string ModelLocation { get; set; }


        [JsonConfEditor(IsRequired = true, HelpText = "Tolerance for approved match, 0 means faces are exactly the same.", DefaultValue = 0.6)]
        [DataMember]
        public double MatchTolerance { get; set; } = 0.6;

    }
}
