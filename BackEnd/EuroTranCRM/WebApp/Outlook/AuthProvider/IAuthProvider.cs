using System.Threading.Tasks;

/*
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
*  See LICENSE in the source repository root for complete license information.
*/

namespace WebApp.Outlook.AuthProvider
{
    public interface IAuthProvider
    {
        Task<string> GetAccessTokenAsync();

        Task<string> GetOutlookAccessTokenAsync();
    }
}