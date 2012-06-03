//#r "System.ServiceModel"
//#r "System.ServiceModel.Web"
namespace WCFHost

open System
open System.Collections.Generic
open System.Diagnostics.Contracts
open System.Globalization
open System.Linq
open System.ServiceModel
open System.ServiceModel.Web
open System.Collections.Generic

type SessionId = string // Session
type ClientId = string // User name

/// WCF Service, Client -> Server -communication
[<ServiceContract(Name="PushService", Namespace="http://localhost/WCFHost/", CallbackContract=typeof<StreamClient>)>]
[<ServiceBehavior(Name="PushService", Namespace="http://localhost/WCFHost/", ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)>]
type PushService() =

    /// List of clients
    let clientCallbacks = new SynchronizedCollection<SessionId*ClientId*StreamClient>()

    /// Is the connection at required state?
    let filterOnes state (cb:StreamClient) =
        let channel:IContextChannel = cb :?> IContextChannel
        channel.State = state

    /// Send message to clients
    let notifyClients message clients =

        let informClients (cb:StreamClient) =
            let endstream res = 
                let disconnectOnError conn =
                    let item = clientCallbacks
                               |> Seq.toList 
                               |> List.tryFind(fun (_,_,c) -> c = conn)
                    match item with 
                    | Some(user) -> clientCallbacks.Remove(user)
                    | None -> false
                try
                    cb.EndReceiveStream res
                with 
                // Should we log or remove the client?
                | :? TimeoutException -> 
                    "Timeout error..." |> ignore
                    disconnectOnError cb |> ignore

                | :? CommunicationException -> 
                    "Communication error..." |> ignore
                    disconnectOnError cb |> ignore
                    
            Async.FromBeginEnd(cb.BeginReceiveStream(message), endstream)
            |> Async.Start

        clients
        |> Seq.map(fun (_,_,v) -> v)
        |> Seq.filter(filterOnes CommunicationState.Opened)
        |> Seq.iter(informClients)

    /// Security: After registration username comes from server, not with every client
    let fetchMyName() = 
        clientCallbacks
        |> Seq.filter (fun (s,n,_) -> s = OperationContext.Current.SessionId)
        |> Seq.map (fun (s,n,_) -> n)
        |> Seq.head

    /// End connection
    let disconnect user =
        try 
            clientCallbacks.Remove(user) |> ignore
            let _,name,_ = user
            clientCallbacks |> notifyClients (UserPart(name))
            clientCallbacks |> notifyClients (SystemMessage(name + " disconnected!"))
        with
        | :? CommunicationException -> "Communication error..." |> ignore

    /// Send message to all
    [<OperationContract(IsOneWay=true)>]
    member x.SendMessageToAll message = 
        let umsg = (UserMessage(fetchMyName(),message))
        notifyClients umsg clientCallbacks

    /// Send message to specific user
    [<OperationContract(IsOneWay=true)>]
    member x.SendMessageTo message target =
        let umsg = (UserMessage(fetchMyName(),message))                
        clientCallbacks
        |> Seq.filter(fun (_,t,_) -> t = target)
        |> notifyClients umsg

    /// User logins to observe messages
    [<OperationContract(IsOneWay=true)>]
    member x.RegisterToObserve myName =

        let context = OperationContext.Current
        let currClient = context.GetCallbackChannel<StreamClient>()
        let disconn (_:EventArgs) = disconnect (context.SessionId, myName, currClient)

        context.Channel.Faulted |> Observable.subscribe(disconn) |> ignore
        context.Channel.Closed |> Observable.subscribe(disconn) |> ignore

        // Send online users to current client:
        clientCallbacks 
        |> Seq.map (fun (_, n, _) -> n)
        |> currClient.ReceiveUserList

        // Add current user to users
        let registered = clientCallbacks
                            |> Seq.exists (fun (i, n, _) -> i = context.SessionId && n = myName)

        if not registered then 
            clientCallbacks.Add(context.SessionId, myName, currClient)

        // Notify join to all:
        clientCallbacks |> notifyClients (UserJoin(myName))
        clientCallbacks |> notifyClients (SystemMessage(myName + " joined!"))
