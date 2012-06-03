namespace ClientLogics

open System
open System.ServiceModel
open System.ComponentModel
open System.Collections.ObjectModel
open DataClient.PushServiceClient
open System.Windows.Browser

/// Silverlight MVVM: View Model
// We could login to many servers (publishers) simultaneously, but for this software, only one is used.
type ClientViewModel() =

    let mutable wcfClient = None
    let mutable users = new ObservableCollection<string>()
    let chatlog = new ObservableCollection<string>()
    let mutable chatMsg = ""

    let mutable serverEndpoint = 
        let uri = HtmlPage.Document.DocumentUri.ToString()
        let wcf = uri.Substring(0, (uri.LastIndexOf '/'))
        wcf + "/PushService.svc"

    /// Sign-in client
    let login (this:ClientViewModel) (nick:obj) = 
        
        // Note: Login two times won't close the previous connection, so user will get
        // messages twice! Could be a feature. But not in this UI...

        // Clear UI
        //chatlog.Clear()
        //users.Clear()
        //this.TriggerPropertyChanged "WhoIsOnline"
        //this.TriggerPropertyChanged "Log"

        // Connect to server
        let address = new EndpointAddress(this.ServerAddress)
        let binding = new PollingDuplexHttpBinding()
        //let binding = new PollingDuplexHttpBinding(PollingDuplexHttpSecurityMode.Transport, Channels.PollingDuplexMode.MultipleMessagesPerPoll)
        let client = new PushServiceClient(binding, address)

        unbox<string>(nick) |> client.RegisterToObserveAsync 

        // Users online
        client.ReceiveUserListReceived 
        |> Observable.subscribe(
            fun r -> 
                users <- new ObservableCollection<string>(r.users)
                this.TriggerPropertyChanged "WhoIsOnline"
        ) |> ignore

        // Actual chat history, register to events
        client.ReceiveStreamReceived
        |> Observable.subscribe(
            fun response -> 
                let doUpdate msg evt =
                    chatlog.Add(msg)
                    this.TriggerPropertyChanged evt
                    
                match response.message with
                | :? PushMessage.SystemMessage as us -> (us.item, "Log") ||> doUpdate
                | :? PushMessage.UserMessage as um -> 
                            ("<" + um.item1 + "> " + um.item2, "Log") ||> doUpdate
                | :? PushMessage.UserJoin as uj -> 
                        users.Add(uj.item) |> ignore
                        this.TriggerPropertyChanged "WhoIsOnline"
                | :? PushMessage.UserPart as up -> 
                        users.Remove(up.item) |> ignore
                        this.TriggerPropertyChanged "WhoIsOnline"
                | _ -> failwith("unknown message")
        ) |> ignore

        wcfClient <- Some(client)
    ///Send message
    let send (this:ClientViewModel) (message:obj) = 
        match wcfClient with 
        |Some(cli) ->
            match cli.State with
            | CommunicationState.Opened ->
                unbox<string>(message)
                |> cli.SendMessageToAllAsync
                this.ChatMessage <- ""
            | CommunicationState.Faulted -> chatlog.Add("Connection closed.")
            | CommunicationState.Closed -> chatlog.Add("Connection closed.")
            | _ -> "Some more state handling..." |> ignore
        |None -> chatlog.Add("Not connected to server...") |> ignore
    
    // Silverlight ViewModel as usual:
    let event = new Event<_,_>()
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member x.PropertyChanged = event.Publish

    member x.TriggerPropertyChanged(name)=
        event.Trigger(x, new PropertyChangedEventArgs(name))

    member x.Login = new Commands.ChatCommand( login x )
    member x.Send = new Commands.ChatCommand( send x )
    member x.Log = chatlog
    member x.WhoIsOnline = users

    member x.ServerAddress 
        with get() = serverEndpoint
        and set t = 
                serverEndpoint <- t
                x.TriggerPropertyChanged "ServerAddress"

    member x.ChatMessage 
        with get() = chatMsg
        and set t = 
                chatMsg <- t
                x.TriggerPropertyChanged "ChatMessage"
