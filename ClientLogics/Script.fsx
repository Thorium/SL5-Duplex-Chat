// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

open System
open System.Windows.Input

#load "Commands.fs"
open Commands

#r "bin/Debug/DataClient.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\Silverlight\v5.0\System.ServiceModel.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\Silverlight\v5.0\System.Runtime.Serialization.dll"
#r @"C:\Program Files (x86)\Microsoft SDKs\Silverlight\v5.0\Libraries\Client\System.ServiceModel.PollingDuplex.dll"
#r @"C:\Program Files (x86)\Microsoft SDKs\Silverlight\v5.0\Libraries\Client\System.ServiceModel.Extensions.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\Silverlight\v5.0\System.Windows.Browser.dll"

open System.ServiceModel
open System.ComponentModel
open System.Collections.ObjectModel
open DataClient.PushServiceClient
open System.Windows.Browser

#load "ClientViewModel.fs"
