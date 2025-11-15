using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Marechai.App.Services;

public class CompositeSerializationWriterFactory : ISerializationWriterFactory
{
    // Internal list of registered factories.
    private readonly List<ISerializationWriterFactory> _factories = [];

    // This method loops through each registered factory and returns the first one that supports the content type.
    public ISerializationWriter GetSerializationWriter(string contentType)
    {
        foreach(ISerializationWriterFactory factory in _factories)
        {
            try
            {
                ISerializationWriter? writer = factory.GetSerializationWriter(contentType);

                if(writer != null) return writer;
            }
            catch(ArgumentOutOfRangeException)
            {
                // This factory doesn't support the content type; try the next.
            }
        }

        throw new ArgumentOutOfRangeException(nameof(contentType),
                                              $"No serialization writer is registered for content type: {contentType}");
    }

    /// <inheritdoc />
    public string ValidContentType => string.Join(";", _factories.Select(f => f.ValidContentType));

    // Add a new factory to the composite.
    public void AddFactory(ISerializationWriterFactory writerFactory)
    {
        if(writerFactory == null) throw new ArgumentNullException(nameof(writerFactory));
        _factories.Add(writerFactory);
    }
}