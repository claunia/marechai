using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Marechai.App.Services;

/// <summary>
///     Re-exports <see cref="Marechai.ApiClient.CompositeSerializationWriterFactory" /> for backward compatibility.
/// </summary>
public class CompositeSerializationWriterFactory : ApiClient.CompositeSerializationWriterFactory;