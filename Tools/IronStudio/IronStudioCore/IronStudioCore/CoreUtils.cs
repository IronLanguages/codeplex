/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronStudio.Core {
    public static class CoreUtils {
        public static IContentType/*!*/ RegisterContentType(
            IContentTypeRegistryService/*!*/ contentTypeRegistryService, 
            IFileExtensionRegistryService/*!*/ fileExtensionRegistryService,
            string/*!*/ contentTypeName,
            IEnumerable<string>/*!*/ contentSubtypes,
            IEnumerable<string>/*!*/ fileExtensions
        ) {
            var existing = contentTypeRegistryService.GetContentType(contentTypeName);
            if (existing != null) {
                foreach (var extension in new List<string>(fileExtensionRegistryService.GetExtensionsForContentType(existing))) {
                    fileExtensionRegistryService.RemoveFileExtension(extension);
                }

                // our content type has been registered by VS (attribute ProvideLanguageService on Package),
                // but we want it to be a subtype of DlrContentType, so go ahead and remove it and re-register it ourselves.
                contentTypeRegistryService.RemoveContentType(contentTypeName);
            }

            var result = contentTypeRegistryService.AddContentType(contentTypeName, contentSubtypes);

            foreach (var extension in fileExtensions) {
                if (fileExtensionRegistryService.GetContentTypeForExtension(extension) == contentTypeRegistryService.UnknownContentType) {
                    fileExtensionRegistryService.AddFileExtension(extension, result);
                }
            }

            return result;
        }
    }
}
