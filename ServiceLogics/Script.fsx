// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// By default script files are not be part of the project build.

// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "System.ServiceModel.dll"
#r "System.Runtime.Serialization.dll"

open System
open System.ServiceModel

open Microsoft.FSharp.Reflection 
open System.Reflection 
open System.Runtime.Serialization 
open System.Xml

#load "StreamClient.fs"

#r "System.ServiceModel.Web.dll"
open WCFHost

open System.Collections.Generic
open System.Diagnostics.Contracts
open System.Globalization
open System.Linq
open System.ServiceModel.Web
open System.Collections.Generic

#load "PushService.svc.fs"
open WCFHost