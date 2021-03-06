﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SparkyStudios.TrakLibrary.Model.Response
{
    public abstract class HateoasResource
    {
        [JsonProperty("_links")]
        public IDictionary<string, HateoasLink> Links { get; set; }

        public Uri GetLink(string link)
        {
            return Links != null && Links.ContainsKey(link) ? Links[link].Href : null;
        }
    }
}