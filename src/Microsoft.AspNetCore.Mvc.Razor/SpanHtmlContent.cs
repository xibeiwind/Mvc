// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    internal unsafe class SpanHtmlContent : IHtmlContent
    {
        private readonly char* _value;
        private readonly int _length;

        public SpanHtmlContent(ReadOnlySpan<char> value)
        {
            fixed(char* p = &value[0])
            {
                _value = p;
                _length = value.Length;
            }
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (writer is HttpResponseStreamWriter responseWriter)
            {

            }
        }
    }
}