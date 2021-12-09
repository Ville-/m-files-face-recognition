
namespace SmartFace
{
    /// <summary>
    /// Terms are used when returning suggested properties.
    /// This class is simply used to declare the terms as constants, rather than
    /// having strings manually typed everywhere.
    /// </summary>
    internal static class MyTerms
    {
        // Terms are used when returning suggested properties.
        // TODO: Add terms as constants here.
        //internal const string MyTerm = "My Term";

        internal const string Face = "Face";

        public static string[] All =
        {
			// TODO: Return all the available terms here, so they're added to the suggestions in the editor.
			//MyTerms.MyTerm
            MyTerms.Face
		};
    }
}
