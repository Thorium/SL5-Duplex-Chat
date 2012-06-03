namespace WCFHost

open System
open System.ServiceModel

open Microsoft.FSharp.Reflection 
open System.Reflection 
open System.Runtime.Serialization 
open System.Xml

// Commands/messages that publisher can send to subscribers
// Another use case would be multiplayer game interfaces as discriminated union:
//type PushMessage =
//| ChatMessage of IChatMessage
//| GameState of IGameState
//| PlayerStats of IPlayerStats

/// Commands/messages that publisher can send to subscribers
[<KnownType("KnownTypes")>]
type PushMessage =
/// System level message
| SystemMessage of string
/// User message: Username and Message
| UserMessage of string*string
/// Join: Username 
| UserJoin of string
/// Leave: Username 
| UserPart of string
    /// WCF Serialization for discriminated union
    static member KnownTypes() = 
        typeof<PushMessage>.GetNestedTypes(BindingFlags.Public ||| BindingFlags.NonPublic) 
        |> Array.filter FSharpType.IsUnion

/// Contract for Server -> Client communication
[<ServiceContract(Name="StreamClient", Namespace="http://localhost/WCFHost/")>]
type StreamClient =
        
    /// Once, in login, get current online users
    [<OperationContract(IsOneWay=true)>]
    abstract member ReceiveUserList : users:string seq -> unit
        
    /// Send message (any of known type Message)
    [<OperationContract(IsOneWay=true, AsyncPattern=true)>]
    abstract member BeginReceiveStream : message:PushMessage -> cb:AsyncCallback*obj -> IAsyncResult
    abstract member EndReceiveStream : IAsyncResult -> unit