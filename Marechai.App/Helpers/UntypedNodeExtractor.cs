using Microsoft.Kiota.Abstractions.Serialization;

namespace Marechai.App.Helpers;

/// <summary>
///     Helper class for extracting values from Kiota UntypedNode objects.
/// </summary>
public static class UntypedNodeExtractor
{
    /// <summary>
    ///     Extracts an integer value from an UntypedNode.
    /// </summary>
    /// <param name="node">The UntypedNode to extract from. Can be null.</param>
    /// <returns>The extracted integer value, or 0 if extraction fails.</returns>
    public static int ExtractInt(UntypedNode? node)
    {
        if(node == null) return 0;

        try
        {
            // Cast to UntypedInteger to access the Value property
            if(node is UntypedInteger intNode) return intNode.GetValue();

            // Fallback: try to parse ToString() result
            var stringValue = node.ToString();

            if(!string.IsNullOrWhiteSpace(stringValue) && int.TryParse(stringValue, out int result)) return result;

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    ///     Extracts a long value from an UntypedNode.
    /// </summary>
    /// <param name="node">The UntypedNode to extract from. Can be null.</param>
    /// <returns>The extracted long value, or 0 if extraction fails.</returns>
    public static long ExtractLong(UntypedNode? node)
    {
        if(node == null) return 0;

        try
        {
            if(node is UntypedInteger intNode) return intNode.GetValue();

            var stringValue = node.ToString();

            if(!string.IsNullOrWhiteSpace(stringValue) && long.TryParse(stringValue, out long result)) return result;

            return 0;
        }
        catch
        {
            return 0;
        }
    }
}