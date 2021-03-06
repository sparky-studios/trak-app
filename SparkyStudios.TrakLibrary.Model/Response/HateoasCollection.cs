﻿using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace SparkyStudios.TrakLibrary.Model.Response
{
    [ExcludeFromCodeCoverage]
    public class HateoasCollection<T> : HateoasResource where T : HateoasResource
    {
        [JsonProperty("_embedded")]
        public HateoasResources<T> Embedded { get; set; }
    }
}