// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ExpoHelpers
{
    /// <summary>
    /// Class used for de-serializing lists of items from Hololens Commander
    /// </summary>
    public class ConnectionInformation
    {
        /// <summary>
        /// The address of the device.
        /// </summary>
        public string Address;

        /// <summary>
        /// Descriptive text, typically a name or location, assigned to the device.
        /// </summary>
        public string Name;

        public string Id;

        public string UserName;

        public string Password;
    }
}
